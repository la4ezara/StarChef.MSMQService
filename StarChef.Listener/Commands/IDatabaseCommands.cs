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
        /// <exception cref="LoginDbNotFoundException">When connection string is not found Login Db</exception>
        /// <exception cref="CustomerDbNotFoundException">When connection string is not found for the given organization Id</exception>
        /// <exception cref="DataNotSavedException">Error is occurred while saving data to DB.</exception>
        Task SavePriceBandData(Guid organisationId, XmlDocument xmlDoc);

        /// <summary>
        ///     Update some fields of user data
        /// </summary>
        /// <param name="extrenalLoginId"></param>
        /// <param name="username"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        Task UpdateUser(string extrenalLoginId, string username, string firstName, string lastName, string emailAddress);

        /// <summary>
        ///     Set user identifier in the external system (such as Fourth Account Service)
        /// </summary>
        /// <param name="user"></param>
        /// <exception cref="LoginDbNotFoundException">Raised when Login DB connection string is not found.</exception>
        /// <exception cref="DataNotSavedException">Error is occurred while saving data to DB.</exception>
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
        /// <returns></returns>
        Task RecordMessagingEvent(string trackingId, bool isSuccessful, string code, string details = null, string payloadJson = null);
    }
}