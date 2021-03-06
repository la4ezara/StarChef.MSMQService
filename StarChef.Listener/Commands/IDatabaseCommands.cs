using System;
using System.Threading.Tasks;
using System.Xml;
using StarChef.Listener.Exceptions;
using StarChef.Orchestrate.Models.TransferObjects;
using System.Collections.Generic;

namespace StarChef.Listener.Commands
{
    public interface IDatabaseCommands
    {
        /// <summary>
        ///     Save data to organization database
        /// </summary>
        /// <param name="organisationId">Organization Id</param>
        /// <param name="xmlDoc"></param>
        /// <exception cref="ConnectionStringNotFoundException">Some connection string is not found</exception>
        /// <exception cref="DatabaseException">Error is occurred while saving data to database</exception>
        /// <exception cref="ConnectionStringLookupException">Error is occurred while getting a customer DB</exception>
        Task SavePriceBandData(Guid organisationId, XmlDocument xmlDoc);

        /// <summary>
        /// Returns True if the listener should catch event of the type
        /// </summary>
        /// <param name="eventTypeShortName">Type short name (without namespace)</param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        Task<bool> IsEventEnabledForOrganization(string eventTypeShortName, Guid organizationId);

        /// <summary>
        /// Returns True if the listener should catch event of the type
        /// </summary>
        /// <param name="eventTypeShortName"></param>
        /// <param name="loginId"></param>
        /// <returns></returns>
        Task<bool> IsEventEnabledForOrganization(string eventTypeShortName, int loginId);

        /// <summary>
        /// Returns True if user is exists
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="externalLoginId">It will be skipped if <paramref name="loginId"></paramref> is specified</param>
        /// <returns></returns>
        Task<bool> IsUserExists(int? loginId = null, string externalLoginId = null, string username = null);

        /// <summary>
        /// Returns True if the listener should catch event of the type
        /// </summary>
        /// <param name="eventTypeShortName"></param>
        /// <param name="externalId"></param>
        /// <returns></returns>
        Task<bool> IsEventEnabledForOrganization(string eventTypeShortName, string externalId);

        /// <summary>
        /// Lookup database for actual user login Id
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        Task<int> OriginateLoginId(int loginId, string username);

        /// <summary>
        /// Disable login
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="externalLoginId"></param>
        /// <returns>If both identifiers are specified, the login id is used</returns>
        /// <exception cref="DatabaseException">Database operation is failed</exception>
        /// <exception cref="ConnectionStringNotFoundException">Customer DB connections string is not found</exception>
        /// <exception cref="ListenerException">Cannot map external account to the StarChef account</exception>
        /// <exception cref="ConnectionStringLookupException">Error is occurred while getting a customer DB</exception>
        Task DisableLogin(int? loginId = null, string externalLoginId = null);

        /// <summary>
        /// Enable login
        /// </summary>
        /// <param name="loginId"></param>
        /// <param name="externalLoginId"></param>
        /// <returns>If both identifiers are specified, the login id is used</returns>
        /// <exception cref="DatabaseException">Database operation is failed</exception>
        /// <exception cref="ConnectionStringNotFoundException">Customer DB connections string is not found</exception>
        /// <exception cref="ListenerException">Cannot map external account to the StarChef account</exception>
        /// <exception cref="ConnectionStringLookupException">Error is occurred while getting a customer DB</exception>
        Task EnableLogin(int? loginId = null, string externalLoginId = null);

        /// <summary>
        ///     Update some fields of user data
        /// </summary>
        /// <param name="externalLoginId"></param>
        /// <param name="username"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="emailAddress"></param>
        /// <exception cref="ConnectionStringNotFoundException">Some connection string is not found</exception>
        /// <exception cref="DatabaseException">Error is occurred while saving data to database</exception>
        /// <exception cref="ListenerException">Exception in general logic of the listener</exception>
        /// <exception cref="ConnectionStringLookupException">Error is occurred while getting a customer DB</exception>
        /// <returns></returns>
        Task UpdateUser(string externalLoginId, string username, string firstName, string lastName, string emailAddress, IEnumerable<string> permissionSets);

        /// <summary>
        ///     Set user identifier in the external system (such as Fourth Account Service)
        /// </summary>
        /// <param name="user"></param>
        /// <exception cref="ConnectionStringNotFoundException">Some connection string is not found</exception>
        /// <exception cref="DatabaseException">Error is occurred while saving data to database</exception>
        /// <returns></returns>
        Task UpdateExternalId(AccountCreatedTransferObject user);

        /// <summary>
        ///     Record information about messaging
        /// </summary>
        /// <param name="user"></param>
        /// <exception cref="ConnectionStringNotFoundException">Login connection string is not found</exception>
        /// <exception cref="DatabaseException">Error is occurred while saving data to database</exception>
        /// <returns></returns>
        Task<int> AddUser(AccountCreatedTransferObject user);

        Task RecordMessagingEvent(string trackingId, bool isSuccessful, string code, string details = null, string payloadJson = null);

        /// <summary>
        ///     Get userId and customerDB details
        /// </summary>
        Task<Tuple<int, int, string>> GetLoginUserIdAndCustomerDb(int loginId);

        Task<int> GetDefaultUserGroup(string customerDbConnectionString);
    }
}