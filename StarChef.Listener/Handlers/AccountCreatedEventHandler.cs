using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Fourth.Orchestration.Messaging;
using log4net;
using StarChef.Listener.Commands;
using StarChef.Listener.Exceptions;
using StarChef.Orchestrate.Models.TransferObjects;
using StarChef.Listener.Extensions;
using SourceSystem = Fourth.Orchestration.Model.People.Events.SourceSystem;
using AccountCreated = Fourth.Orchestration.Model.People.Events.AccountCreated;

namespace StarChef.Listener.Handlers
{
    public delegate Task AccountCreatedProcessedDelegate(AccountCreatedEventHandler sender, AccountCreatedTransferObject user, IConfiguration config);

    public class AccountCreatedEventHandler : ListenerEventHandler, IMessageHandler<AccountCreated>
    {
        private readonly ILog _logger;
        private readonly IConfiguration _config;

        public AccountCreatedEventHandler(IDatabaseCommands dbCommands, IEventValidator validator, IConfiguration config, IMessagingLogger messagingLogger) : base(dbCommands, validator, messagingLogger)
        {
            _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public AccountCreatedEventHandler(IDatabaseCommands dbCommands, IEventValidator validator, IMessagingLogger messagingLogger, IConfiguration config, ILog errorLogger) : base(dbCommands, validator, messagingLogger)
        {
            _logger = errorLogger;
        }

        public event AccountCreatedProcessedDelegate OnProcessed;

        public async Task<MessageHandlerResult> HandleAsync(AccountCreated payload, string trackingId)
        {
            ThreadContext.Properties[INTERNAL_ID] = payload.InternalId;

            try
            {
                if (Validator.IsAllowedEvent(payload))
                {
                    _logger.EventReceived(trackingId, payload);

                    if (Validator.IsValidPayload(payload))
                    {
                        AccountCreatedTransferObject user = null;
                        try
                        {
                            user = Mapper.Map<AccountCreatedTransferObject>(payload);
                            var isUserExists = await DbCommands.IsUserExists(user.LoginId, username: user.Username);
                            if (isUserExists)
                            {
                                /* NOTE
                                 * LoginId is missing in the system when the event is issued 
                                 * #1 by another system OR
                                 * #2 be user management API (as of 18/Apr/17).
                                 * The operation to originate loginId will lookup database for the actual value.
                                 * NB: it should be originated always because User Management send event with StarChef source system.
                                 */
                                user.LoginId = await DbCommands.OriginateLoginId(user.LoginId, user.Username);
                                _logger.UpdatingUserExternalId(user);
                                await DbCommands.UpdateExternalId(user);
                            }
                            else
                            {
                                _logger.AddingUser(user);
                                user.LoginId = await DbCommands.AddUser(user);
                            }

                            await MessagingLogger.MessageProcessedSuccessfully(payload, trackingId);
                            _logger.Processed(trackingId, payload);

                            // run subscribed post-events
                            var evt = OnProcessed;
                            if (evt != null)
                            {
                                _logger.Info("Post-processing the event");
                                await evt(this, user, _config);
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
                }
            }
            catch (System.Exception e)
            {
                _logger.Error(e);
                return MessageHandlerResult.Fatal;
            }
            finally
            {
                ThreadContext.Properties.Remove(INTERNAL_ID);
            }
            return MessageHandlerResult.Success;
        }
    }
}
