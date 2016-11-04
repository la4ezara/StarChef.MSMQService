using Fourth.Orchestration.Messaging;
using StarChef.Listener.Commands.Impl;
using StarChef.Listener.Exceptions;
using StarChef.Listener.Handlers;

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
                return new PriceBandEventHandler(new PriceBandCommands(csProvider));

            if (typeof(T) == typeof(AccountCreated))
                return new AccountCreatedEventHandler(new AccountCreatedCommands(csProvider));

            if (typeof(T) == typeof(AccountCreateFailed))
                return new AccountCreateFailedEventHandler(new AccountCreateFailedCommands(csProvider));

            if (typeof(T) == typeof(AccountUpdated))
                return new AccountUpdatedEventHandler(new AccountUpdatedCommands(csProvider));

            if (typeof(T) == typeof(AccountUpdateFailed))
                return new AccountUpdateFailedEventHandler(new AccountUpdateFailedCommands(csProvider));

            throw new NotSupportedMessageException(string.Format("Message type {0} is not supported.", typeof(T).Name));
        }
    }
}
