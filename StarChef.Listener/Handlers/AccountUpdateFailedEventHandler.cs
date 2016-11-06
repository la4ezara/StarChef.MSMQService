using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Model.People;
using log4net;
using StarChef.Listener.Commands;
using StarChef.Orchestrate.Models.TransferObjects;

namespace StarChef.Listener.Handlers
{
    public class AccountUpdateFailedEventHandler : ListenerEventHandler, IMessageHandler<Events.AccountUpdateFailed>
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public AccountUpdateFailedEventHandler(IDatabaseCommands dbCommands, IEventValidator validator, IMessagingLogger messagingLogger) : base(dbCommands, validator, messagingLogger)
        {
        }

        public async Task<MessageHandlerResult> HandleAsync(Events.AccountUpdateFailed payload, string trackingId)
        {
            if (Validator.IsStarChefEvent(payload))
                if (Validator.IsValid(payload))
                {
                    var operationFailed = Mapper.Map<OperationFailedTransferObject>(payload);
                    await MessagingLogger.RegisterFailedMessage(operationFailed, trackingId);
                    return MessageHandlerResult.Success;
                }

            var errors = Validator.GetErrors();
            _logger.Error(string.Format("AccountUpdateFailed message is received, but cannot be read. Tracking ID: {0}", trackingId));
            await MessagingLogger.RegisterInvalidModel(errors, payload, trackingId);
            return MessageHandlerResult.Fatal;
        }
    }
}