using System;
using System.Threading.Tasks;
using System.Xml;
using StarChef.Listener.Exceptions;
using StarChef.Orchestrate.Models.TransferObjects;

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
        Task UpdateUser(string externalLoginId, string username, string firstName, string lastName, string emailAddress);

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
        /// <param name="trackingId">Caught event ID</param>
        /// <param name="isSuccessful">Set True if the message was successfully processed</param>
        /// <param name="code">Short unique name of the operation. The code may come from external system as well as from the SC itself</param>
        /// <param name="details">Additional details about the event, for examples errors description</param>
        /// <param name="payloadJson">(Optional) JSON representation of event's payload </param>
        /// <exception cref="ConnectionStringNotFoundException">Login connection string is not found</exception>
        /// <exception cref="DatabaseException">Error is occurred while saving data to database</exception>
        /// <returns></returns>
        Task RecordMessagingEvent(string trackingId, bool isSuccessful, string code, string details = null, string payloadJson = null);
    }
}