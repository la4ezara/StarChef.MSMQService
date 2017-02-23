using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml;
using log4net;
using StarChef.Listener.Exceptions;
using StarChef.Orchestrate.Models.TransferObjects;
using StarChef.Listener.Extensions;

namespace StarChef.Listener.Commands.Impl
{
    internal class DatabaseCommands : IDatabaseCommands
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IConnectionStringProvider _csProvider;

        public DatabaseCommands(IConnectionStringProvider csProvider)
        {
            _csProvider = csProvider;
        }

        /// <exception cref="ConnectionStringNotFoundException">Login connection string is not found</exception>
        /// <exception cref="DatabaseException">Error is occurred while saving data to database</exception>
        public async Task RecordMessagingEvent(string trackingId, bool isSuccessful, string code, string details = null, string payloadJson = null)
        {
            var loginDbConnectionString = await _csProvider.GetLoginDb();
            if (string.IsNullOrEmpty(loginDbConnectionString))
                throw new ConnectionStringNotFoundException("Login connection string is not found");

            await Exec(loginDbConnectionString, "sc_orchestration_record_messaging_event", p =>
            {
                p.AddWithValue("@tracking_id", trackingId);
                p.AddWithValue("@is_successful", isSuccessful);
                p.AddWithValue("@code", code);
                if (!string.IsNullOrEmpty(details))
                    p.AddWithValue("@description", details);
                if (!string.IsNullOrEmpty(payloadJson))
                    p.AddWithValue("@payload", payloadJson);
            });
        }

        /// <exception cref="ConnectionStringNotFoundException">Some connection string is not found</exception>
        /// <exception cref="DatabaseException">Error is occurred while saving data to database</exception>
        /// <exception cref="ConnectionStringLookupException">Error is occurred while getting a customer DB</exception>
        public async Task SavePriceBandData(Guid organisationId, XmlDocument xmlDoc)
        {
            var loginDbConnectionString = await _csProvider.GetLoginDb();
            if (string.IsNullOrEmpty(loginDbConnectionString))
                throw new ConnectionStringNotFoundException("Login connection string is not found");

            var connectionString = await _csProvider.GetCustomerDb(organisationId, loginDbConnectionString);
            if (string.IsNullOrEmpty(connectionString))
                throw new ConnectionStringNotFoundException("Customer DB connection string is not found");

            await Exec(connectionString, "sc_save_product_price_band_list", p => { p.Add("@PriceBandListXml", SqlDbType.Xml).Value = xmlDoc.InnerXml; });
        }

        /// <exception cref="ConnectionStringNotFoundException">Some connection string is not found</exception>
        /// <exception cref="DatabaseException">Error is occurred while saving data to database</exception>
        public async Task UpdateExternalId(AccountCreatedTransferObject user)
        {
            var loginDbConnectionString = await _csProvider.GetLoginDb();
                if (string.IsNullOrEmpty(loginDbConnectionString))
                    throw new ConnectionStringNotFoundException("Login DB connection string is not found");

                await Exec(loginDbConnectionString, "sc_orchestration_update_login_external_id", p =>
                {
                    p.AddWithValue("@login_id", user.LoginId);
                    p.AddWithValue("@external_login_id", user.ExternalLoginId);
                });
        }

        /// <exception cref="ConnectionStringNotFoundException">Some connection string is not found</exception>
        /// <exception cref="DatabaseException">Error is occurred while saving data to database</exception>
        /// <exception cref="ListenerException">Exception in general logic of the listener</exception>
        /// <exception cref="ConnectionStringLookupException">Error is occurred while getting a customer DB</exception>
        public async Task UpdateUser(string externalLoginId, string username, string firstName, string lastName, string emailAddress)
        {
            var loginDbConnectionString = await _csProvider.GetLoginDb();
            if (string.IsNullOrEmpty(loginDbConnectionString))
                throw new ConnectionStringNotFoundException("Login DB connection string is not found");

            var ids = await GetLoginUserId(loginDbConnectionString, externalLoginId: externalLoginId);
            if (ids == null)
                throw new ListenerException("Cannot map external account to the StarChef account");
            var loginId = ids.Item1;
            var userId = ids.Item2;

            var connectionString = await _csProvider.GetCustomerDb(loginId, loginDbConnectionString);
            if (string.IsNullOrEmpty(connectionString))
                throw new ConnectionStringNotFoundException("Customer DB connections string is not found");
            
            await Exec(loginDbConnectionString, "sc_orchestration_update_user", p =>
            {
                p.AddWithValue("@login_id", loginId);
                p.AddWithValue("@login_name", username);
            });
            await Exec(connectionString, "sc_orchestration_update_user", p =>
            {
                p.AddWithValue("@userId", userId);
                p.AddWithValue("@email", emailAddress);
                p.AddWithValue("@login_name", username);
                p.AddWithValue("@forename", firstName);
                p.AddWithValue("@lastname", lastName);
            });            
        }

