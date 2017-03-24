﻿using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Model.People;
using log4net;
using StarChef.Listener.Commands;
using StarChef.Listener.Exceptions;
using StarChef.Orchestrate.Models.TransferObjects;
using StarChef.Listener.Extensions;

namespace StarChef.Listener.Handlers
{
    public delegate Task AccountCreatedProcessedDelegate(AccountCreatedEventHandler sender, AccountCreatedTransferObject user);

    public class AccountCreatedEventHandler : ListenerEventHandler, IMessageHandler<Events.AccountCreated>
    {
        private readonly ILog _logger;

        public AccountCreatedEventHandler(IDatabaseCommands dbCommands, IEventValidator validator, IMessagingLogger messagingLogger) : base(dbCommands, validator, messagingLogger)
        {
            _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public AccountCreatedEventHandler(IDatabaseCommands dbCommands, IEventValidator validator, IMessagingLogger messagingLogger, ILog errorLogger) : base(dbCommands, validator, messagingLogger)
        {
            _logger = errorLogger;
        }

        public event AccountCreatedProcessedDelegate OnProcessed;

        public async Task<MessageHandlerResult> HandleAsync(Events.AccountCreated payload, string trackingId)
        {
            ThreadContext.Properties[INTERNAL_ID] = payload.InternalId;

            if (Validator.IsStarChefEvent(payload))
            {
                _logger.EventReceived(trackingId, payload);

                if (Validator.IsValid(payload))
                {
                    var user = Mapper.Map<AccountCreatedTransferObject>(payload);
                    try
                    {
                        await DbCommands.AddUser(user);//!!!!!!!!! to be removed
                        await DbCommands.UpdateExternalId(user);
                        await MessagingLogger.MessageProcessedSuccessfully(payload, trackingId);
                        _logger.Processed(trackingId, payload);

                        // run subscribed post-events
                        var evt = OnProcessed;
                        if (evt != null)
                            await evt(this, user);
                    }
                    catch (ListenerException ex)
                    {
                        _logger.ListenerException(ex, trackingId, user);
                        ThreadContext.Properties.Remove(INTERNAL_ID);
                        return MessageHandlerResult.Fatal;
                    }
                }
                else
                {
                    var errors = Validator.GetErrors();
                    _logger.InvalidModel(trackingId, payload, errors);
                    await MessagingLogger.ReceivedInvalidModel(trackingId, payload, errors);
                    ThreadContext.Properties.Remove(INTERNAL_ID);
                    return MessageHandlerResult.Fatal;
                }}

            ThreadContext.Properties.Remove(INTERNAL_ID);
            return MessageHandlerResult.Success;
        }
    }
}
