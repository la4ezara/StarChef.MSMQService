using System.Reflection;
using System.Threading.Tasks;
using System.Transactions;
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
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public AccountCreateFailedEventHandler(IDatabaseCommands dbCommands, IEventValidator validator, IMessagingLogger messagingLogger) : base(dbCommands, validator, messagingLogger)
        {
        }

        public async Task<MessageHandlerResult> HandleAsync(Events.AccountCreateFailed payload, string trackingId)
        {
            _logger.EventReceived(trackingId, payload);

            if (Validator.IsStarChefEvent(payload))
                if (Validator.IsValid(payload))
                {
                    var operationFailed = Mapper.Map<AccountCreateFailedTransferObject>(payload);

                    await MessagingLogger.ReceivedFailedMessage(operationFailed, trackingId);
                    await DbCommands.DisableLogin(operationFailed.LoginId);
                    _logger.Processed(trackingId, payload);
                }
                else
                {
                    var errors = Validator.GetErrors();
                    _logger.InvalidModel(trackingId, payload, errors);
                    await MessagingLogger.ReceivedInvalidModel(trackingId, payload, errors);
                    return MessageHandlerResult.Fatal;
                }
            return MessageHandlerResult.Success;
        }
    }
}