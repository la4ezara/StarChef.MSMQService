﻿using System;
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
        private readonly ILog _logger;

        public AccountUpdatedEventHandler(IDatabaseCommands dbCommands, IEventValidator validator, IMessagingLogger messagingLogger) : base(dbCommands, validator, messagingLogger)
        {
            _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public AccountUpdatedEventHandler(IDatabaseCommands dbCommands, IEventValidator validator, IMessagingLogger messagingLogger, ILog errorLogger) : base(dbCommands, validator, messagingLogger)
        {
            _logger = errorLogger;
        }

        public async Task<MessageHandlerResult> HandleAsync(Events.AccountUpdated payload, string trackingId)
        {
            ThreadContext.Properties[EXTERNAL_ID] = payload.ExternalId;
            try
            {
                if (Validator.IsAllowedEvent(payload))
                {
                    _logger.EventReceived(trackingId, payload);

                    if (Validator.IsValidPayload(payload))
                    {
                        var user = Mapper.Map<AccountUpdatedTransferObject>(payload);

                        try
                        {
                            var isUserExists = await DbCommands.IsUserExists(externalLoginId: user.ExternalLoginId);
                            if (isUserExists)
                            {
                                _logger.UpdatingUser(user);
                                await DbCommands.UpdateUser(user.ExternalLoginId, user.Username, user.FirstName, user.LastName, user.EmailAddress, user.PermissionSets);
                            }

                            await MessagingLogger.MessageProcessedSuccessfully(payload, trackingId);
                            _logger.Processed(trackingId, payload);
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
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return MessageHandlerResult.Fatal;
            }
            finally
            {
                ThreadContext.Properties.Remove(EXTERNAL_ID);
            }
            return MessageHandlerResult.Success;
        }
    }
}