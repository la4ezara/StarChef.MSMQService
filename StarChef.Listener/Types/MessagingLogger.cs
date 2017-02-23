using System.Threading.Tasks;
using Newtonsoft.Json;
using StarChef.Listener.Commands;
using StarChef.Listener.Exceptions;
using StarChef.Listener.Extensions;
using StarChef.Orchestrate.Models.TransferObjects;

namespace StarChef.Listener.Types
{
    internal class MessagingLogger : IMessagingLogger
    {
        private readonly IDatabaseCommands _dbCommands;

        public MessagingLogger(IDatabaseCommands dbCommands)
        {
            _dbCommands = dbCommands;
        }

        /// <exception cref="ConnectionStringNotFoundException">Login connection string is not found</exception>
        /// <exception cref="DatabaseException">Error is occurred while saving data to database</exception>
        public async Task ReceivedFailedMessage(FailedTransferObject operationFailed, string trackingId)
        {
            var json = string.Format("[{0}] {1}", operationFailed.GetType().Name, operationFailed.ToJson());
            await _dbCommands.RecordMessagingEvent(trackingId, true, operationFailed.ErrorCode, operationFailed.Description, json);
        }

        /// <exception cref="ConnectionStringNotFoundException">Login connection string is not found</exception>
        /// <exception cref="DatabaseException">Error is occurred while saving data to database</exception>
        public async Task ReceivedInvalidModel(string trackingId, object payload, string error)
        {
            var json = string.Format("[{0}] {1}", payload.GetType().Name, payload.ToJson());
            await _dbCommands.RecordMessagingEvent(trackingId, false, Codes.InvalidModel, error, json);
        }

        /// <exception cref="ConnectionStringNotFoundException">Login connection string is not found</exception>
        /// <exception cref="DatabaseException">Error is occurred while saving data to database</exception>
        public async Task MessageProcessedSuccessfully(object payload, string trackingId)
        {
            var json = string.Format("[{0}] {1}", payload.GetType().Name, payload.ToJson());
            await _dbCommands.RecordMessagingEvent(trackingId, true, Codes.MessageProcessed, payloadJson: json);
        }

        private static class Codes
        {
            public const string InvalidModel = "INVALID_MODEL";
            public const string MessageProcessed = "PROCESSED";
        }
    }
}