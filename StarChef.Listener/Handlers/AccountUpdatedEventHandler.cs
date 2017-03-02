using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Model.People;
using log4net;
using StarChef.Listener.Commands;
using StarChef.Listener.Exceptions;
using StarChef.Listener.Extensions;
using StarChef.Orchestrate.Models.TransferObjects;

namespace StarChef.Listener.Handlers
{
    public class AccountUpdatedEventHandler : ListenerEventHandler, IMessageHandler<Events.AccountUpdated>
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public AccountUpdatedEventHandler(IDatabaseCommands dbCommands, IEventValidator validator, IMessagingLogger messagingLogger) : base(dbCommands, validator, messagingLogger)
        {
        }

        public async Task<MessageHandlerResult> HandleAsync(Events.AccountUpdated payload, string trackingId)
        {
            ThreadContext.Properties[EXTERNAL_ID] = payload.ExternalId;

            if (Validator.IsStarChefEvent(payload))
            {
                _logger.EventReceived(trackingId, payload);

                if (Validator.IsValid(payload))
                {
                    var user = Mapper.Map<AccountUpdatedTransferObject>(payload);

                    try
                    {
                        await MessagingLogger.MessageProcessedSuccessfully(payload, trackingId);
                        _logger.Processed(trackingId, payload);
                    }
                    catch (ListenerException ex)
                    {
                        _logger.ListenerException(ex, trackingId, user);
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