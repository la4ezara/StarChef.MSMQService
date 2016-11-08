using System.Threading.Tasks;
using Newtonsoft.Json;
using StarChef.Listener.Commands;
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

        public async Task ReceivedFailedMessage(FailedTransferObject operationFailed, string trackingId)
        {
            var json = SerializeObject(operationFailed);
            await _dbCommands.RecordMessagingEvent(trackingId, true, operationFailed.ErrorCode, operationFailed.Description, json);
        }

        public async Task ReceivedInvalidModel(string trackingId, object payload, string error)
        {
            var json = SerializeObject(payload);
            await _dbCommands.RecordMessagingEvent(trackingId, false, Codes.InvalidModel, error, json);
        }

        public async Task MessageProcessedSuccessfully(object payload, string trackingId)
        {
            var json = SerializeObject(payload);
            await _dbCommands.RecordMessagingEvent(trackingId, true, Codes.MessageProcessed, payloadJson: json);
        }

        private static string SerializeObject(object operationFailed)
        {
            return JsonConvert.SerializeObject(operationFailed, Formatting.Indented);
        }

        private static class Codes
        {
            public const string InvalidModel = "INVALID_MODEL";
            public const string MessageProcessed = "PROCESSED";
        }
    }
}