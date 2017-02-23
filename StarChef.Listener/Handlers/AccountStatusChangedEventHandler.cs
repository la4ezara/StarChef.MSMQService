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
    public class AccountStatusChangedEventHandler : ListenerEventHandler, IMessageHandler<Events.AccountStatusChanged>
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public AccountStatusChangedEventHandler(IDatabaseCommands dbCommands, IEventValidator validator, IMessagingLogger messagingLogger) : base(dbCommands, validator, messagingLogger)
        {
        }

        public async Task<MessageHandlerResult> HandleAsync(Events.AccountStatusChanged payload, string trackingId)
        {
            if (Validator.IsStarChefEvent(payload))
            {
                _logger.EventReceived(trackingId, payload);

                if (!Validator.IsEnabled(payload))
                {
                    _logger.EventDisabledForOrganization(payload);
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
                        return MessageHandlerResult.Fatal;
                    }
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