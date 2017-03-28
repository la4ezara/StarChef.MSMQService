using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Model.People;
using log4net;
using StarChef.Listener.Commands;
using StarChef.Orchestrate.Models.TransferObjects;
using StarChef.Listener.Extensions;

namespace StarChef.Listener.Handlers
{
    public class AccountStatusChangeFailedEventHandler : ListenerEventHandler, IMessageHandler<Events.AccountStatusChangeFailed>
    {
        private readonly ILog _logger;

        public AccountStatusChangeFailedEventHandler(IDatabaseCommands dbCommands, IEventValidator validator, IMessagingLogger messagingLogger) : base(dbCommands, validator, messagingLogger)
        {
            _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public AccountStatusChangeFailedEventHandler(IDatabaseCommands dbCommands, IEventValidator validator, IMessagingLogger messagingLogger, ILog errorLogger) : base(dbCommands, validator, messagingLogger)
        {
            _logger = errorLogger;
        }

        public async Task<MessageHandlerResult> HandleAsync(Events.AccountStatusChangeFailed payload, string trackingId)
        {
            ThreadContext.Properties[EXTERNAL_ID] = payload.ExternalId;

            if (Validator.IsStarChefEvent(payload))
            {
                _logger.EventReceived(trackingId, payload);

                if (Validator.IsValidPayload(payload))
                {
                    var operationFailed = Mapper.Map<AccountStatusChangeFailedTransferObject>(payload);
                    await MessagingLogger.ReceivedFailedMessage(operationFailed, trackingId);
                    _logger.Processed(trackingId, payload);
                }
                else
                {
                    var errors = Validator.GetErrors();
                    _logger.InvalidModel(trackingId, payload, errors);
                    await MessagingLogger.ReceivedInvalidModel(trackingId, payload, errors);
                    ThreadContext.Properties.Remove(EXTERNAL_ID);
                    return MessageHandlerResult.Fatal;
                }
            }

            ThreadContext.Properties.Remove(EXTERNAL_ID);
            return MessageHandlerResult.Success;
        }
    }
}