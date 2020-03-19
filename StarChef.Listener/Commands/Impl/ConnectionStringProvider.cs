using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using StarChef.Listener.Exceptions;
using StarChef.Common;

namespace StarChef.Listener.Commands.Impl
{
    internal class ConnectionStringProvider : IConnectionStringProvider
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public async Task<Tuple<int, string, Guid>> GetCustomerDbDetails(string externalDatabaseId, string connectionStringLoginDb)
        {
            using (var sqlConnection = new SqlConnection(connectionStringLoginDb))
            {
                Exception exception = null;
                await sqlConnection.OpenAsync();
                try
                {
                    using (var sqlCmd = new SqlCommand("sc_database_GetByExternalId", sqlConnection))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.Parameters.AddWithValue("@externalId", externalDatabaseId);
                        var reader = await sqlCmd.ExecuteReaderAsync();
                        if (await reader.ReadAsync())
                        {
                            var dbId = reader.GetValue<int>("db_database_id");
                            var connStr = reader.GetValue<string>("database_connection_string");
                            var dbGuid = reader.GetValue<Guid>("db_database_guid");
                            return new Tuple<int, string, Guid>(dbId, connStr, dbGuid);
                        }
                        throw new ConnectionStringLookupException("Connection string not found for DB with external ID = " + externalDatabaseId);
                    }
                }
                catch (InvalidCastException ex)
                {
                    exception = ex;
                }
                catch (SqlException ex)
                {
                    exception = ex;
                }
                catch (IOException ex)
                {
                    exception = ex;
                }
                throw new ConnectionStringLookupException("Error is occurred while getting a customer DB", exception);
            }
        }

        /// <exception cref="ConnectionStringLookupException">Error is occurred while getting a customer DB</exception>
        public async Task<string> GetCustomerDb(int loginId, string connectionStringLoginDb)
        {
            using (var sqlConnection = new SqlConnection(connectionStringLoginDb))
            {
                Exception exception = null;
                await sqlConnection.OpenAsync();
                try
                {
                    using (var sqlCmd = new SqlCommand("sc_orchestration_get_database_by_login", sqlConnection))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.Parameters.AddWithValue("@login_id", loginId);
                        var rtnVal = sqlCmd.ExecuteScalar();
                        return rtnVal.ToString();
                    }
                }
                catch (InvalidCastException ex)
                {
                    exception = ex;
                }
                catch (SqlException ex)
                {
                    exception = ex;
                }
                catch (IOException ex)
                {
                    exception = ex;
                }
                throw new ConnectionStringLookupException("Error is occurred while getting a customer DB", exception);
            }
        }

        /// <exception cref="ConnectionStringLookupException">Error is occurred while getting a customer DB</exception>
        public async Task<string> GetCustomerDb(Guid organizationId, string connectionStringLoginDb)
        {
            using (var sqlConnection = new SqlConnection(connectionStringLoginDb))
            {
                Exception exception = null;
                await sqlConnection.OpenAsync();
                try
                {
                    using (var sqlCmd = new SqlCommand("sc_database_GetByOrgGuid", sqlConnection))
                    {
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.Parameters.Add("@OrganisationGuid", SqlDbType.UniqueIdentifier).Value = organizationId;
                        var rtnVal = sqlCmd.ExecuteScalar();
                        if (rtnVal == null || rtnVal == DBNull.Value)
                        {
                            _logger.Error(string.Format("There is no organization with the given organization Guid: {0}", organizationId));
                            return null;
                        }
                        return rtnVal.ToString();
                    }
                }
                catch (InvalidCastException ex) {
                    exception = ex;
                }
                catch (SqlException ex) {
                    exception = ex;
                }
                catch (IOException ex) {
                    exception = ex;
                }
                throw new ConnectionStringLookupException("Error is occurred while getting a customer DB", exception);
            }
        }

        /// <exception cref="ConnectionStringLookupException">Error is occurred while getting a login DB</exception>
        public Task<string> GetLoginDb()
        {
            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["StarchefLogin"].ConnectionString;
                return Task.FromResult(connectionString);
            }
            catch (ConfigurationErrorsException ex) {
                throw new ConnectionStringLookupException("Error is occurred while getting a login DB", ex);
            }
        }
    }
}