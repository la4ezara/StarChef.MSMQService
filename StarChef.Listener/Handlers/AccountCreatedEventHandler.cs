using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Model.People;
using log4net;
using StarChef.Listener.Commands;
using StarChef.Listener.Exceptions;
using StarChef.Orchestrate.Models.TransferObjects;
using StarChef.Listener.Extensions;
using System.Transactions;

namespace StarChef.Listener.Handlers
{
    public class AccountCreatedEventHandler : ListenerEventHandler, IMessageHandler<Events.AccountCreated>
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public AccountCreatedEventHandler(IDatabaseCommands dbCommands, IEventValidator validator, IMessagingLogger messagingLogger) : base(dbCommands, validator, messagingLogger)
        {
        }

        public async Task<MessageHandlerResult> HandleAsync(Events.AccountCreated payload, string trackingId)
        {
            _logger.EventReceived(trackingId, payload);

            if (Validator.IsStarChefEvent(payload))
                if (Validator.IsValid(payload))
                {
                    var user = Mapper.Map<AccountCreatedTransferObject>(payload);
                    try
                    {
                        using (var tran = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            await DbCommands.UpdateExternalId(user);
                            await MessagingLogger.MessageProcessedSuccessfully(payload, trackingId);
                            tran.Complete();
                            _logger.Processed(trackingId, payload);
                        }
                    }
                    catch (ListenerException ex)
                    {
                        _logger.ListenerException(ex, trackingId, user);
                        return MessageHandlerResult.Fatal;
                    }
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
