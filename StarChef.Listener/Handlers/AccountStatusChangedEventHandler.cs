
using System.Reflection;
using System.Threading.Tasks;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Model.People;
using log4net;
using StarChef.Listener.Commands;
using StarChef.Listener.Exceptions;
using StarChef.Listener.Extensions;

namespace StarChef.Listener.Handlers
{
    public class AccountStatusChangedEventHandler : ListenerEventHandler, IMessageHandler<Events.AccountStatusChanged>
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public AccountStatusChangedEventHandler(IDatabaseCommands dbCommands, IEventValidator validator, IMessagingLogger messagingLogger) : base(dbCommands, validator, messagingLogger)
        {
        }

        public async Task<MessageHandlerResult> HandleAsync(Events.AccountStatusChanged payload, string trackingId)
        {
            ThreadContext.Properties[EXTERNAL_ID] = payload.ExternalId;

            if (Validator.IsStarChefEvent(payload))
            {
                _logger.EventReceived(trackingId, payload);

                if (!Validator.IsEnabled(payload))
                {
                    _logger.InfoFormat("Processing of the event is disabled for organization.");
                    ThreadContext.Properties.Remove(EXTERNAL_ID);
                    return MessageHandlerResult.Success;
                }

                if (Validator.IsValid(payload))
                {
                    try
                    {
                        await MessagingLogger.MessageProcessedSuccessfully(payload, trackingId);
                        _logger.Processed(trackingId, payload);
                    }
                    catch (ListenerException ex)
                    {
                        _logger.ListenerException(ex, trackingId, payload);
                        ThreadContext.Properties.Remove(EXTERNAL_ID);
                        return MessageHandlerResult.Fatal;
                    }
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