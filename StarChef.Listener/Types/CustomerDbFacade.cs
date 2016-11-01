using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using log4net;

namespace StarChef.Listener.Types
{
    internal abstract class CustomerDbFacade : ICustomerDb
    {
        private readonly string _loginDbConnectionString;

        protected CustomerDbFacade(string loginDbConnectionString)
        {
            _loginDbConnectionString = loginDbConnectionString;
        }

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public async Task<string> GetConnectionString(Guid organisationGuid)
        {
            using (var sqlConnection = new SqlConnection(_loginDbConnectionString))
            {
                await sqlConnection.OpenAsync();

                using (var sqlCmd = new SqlCommand("sc_database_GetByOrgGuid", sqlConnection))
                {
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.Add("@OrganisationGuid", SqlDbType.UniqueIdentifier).Value = organisationGuid;

                    var rtnVal = sqlCmd.ExecuteScalar();

                    if (rtnVal == DBNull.Value)
                    {
                        Logger.Error($"There is no organisation with the given organisation Guid: {organisationGuid}");
                        return null;
                    }
                    return rtnVal.ToString();
                }
            }
        }

        public async Task SaveDataToDb(string connectionString, XmlDocument xmlDoc)
        {
            using (var sqlConn = new SqlConnection(connectionString))
            {
                await sqlConn.OpenAsync();

                using (var sqlCmd = new SqlCommand(SaveStoredProcedureName, sqlConn))
                {
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.Add("@DataXml", SqlDbType.Xml).Value = xmlDoc.InnerXml;

                    await sqlCmd.ExecuteNonQueryAsync();
                }
            }
        }

        protected abstract string SaveStoredProcedureName { get; }
    }
}