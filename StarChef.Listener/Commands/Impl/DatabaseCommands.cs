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

        public Task RecordMessagingEvent(string trackingId, bool isSuccessful, string code, string details = null, string payloadJson = null)
        {
            throw new NotImplementedException();
        }

        /// <exception cref="DataNotSavedException">Error is occurred while saving data to DB.</exception>
        public async Task SavePriceBandData(Guid organisationId, XmlDocument xmlDoc)
        {
            Exception exception = null;
            try
            {
                var loginDbConnectionString = await _csProvider.GetLoginDb();
                if (string.IsNullOrEmpty(loginDbConnectionString))
                    throw new ConnectionStringNotFoundException("Login connection string is not found");

                var connectionString = await _csProvider.GetCustomerDb(organisationId, loginDbConnectionString);
                if (string.IsNullOrEmpty(connectionString))
                    throw new ConnectionStringNotFoundException("Customer DB connection string is not found");

                await Exec(loginDbConnectionString, "sc_save_product_price_band_list", p => { p.Add("@DataXml", SqlDbType.Xml).Value = xmlDoc.InnerXml; });
            }
            catch (ListenerException ex) {
                exception = ex;
            }
            catch (DbException ex) {
                exception = ex;
            }
            catch (XmlException ex) {
                exception = ex;
            }
            if (exception != null)
                throw new DataNotSavedException("Error is occurred while saving data to DB.", exception);
        }

        /// <exception cref="DataNotSavedException">Error is occurred while saving data to DB.</exception>
        public async Task UpdateExternalId(AccountCreatedTransferObject user)
        {
            Exception exception = null;
            try
            {
                var loginDbConnectionString = await _csProvider.GetLoginDb();
                if (string.IsNullOrEmpty(loginDbConnectionString))
                    throw new ConnectionStringNotFoundException("Login DB connection string is not found");

                await Exec(loginDbConnectionString, "sc_orchestration_update_login_external_id", p =>
                {
                    p.AddWithValue("@login_id", user.LoginId);
                    p.AddWithValue("@external_login_id", user.ExtrenalLoginId);
                });
            }
            catch (ConnectionStringLookupException ex) {
                exception = ex;
            }
            catch (DbException ex) {
                exception = ex;
            }
            if (exception != null)
                throw new DataNotSavedException("Error is occurred while saving data to DB.", exception);
        }

        /// <exception cref="DataNotSavedException">Error is occurred while saving data to DB.</exception>
        public async Task UpdateUser(string extrenalLoginId, string username, string firstName, string lastName, string emailAddress)
        {
            Exception exception = null;
            try
            {
                var loginDbConnectionString = await _csProvider.GetLoginDb();
                if (string.IsNullOrEmpty(loginDbConnectionString))
                    throw new ConnectionStringNotFoundException("Login DB connection string is not found");

                var ids = await GetLoginUserId(loginDbConnectionString, extrenalLoginId);
                if (ids == null)
                    throw new DataNotSavedException("Cannot map external account to the StarChef account");
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
            catch (DbException ex) {
                exception = ex;
            }
            catch (ConnectionStringLookupException ex) {
                exception = ex;
            }
            if (exception != null)
                throw new DataNotSavedException("Error is occurred while saving data to DB.", exception);
        }

        private async Task<Tuple<int, int>> GetLoginUserId(string loginDbConnectionString, string extrenalLoginId)
        {
            var reader = await GetReader(loginDbConnectionString, "sc_orchestration_get_loginuser_id", p => p.AddWithValue("@external_login_id", extrenalLoginId));
            try
            {
                if (reader.HasRows)
                {
                    await reader.ReadAsync();
                    var loginId = reader.GetInt32(0);
                    var userId = reader.GetInt32(1);
                    return new Tuple<int, int>(loginId, userId);
                }
            }
            finally { reader.Close(); }
            return null;
        }

        private async Task Exec(string connectionString, string spName, Action<SqlParameterCollection> addParametersAction = null)
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

        private async Task<SqlDataReader> GetReader(string connectionString, string spName, Action<SqlParameterCollection> addParametersAction = null)
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
    }
}