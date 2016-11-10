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

            using (var tran = new TransactionScope())
            {
                await Exec(loginDbConnectionString, "sc_orchestration_update_user", p =>
                {
                    p.AddWithValue("@loginId", loginId);
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
                tran.Complete();
            }
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

            using (var tran = new TransactionScope())
            {
                await Exec(loginDbConnectionString, "sc_orchestration_disable_user", p => p.AddWithValue("@loginId", existingLoginId));
                await Exec(connectionString, "sc_orchestration_disable_user", p => p.AddWithValue("@userId", existingUserId));
                tran.Complete();
            }
        }

        #region private methods

        /// <exception cref="DatabaseException">Database operation is failed</exception>
        private async Task<Tuple<int, int>> GetLoginUserId(string loginDbConnectionString, int? loginId = default(int?), string externalLoginId = null)
        {
            var reader = await GetReader(loginDbConnectionString, "sc_orchestration_get_loginuser_id", p =>
            {
                if (loginId.HasValue)
                    p.AddWithValue("@login_id", loginId.Value);
                else if (!string.IsNullOrEmpty(externalLoginId))
                    p.AddWithValue("@external_login_id", externalLoginId);
            });
            try
            {
                if (reader.HasRows)
                {
                    await reader.ReadAsync();
                    var dbLoginId = reader.GetInt32(0);
                    var dbUserId = reader.GetInt32(1);
                    return new Tuple<int, int>(dbLoginId, dbUserId);
                }
            }
            finally { reader.Close(); }
            return null;
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
                throw new DatabaseException(ex);
            }
        }

        /// <exception cref="DatabaseException">Database operation is failed</exception>
        private async Task<SqlDataReader> GetReader(string connectionString, string spName, Action<SqlParameterCollection> addParametersAction = null)
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
                        return await sqlCmd.ExecuteReaderAsync();
                    }
                }
            }
            catch (Exception ex) {
                throw new DatabaseException(ex);
            }
        }

        #endregion
    }
}