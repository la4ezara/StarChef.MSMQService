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

namespace StarChef.Listener
{
    class MessagingHandlersFactory : IMessagingHandlersFactory
    {
        /// <exception cref="NotSupportedMessageException">Raised when message type is not supported</exception>
        public IMessageHandler CreateHandler<T>()
        {
            var csProvider = new ConnectionStringProvider();

            if (typeof(T) == typeof(PriceBandUpdated))
            {
                var dbCommands = new PriceBandCommands(csProvider);
                var validator = new AlwaysTrueEventValidator();
                var messagingLogger = new MessagingLogger(dbCommands);
                return new PriceBandEventHandler(dbCommands, validator, messagingLogger);
            }

            if (typeof(T) == typeof(AccountCreated))
            {
                var dbCommands = new AccountCreatedCommands(csProvider);
                var validator = new AccountCreatedValidator();
                var messagingLogger = new MessagingLogger(dbCommands);
                return new AccountCreatedEventHandler(dbCommands, validator, messagingLogger);
            }

            if (typeof(T) == typeof(AccountCreateFailed))
            {
                var dbCommands = new AccountCreateFailedCommands(csProvider);
                var validator = new AccountCreateFailedValidator();
                var messagingLogger = new MessagingLogger(dbCommands);
                return new AccountCreateFailedEventHandler(dbCommands, validator, messagingLogger);
            }

            if (typeof(T) == typeof(AccountUpdated))
            {
                var dbCommands = new AccountUpdatedCommands(csProvider);
                var validator = new AccountUpdatedValidator();
                var messagingLogger = new MessagingLogger(dbCommands);
                return new AccountUpdatedEventHandler(dbCommands, validator, messagingLogger);
            }

            if (typeof(T) == typeof(AccountUpdateFailed))
            {
                var dbCommands = new AccountUpdateFailedCommands(csProvider);
                var validator = new AccountUpdateFailedValidator();
                var messagingLogger = new MessagingLogger(dbCommands);
                return new AccountUpdateFailedEventHandler(dbCommands, validator, messagingLogger);
            }

            throw new NotSupportedMessageException(string.Format("Message type {0} is not supported.", typeof(T).Name));
        }
    }
}
