using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;
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

        public Task RecordMessagingEvent(string trackingId, OperationFailedTransferObject operationFailed)
        {
            throw new NotImplementedException();
        }

        public Task RecordMessagingEvent(string trackingId, string jsonEvent, bool isSuccessful, string details = null)
        {
            throw new NotImplementedException();
        }

        /// <exception cref="LoginDbNotFoundException">Raised when Login DB connection string is not found.</exception>
        /// <exception cref="CustomerDbNotFoundException">Raised when Customer DB connection string is not found for the given organization ID.</exception>
        /// <exception cref="DataNotSavedException">Error is occurred while saving data to DB.</exception>
        public async Task SavePriceBandData(Guid organisationId, XmlDocument xmlDoc)
        {
            Exception exception = null;
            try
            {
                var loginDbConnectionString = await _csProvider.GetLoginDb();
                if (string.IsNullOrEmpty(loginDbConnectionString))
                    throw new LoginDbNotFoundException();

                var connectionString = await _csProvider.GetCustomerDb(organisationId, loginDbConnectionString);
                if (string.IsNullOrEmpty(connectionString))
                    throw new CustomerDbNotFoundException();

                using (var sqlConn = new SqlConnection(connectionString))
                {
                    await sqlConn.OpenAsync();

                    using (var sqlCmd = new SqlCommand("sc_save_product_price_band_list", sqlConn))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.Parameters.Add("@DataXml", SqlDbType.Xml).Value = xmlDoc.InnerXml;

                        await sqlCmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (LoginDbLookupException ex) {
                exception = ex;
            }
            catch (CustomerDbLookupException ex) {
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

        /// <exception cref="LoginDbNotFoundException">Raised when Login DB connection string is not found.</exception>
        /// <exception cref="DataNotSavedException">Error is occurred while saving data to DB.</exception>
        public async Task UpdateExternalId(UserTransferObject user)
        {
            Exception exception = null;
            try
            {
                var loginDbConnectionString = await _csProvider.GetLoginDb();
                if (string.IsNullOrEmpty(loginDbConnectionString))
                    throw new LoginDbNotFoundException();

                using (var sqlConn = new SqlConnection(loginDbConnectionString))
                {
                    await sqlConn.OpenAsync();

                    using (var sqlCmd = new SqlCommand("sc_orchestration_update_login_external_id", sqlConn))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.Parameters.AddWithValue("@login_id", user.LoginId);
                        sqlCmd.Parameters.AddWithValue("@external_login_id", user.ExtrenalLoginId);

                        await sqlCmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (LoginDbLookupException ex)
            {
                exception = ex;
            }
            catch (CustomerDbLookupException ex)
            {
                exception = ex;
            }
            catch (DbException ex)
            {
                exception = ex;
            }
            if (exception != null)
                throw new DataNotSavedException("Error is occurred while saving data to DB.", exception);
        }

        /// <exception cref="LoginDbNotFoundException">Raised when Login DB connection string is not found.</exception>
        /// <exception cref="DataNotSavedException">Error is occurred while saving data to DB.</exception>
        public async Task UpdateUser(UserTransferObject user)
        {
            Exception exception = null;
            try
            {
                var loginDbConnectionString = await _csProvider.GetLoginDb();
                if (string.IsNullOrEmpty(loginDbConnectionString))
                    throw new LoginDbNotFoundException();

                var connectionString = await _csProvider.GetCustomerDb(user.LoginId, loginDbConnectionString);
                if (string.IsNullOrEmpty(connectionString))
                    throw new CustomerDbNotFoundException();

                using (var sqlConn = new SqlConnection(loginDbConnectionString))
                {
                    await sqlConn.OpenAsync();

                    using (var sqlCmd = new SqlCommand("sc_orchestration_update_user", sqlConn))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.Parameters.AddWithValue("@userId", user.ExtrenalLoginId);
                        sqlCmd.Parameters.AddWithValue("@email", user.EmailAddress);
                        sqlCmd.Parameters.AddWithValue("@login_name", user.Username);
                        sqlCmd.Parameters.AddWithValue("@forename", user.FirstName);
                        sqlCmd.Parameters.AddWithValue("@lastname", user.LastName);
                        await sqlCmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (LoginDbLookupException ex)
            {
                exception = ex;
            }
            catch (DbException ex)
            {
                exception = ex;
            }
            catch (CustomerDbLookupException ex) {
                exception = ex;
            }
            if (exception != null)
                throw new DataNotSavedException("Error is occurred while saving data to DB.", exception);
        }
    }
}