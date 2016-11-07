using System;
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
                    var user = Mapper.Map<UserTransferObject>(payload);

                    await DbCommands.UpdateUser(user);
                    await MessagingLogger.RegisterSuccess(payload, trackingId);
                }
                else
                {

                    var errors = Validator.GetErrors();
                    _logger.Error(string.Format("AccountUpdated message is received, but cannot be read. Tracking ID: {0}", trackingId));
                    await MessagingLogger.RegisterInvalidModel(errors, payload, trackingId);
                    return MessageHandlerResult.Fatal;
                }
            return MessageHandlerResult.Success;
        }
    }
}