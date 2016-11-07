using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using StarChef.Listener.Exceptions;

namespace StarChef.Listener.Commands.Impl
{
    internal class ConnectionStringProvider : IConnectionStringProvider
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <exception cref="CustomerDbLookupException">Error is occurred while getting a customer DB</exception>
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
                throw new CustomerDbLookupException("Error is occurred while getting a customer DB", exception);
            }
        }

        /// <exception cref="CustomerDbLookupException">Error is occurred while getting a customer DB</exception>
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
                        if (rtnVal == DBNull.Value)
                        {
                            Logger.Error(string.Format("There is no organization with the given organization Guid: {0}", organizationId));
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
                throw new CustomerDbLookupException("Error is occurred while getting a customer DB", exception);
            }
        }

        /// <exception cref="LoginDbLookupException">Error is occurred while getting a login DB</exception>
        public Task<string> GetLoginDb()
        {
            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["StarchefLogin"].ConnectionString;
                return Task.FromResult(connectionString);
            }
            catch (ConfigurationErrorsException ex) {
                throw new LoginDbLookupException("Error is occurred while getting a login DB", ex);
            }
        }
    }
}