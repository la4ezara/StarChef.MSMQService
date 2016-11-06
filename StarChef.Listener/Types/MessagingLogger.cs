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

        public async Task RegisterFailedMessage(OperationFailedTransferObject operationFailed, string trackingId)
        {
            await _dbCommands.RecordMessagingEvent(trackingId, operationFailed);
        }

        public async Task RegisterInvalidModel(string error, object payload, string trackingId)
        {
            var json = JsonConvert.SerializeObject(payload);
            await _dbCommands.RecordMessagingEvent(trackingId, json, false, error);
        }

        public async Task RegisterSuccess(object payload, string trackingId)
        {
            var json = JsonConvert.SerializeObject(payload);
            await _dbCommands.RecordMessagingEvent(trackingId, json, true);
        }
    }
}