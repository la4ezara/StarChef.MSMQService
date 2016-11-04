using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using log4net;
using StarChef.Listener.Exceptions;

namespace StarChef.Listener.Commands.Impl
{
    internal abstract class DatabaseCommands : IDatabaseCommands
    {
        private readonly IConnectionStringProvider _csProvider;

        protected DatabaseCommands(IConnectionStringProvider csProvider)
        {
            _csProvider = csProvider;
        }

        protected abstract string SaveStoredProcedureName { get; }

        /// <exception cref="LoginDbNotFoundException">Raised when Login DB connection string is not found.</exception>
        /// <exception cref="CustomerDbNotFoundException">Raised when Customer DB connection string is not found for the given organization ID.</exception>
        /// <exception cref="DataNotSavedException">Error is occurred while saving data to DB.</exception>
        public async Task SaveData(Guid organisationId, XmlDocument xmlDoc)
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

                    using (var sqlCmd = new SqlCommand(SaveStoredProcedureName, sqlConn))
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
    }
}