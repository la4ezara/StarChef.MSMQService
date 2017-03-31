using System;
using System.Threading.Tasks;
using StarChef.Listener.Exceptions;

namespace StarChef.Listener.Commands
{
    internal interface IConnectionStringProvider
    {
        /// <summary>
        /// Get connection string Login DB
        /// </summary>
        /// <returns></returns>
        Task<string> GetLoginDb();

        /// <summary>
        /// Get connection string to Customer DB
        /// </summary>
        /// <param name="organisationId">Organization ID associated with the database</param>
        /// <param name="connectionStringLoginDb">Connection string to Login DB</param>
        /// <exception cref="ConnectionStringLookupException">Error is occurred while getting a customer DB</exception>
        /// <returns></returns>
        Task<string> GetCustomerDb(Guid organisationId, string connectionStringLoginDb);
        /// <summary>
        /// Get connection string to Customer DB
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="connectionStringLoginDb"></param>
        /// <exception cref="ConnectionStringLookupException">Error is occurred while getting a customer DB</exception>
        /// <returns></returns>
        Task<string> GetCustomerDb(int loginId, string connectionStringLoginDb);

        /// <summary>
        /// Return organization Id by Org Guid
        /// </summary>
        /// <param name="orgGuid"></param>
        /// <param name="connectionStringLoginDb"></param>
        /// <returns></returns>
        Task<int> GetCustomerDbId(Guid orgGuid, string connectionStringLoginDb);
    }
}