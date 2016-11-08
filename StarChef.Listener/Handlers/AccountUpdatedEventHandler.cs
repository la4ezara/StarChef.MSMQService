using System;
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
using System.Transactions;

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
            if (Validator.IsStarChefEvent(payload))
                if (Validator.IsValid(payload))
                {
                    var user = Mapper.Map<AccountUpdatedTransferObject>(payload);

                    try
                    {
                        using (var tran = new TransactionScope())
                        {
                            await DbCommands.UpdateUser(user.ExternalLoginId, user.Username, user.FirstName, user.LastName, user.EmailAddress);
                            await MessagingLogger.MessageProcessedSuccessfully(payload, trackingId);
                            tran.Complete();
                        }
                    }
                    catch (ListenerException ex)
                    {
                        _logger.Error(string.Format("Exception of type {0} is occurred with message: {1}. Tracking ID: {2}", ex.GetType(), ex.Message, trackingId));
                        _logger.Error(string.Format("Exception data: {0}", user.ToJson()));
                        return MessageHandlerResult.Fatal;
                    }
                }
                else
                {

                    var error = Validator.GetErrors();
                    _logger.Error(string.Format("AccountUpdated message is received, but cannot be read. Tracking ID: {0}", trackingId));
                    await MessagingLogger.ReceivedInvalidModel(trackingId, payload, error);
                    return MessageHandlerResult.Fatal;
                }
            return MessageHandlerResult.Success;
        }
    }
}