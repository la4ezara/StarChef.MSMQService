using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Model.People;
using log4net;
using StarChef.Listener.Commands;
using StarChef.Listener.Extensions;
using StarChef.Orchestrate.Models.TransferObjects;

namespace StarChef.Listener.Handlers
{
    public class AccountCreateFailedEventHandler : ListenerEventHandler, IMessageHandler<Events.AccountCreateFailed>
    {
        private readonly ILog _logger;

        public AccountCreateFailedEventHandler(IDatabaseCommands dbCommands, IEventValidator validator, IMessagingLogger messagingLogger) : base(dbCommands, validator, messagingLogger)
        {
            _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public AccountCreateFailedEventHandler(IDatabaseCommands dbCommands, IEventValidator validator, IMessagingLogger messagingLogger, ILog errorLogger) : base(dbCommands, validator, messagingLogger)
        {
            _logger = errorLogger;
        }

        public async Task<MessageHandlerResult> HandleAsync(Events.AccountCreateFailed payload, string trackingId)
        {
            ThreadContext.Properties[INTERNAL_ID] = payload.InternalId;

            if (Validator.IsStarChefEvent(payload))
            {
                _logger.EventReceived(trackingId, payload);

                if (Validator.IsValid(payload))
                {
                    var operationFailed = Mapper.Map<AccountCreateFailedTransferObject>(payload);

                    await MessagingLogger.ReceivedFailedMessage(operationFailed, trackingId);
                    _logger.Processed(trackingId, payload);
                }
                else
                {
                    var errors = Validator.GetErrors();
                    _logger.InvalidModel(trackingId, payload, errors);
                    await MessagingLogger.ReceivedInvalidModel(trackingId, payload, errors);
                    ThreadContext.Properties.Remove(INTERNAL_ID);
                    return MessageHandlerResult.Fatal;
                }
            }

            ThreadContext.Properties.Remove(INTERNAL_ID);
            return MessageHandlerResult.Success;
        }
    }
}