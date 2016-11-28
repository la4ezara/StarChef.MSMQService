using System.Reflection;
using Fourth.Orchestration.Messaging;
using StarChef.Listener.Commands.Impl;
using StarChef.Listener.Exceptions;
using StarChef.Listener.Handlers;
using StarChef.Listener.Types;
using PriceBandUpdated = Fourth.Orchestration.Model.Recipes.Events.PriceBandUpdated;
using AccountCreated = Fourth.Orchestration.Model.People.Events.AccountCreated;
using AccountCreateFailed = Fourth.Orchestration.Model.People.Events.AccountCreateFailed;
using AccountUpdated = Fourth.Orchestration.Model.People.Events.AccountUpdated;
using AccountUpdateFailed = Fourth.Orchestration.Model.People.Events.AccountUpdateFailed;
using System.Threading.Tasks;
using log4net;
using StarChef.Data;
using StarChef.Orchestrate.Models.TransferObjects;
using MSMQHelper = StarChef.MSMQService.MSMQHelper;
using UpdateMessage = StarChef.MSMQService.UpdateMessage;
using StarChef.Listener.Extensions;

namespace StarChef.Listener
{
    class MessagingHandlersFactory : IMessagingHandlersFactory
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <exception cref="NotSupportedMessageException">Raised when message type is not supported</exception>
        public IMessageHandler CreateHandler<T>()
        {
            var csProvider = new ConnectionStringProvider();
            var dbCommands = new DatabaseCommands(csProvider);

            if (typeof(T) == typeof(PriceBandUpdated))
            {
                var validator = new AlwaysTrueEventValidator();
                var messagingLogger = new MessagingLogger(dbCommands);
                return new PriceBandEventHandler(dbCommands, validator, messagingLogger);
            }

            if (typeof(T) == typeof(AccountCreated))
            {
                var validator = new AccountCreatedValidator();
                var messagingLogger = new MessagingLogger(dbCommands);
                var handler = new AccountCreatedEventHandler(dbCommands, validator, messagingLogger);
                handler.OnProcessed += SendMsmqMessage;
                return handler;
            }

            if (typeof(T) == typeof(AccountCreateFailed))
            {
                var validator = new AccountCreateFailedValidator();
                var messagingLogger = new MessagingLogger(dbCommands);
                return new AccountCreateFailedEventHandler(dbCommands, validator, messagingLogger);
            }

            if (typeof(T) == typeof(AccountUpdated))
            {
                var validator = new AccountUpdatedValidator();
                var messagingLogger = new MessagingLogger(dbCommands);
                return new AccountUpdatedEventHandler(dbCommands, validator, messagingLogger);
            }

            if (typeof(T) == typeof(AccountUpdateFailed))
            {
                var validator = new AccountUpdateFailedValidator();
                var messagingLogger = new MessagingLogger(dbCommands);
                return new AccountUpdateFailedEventHandler(dbCommands, validator, messagingLogger);
            }

            throw new NotSupportedMessageException(string.Format("Message type {0} is not supported.", typeof(T).Name));
        }

        private async Task SendMsmqMessage(AccountCreatedEventHandler sender, AccountCreatedTransferObject user)
        {
            if (sender == null) return;

            var userDetail = await sender.DbCommands.GetLoginUserIdAndCustomerDb(user.LoginId);

            var msg = new UpdateMessage(productId: userDetail.Item1,
                                        entityTypeId: (int)Constants.EntityType.User,
                                        action: (int)Constants.MessageActionType.SalesForceUserCreated,
                                        dbDSN: userDetail.Item3,
                                        databaseId: userDetail.Item2);
            MSMQHelper.Send(msg);
            _logger.MessageSent(msg);
        }
    }
}