        /// <exception cref="DatabaseException">Database operation is failed</exception>
        /// <exception cref="ConnectionStringNotFoundException">Customer DB connections string is not found</exception>
        /// <exception cref="ListenerException">Cannot map external account to the StarChef account</exception>
        /// <exception cref="ConnectionStringLookupException">Error is occurred while getting a customer DB</exception>
        public async Task DisableLogin(int? loginId = default(int?), string externalLoginId = null)
        {
            var loginDbConnectionString = await _csProvider.GetLoginDb();
            if (string.IsNullOrEmpty(loginDbConnectionString))
                throw new ConnectionStringNotFoundException("Login DB connection string is not found");

            var ids = await GetLoginUserId(loginDbConnectionString, loginId, externalLoginId);
            if (ids == null)
                throw new ListenerException("Cannot map external account to the StarChef account");
            var existingLoginId = ids.Item1;
            var existingUserId = ids.Item2;

            var connectionString = await _csProvider.GetCustomerDb(existingLoginId, loginDbConnectionString);
            if (string.IsNullOrEmpty(connectionString))
                throw new ConnectionStringNotFoundException("Customer DB connections string is not found");

            await Exec(loginDbConnectionString, "sc_orchestration_disable_user", p => p.AddWithValue("@login_id", existingLoginId));
            await Exec(connectionString, "sc_orchestration_disable_user", p => p.AddWithValue("@user_id", existingUserId));
        }

        /// <exception cref="DatabaseException">Database operation is failed</exception>
        public async Task<Tuple<int, int, string>> GetLoginUserIdAndCustomerDb(int loginId)
        {
            var loginDbConnectionString = await _csProvider.GetLoginDb();

            var result = await UseReader(loginDbConnectionString, "sc_database_GetByLoginId",
                parametres =>
                {
                    parametres.AddWithValue("@login_id", loginId);
                },
                async reader =>
                {
                    await reader.ReadAsync();
                    var dbUserId = reader.GetInt32(0);
                    var dbDatabaseId = reader.GetInt16(1);
                    var dbCustomerConnectionString = reader.GetString(2);
                    return new Tuple<int, int, string>(dbUserId, dbDatabaseId, dbCustomerConnectionString);
                });
            return result;
        }

        #region private methods

        /// <exception cref="DatabaseException">Database operation is failed</exception>
        private async Task<Tuple<int, int>> GetLoginUserId(string loginDbConnectionString, int? loginId = default(int?), string externalLoginId = null)
        {
            Action<SqlParameterCollection> addParametersAction = parameters =>
            {
                if (loginId.HasValue)
                    parameters.AddWithValue("@login_id", loginId.Value);
                else if (!string.IsNullOrEmpty(externalLoginId))
                    parameters.AddWithValue("@external_login_id", externalLoginId);
            };
            Func<SqlDataReader, Task<Tuple<int, int>>> processReader = async reader =>
            {
                await reader.ReadAsync();
                var dbLoginId = reader.GetInt32(0);
                var dbUserId = reader.GetInt32(1);
                return new Tuple<int, int>(dbLoginId, dbUserId);
            };
            var result = await UseReader(loginDbConnectionString, "sc_orchestration_get_loginuser_id", addParametersAction, processReader);
            return result;
        }

        /// <exception cref="DatabaseException">Database operation is failed</exception>
        private async Task Exec(string connectionString, string spName, Action<SqlParameterCollection> addParametersAction = null)
        {
            try
            {
                using (var sqlConn = new SqlConnection(connectionString))
                {
                    await sqlConn.OpenAsync();

                    using (var sqlCmd = new SqlCommand(spName, sqlConn))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        addParametersAction?.Invoke(sqlCmd.Parameters);
                        await sqlCmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex) {
                _logger.DatabaseError(ex);
                throw new DatabaseException(ex);
            }
        }

        /// <exception cref="DatabaseException">Database operation is failed</exception>
        private async Task<T> UseReader<T>(string connectionString, string spName, Action<SqlParameterCollection> addParametersAction = null, Func<SqlDataReader, Task<T>> processReader = null)
        {
            T result = default (T);
            try
            {
                using (var sqlConn = new SqlConnection(connectionString))
                {
                    await sqlConn.OpenAsync();

                    using (var sqlCmd = new SqlCommand(spName, sqlConn))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        addParametersAction?.Invoke(sqlCmd.Parameters);
                        using (var reader = await sqlCmd.ExecuteReaderAsync())
                        {
                            if (reader.HasRows && processReader != null) {
                                result = await processReader(reader);
                            }
                            if (!reader.IsClosed)
                                reader.Close();
                        }
                    }
                }
                return result;
            }
            catch (Exception ex) {
                _logger.DatabaseError(ex);
                throw new DatabaseException(ex);
            }
        }
        #endregion
    }
}