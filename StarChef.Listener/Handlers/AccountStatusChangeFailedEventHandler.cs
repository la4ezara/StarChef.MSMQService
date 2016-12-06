using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Model.People;
using log4net;
using StarChef.Listener.Commands;
using StarChef.Orchestrate.Models.TransferObjects;
using System.Transactions;
using StarChef.Listener.Extensions;

namespace StarChef.Listener.Handlers
{
    public class AccountStatusChangeFailedEventHandler : ListenerEventHandler, IMessageHandler<Events.AccountStatusChangeFailed>
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public AccountStatusChangeFailedEventHandler(IDatabaseCommands dbCommands, IEventValidator validator, IMessagingLogger messagingLogger) : base(dbCommands, validator, messagingLogger)
        {
        }

        public async Task<MessageHandlerResult> HandleAsync(Events.AccountStatusChangeFailed payload, string trackingId)
        {
            _logger.Info("AccountStatusChangeFailed received");

            if (Validator.IsStarChefEvent(payload))
            {
                _logger.EventReceived(trackingId, payload);

                if (Validator.IsValid(payload))
                {
                    var operationFailed = Mapper.Map<AccountUpdateFailedTransferObject>(payload);
                    await MessagingLogger.ReceivedFailedMessage(operationFailed, trackingId);
                    await DbCommands.DisableLogin(externalLoginId: operationFailed.ExternalLoginId);
                    _logger.Processed(trackingId, payload);
                }
                else
                {
                    var errors = Validator.GetErrors();
                    _logger.InvalidModel(trackingId, payload, errors);
                    await MessagingLogger.ReceivedInvalidModel(trackingId, payload, errors);
                    return MessageHandlerResult.Fatal;
                }}
            return MessageHandlerResult.Success;
        }
    }
}