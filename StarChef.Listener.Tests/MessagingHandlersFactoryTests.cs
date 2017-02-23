using StarChef.Listener.Handlers;
using Xunit;
using PriceBandUpdated = Fourth.Orchestration.Model.Recipes.Events.PriceBandUpdated;
using AccountCreated = Fourth.Orchestration.Model.People.Events.AccountCreated;
using AccountCreateFailed = Fourth.Orchestration.Model.People.Events.AccountCreateFailed;
using AccountUpdated = Fourth.Orchestration.Model.People.Events.AccountUpdated;
using AccountUpdateFailed = Fourth.Orchestration.Model.People.Events.AccountUpdateFailed;

namespace StarChef.Listener.Tests
{
    public class MessagingHandlersFactoryTests
    {
        [Fact]
        public void Should_create_handler_for_PriceBandUpdated()
        {
            var factory = new MessagingHandlersFactory();
            var actual = factory.CreateHandler<PriceBandUpdated>();
            Assert.Equal(typeof(PriceBandEventHandler), actual.GetType());
        }

        [Fact]
        public void Should_create_handler_for_AccountCreated()
        {
            var factory = new MessagingHandlersFactory();
            var actual = factory.CreateHandler<AccountCreated>();
            Assert.Equal(typeof(AccountCreatedEventHandler), actual.GetType());
        }

        [Fact]
        public void Should_create_handler_for_AccountCreateFailed()
        {
            var factory = new MessagingHandlersFactory();
            var actual = factory.CreateHandler<AccountCreateFailed>();
            Assert.Equal(typeof(AccountCreateFailedEventHandler), actual.GetType());
        }

        [Fact]
        public void Should_create_handler_for_AccountUpdated()
        {
            var factory = new MessagingHandlersFactory();
            var actual = factory.CreateHandler<AccountUpdated>();
            Assert.Equal(typeof(AccountUpdatedEventHandler), actual.GetType());
        }

        [Fact]
        public void Should_create_handler_for_AccountUpdateFailed()
        {
            var factory = new MessagingHandlersFactory();
            var actual = factory.CreateHandler<AccountUpdateFailed>();
            Assert.Equal(typeof(AccountUpdateFailedEventHandler), actual.GetType());
        }
    }
}
