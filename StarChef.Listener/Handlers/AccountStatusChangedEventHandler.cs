
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

namespace StarChef.Listener.Handlers
{
    public class AccountStatusChangedEventHandler : ListenerEventHandler, IMessageHandler<Events.AccountStatusChanged>
    {
        private readonly ILog _logger;

        public AccountStatusChangedEventHandler(IDatabaseCommands dbCommands, IEventValidator validator, IMessagingLogger messagingLogger) : base(dbCommands, validator, messagingLogger)
        {
            _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public AccountStatusChangedEventHandler(IDatabaseCommands dbCommands, IEventValidator validator, IMessagingLogger messagingLogger, ILog errorLogger) : base(dbCommands, validator, messagingLogger)
        {
            _logger = errorLogger;
        }

        public async Task<MessageHandlerResult> HandleAsync(Events.AccountStatusChanged payload, string trackingId)
        {
            ThreadContext.Properties[EXTERNAL_ID] = payload.ExternalId;

            try
            {
                if (Validator.IsAllowedEvent(payload))
                {
                    _logger.EventReceived(trackingId, payload);

                    if (Validator.IsValidPayload(payload))
                    {
                        try
                        {
                            var statusChanged = Mapper.Map<AccountStatusChangedTransferObject>(payload);
                            var isUserExists = await DbCommands.IsUserExists(externalLoginId: statusChanged.ExternalLoginId);
                            if (isUserExists)
                            {
                                if (statusChanged.IsActive)
                                {
                                    _logger.EnablingUser(statusChanged);
                                    await DbCommands.EnableLogin(externalLoginId: statusChanged.ExternalLoginId);
                                }
                                else
                                {
                                    _logger.DisablingUser(statusChanged);
                                    await DbCommands.DisableLogin(externalLoginId: statusChanged.ExternalLoginId);
                                }
                            }
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