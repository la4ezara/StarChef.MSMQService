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
        /// <param name="user"></param>
        /// <returns></returns>
        Task UpdateUser(UserTransferObject user);

        /// <summary>
        ///     Set user identifier in the external system (such as Fourth Account Service)
        /// </summary>
        /// <param name="user"></param>
        /// <exception cref="LoginDbNotFoundException">Raised when Login DB connection string is not found.</exception>
        /// <exception cref="DataNotSavedException">Error is occurred while saving data to DB.</exception>
        /// <returns></returns>
        Task UpdateExternalId(UserTransferObject user);

        /// <summary>
        ///     Record information about messaging
        /// </summary>
        /// <param name="trackingId">Caught event ID</param>
        /// <param name="jsonEvent">String representation of event</param>
        /// <param name="isSuccessful">Set True if the message was successfully processed</param>
        /// <param name="details">Additional details about the event, for examples errors description</param>
        /// <returns></returns>
        Task RecordMessagingEvent(string trackingId, string jsonEvent, bool isSuccessful, string details = null);

        /// <summary>
        ///     Record information about message with information about failure in the external system
        /// </summary>
        /// <param name="trackingId">Caught event ID</param>
        /// <param name="operationFailed">Information about failure</param>
        /// <returns></returns>
        Task RecordMessagingEvent(string trackingId, OperationFailedTransferObject operationFailed);
    }
}