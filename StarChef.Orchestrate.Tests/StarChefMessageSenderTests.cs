using Fourth.Orchestration.Messaging;
using Moq;
using StarChef.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Fourth.StarChef.Invariables;
using Google.ProtocolBuffers;
using Microsoft.WindowsAzure.Storage.Table;
using Xunit;
using Fourth.Orchestration.Model.Menus;

using DeactivateAccount = Fourth.Orchestration.Model.People.Commands.DeactivateAccount;
using SupplierUpdated = Fourth.Orchestration.Model.Menus.Events.SupplierUpdated;

using IngredientUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.IngredientUpdated.Builder;
using RecipeUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.RecipeUpdated.Builder;
using GroupUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.GroupUpdated.Builder;
using MenuUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.MenuUpdated.Builder;
using MealPeriodUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.MealPeriodUpdated.Builder;
using SupplierUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.SupplierUpdated.Builder;
using UserUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.UserUpdated.Builder;
using SetUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.SetUpdated.Builder;
using RecipeNutritionUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.RecipeNutritionUpdated.Builder;

using StarChef.Orchestrate.EventSetters.Impl;

namespace StarChef.Orchestrate.Tests
{
    public class StarChefMessageSenderTests
    {
        [Fact]
        public void Should_send_command_DeactivateAccount()
        {
            // mock bus interface to verify Send method later
            var bus = new Mock<IMessageBus>();
            // mock the factory to ensure it returns the mocked version of the bus
            var messagingFactory = new Mock<IMessagingFactory>();
            messagingFactory.Setup(m => m.CreateMessageBus()).Returns(bus.Object);
            // use real factory to create an actual command
            var commandFactory = new CommandFactory(new DeactivateAccountSetter());
            var databaseManager = new Mock<IDatabaseManager>();
            databaseManager.Setup(m => m.IsSsoEnabled(It.IsAny<string>())).Returns(true);
            databaseManager.Setup(m => m.IsPublishEnabled(It.IsAny<string>(), It.IsAny<int>())).Returns(true);
            var sender = new StarChefMessageSender(messagingFactory.Object, databaseManager.Object, Mock.Of<IEventFactory>(), commandFactory);
            // the message which is received from MSMQ
            var msg = new UpdateMessage
            {
                Action = (int) Constants.MessageActionType.UserDeActivated,
                ExternalId = "test"
            };
            
            sender.PublishCommand(msg);

            // verify that the command is passed to the Send method of the bus and it has correct values in properties
            bus.Verify(m => m.Send(It.Is<IMessage>(omsg => ((DeactivateAccount) omsg).ExternalId == "test")), Times.Once);
        }

        [Fact]
        public void Should_not_send_command_DeactivateAccount_if_publishing_disabled()
        {
            // mock bus interface to verify Send method later
            var bus = new Mock<IMessageBus>();
            // mock the factory to ensure it returns the mocked version of the bus
            var messagingFactory = new Mock<IMessagingFactory>();
            messagingFactory.Setup(m => m.CreateMessageBus()).Returns(bus.Object);
            // use real factory to create an actual command
            var commandFactory = new CommandFactory(new DeactivateAccountSetter());
            var databaseManager = new Mock<IDatabaseManager>();
            databaseManager.Setup(m => m.IsSsoEnabled(It.IsAny<string>())).Returns(true);
            databaseManager.Setup(m => m.IsPublishEnabled(It.IsAny<string>(), It.IsAny<int>())).Returns(false);
            var sender = new StarChefMessageSender(messagingFactory.Object, databaseManager.Object, Mock.Of<IEventFactory>(), commandFactory);
            // the message which is received from MSMQ
            var msg = new UpdateMessage
            {
                Action = (int)Constants.MessageActionType.UserDeActivated,
                ExternalId = "test"
            };

            sender.PublishCommand(msg);

            // verify that the command is passed to the Send method of the bus and it has correct values in properties
            bus.Verify(m => m.Send(It.IsAny<IMessage>()), Times.Never);
        }

        [Fact]
        public void Should_not_send_command_DeactivateAccount_if_sso_disabled()
        {
            // mock bus interface to verify Send method later
            var bus = new Mock<IMessageBus>();
            // mock the factory to ensure it returns the mocked version of the bus
            var messagingFactory = new Mock<IMessagingFactory>();
            messagingFactory.Setup(m => m.CreateMessageBus()).Returns(bus.Object);
            // use real factory to create an actual command
            var commandFactory = new CommandFactory(new DeactivateAccountSetter());
            var databaseManager = new Mock<IDatabaseManager>();
            databaseManager.Setup(m => m.IsSsoEnabled(It.IsAny<string>())).Returns(false);
            databaseManager.Setup(m => m.IsPublishEnabled(It.IsAny<string>(), It.IsAny<int>())).Returns(true);
            var sender = new StarChefMessageSender(messagingFactory.Object, databaseManager.Object, Mock.Of<IEventFactory>(), commandFactory);
            // the message which is received from MSMQ
            var msg = new UpdateMessage
            {
                Action = (int)Constants.MessageActionType.UserDeActivated,
                ExternalId = "test"
            };

            sender.PublishCommand(msg);

            // verify that the command is passed to the Send method of the bus and it has correct values in properties
            bus.Verify(m => m.Send(It.IsAny<IMessage>()), Times.Never);
        }

        [Theory]
        [InlineData((int)Constants.EntityType.Ingredient)]
        [InlineData((int)Constants.EntityType.Dish)]
        [InlineData((int)Constants.EntityType.User)]
        [InlineData((int)Constants.EntityType.UserGroup)]
        [InlineData((int)Constants.EntityType.Supplier)]
        [InlineData((int)Constants.EntityType.Group)]
        [InlineData((int)Constants.EntityType.MealPeriodManagement)]
        [InlineData((int)Constants.EntityType.Menu)]
        public void Should_not_send_to_orchestration_if_not_configured_for_entityType(int entityTypeId)
        {
            // mock bus interface to verify Send method later
            var bus = new Mock<IMessageBus>();
            // mock the factory to ensure it returns the mocked version of the bus
            var messagingFactory = new Mock<IMessagingFactory>();
            messagingFactory.Setup(m => m.CreateMessageBus()).Returns(bus.Object);
            // use real factory to create an actual command
            var commandFactory = new CommandFactory(new DeactivateAccountSetter());
            var databaseManager = new Mock<IDatabaseManager>();
            databaseManager.Setup(m => m.IsPublishEnabled(It.IsAny<string>(), It.IsAny<int>())).Returns(false);
            var sender = new StarChefMessageSender(messagingFactory.Object, databaseManager.Object, Mock.Of<IEventFactory>(), commandFactory);
            // the message which is received from MSMQ
            const int ANY_INT = 0;
            const string ANY_TEXT = "any";
            const EnumHelper.EntityTypeWrapper ANY_WRAPPER = EnumHelper.EntityTypeWrapper.Ingredient;
            sender.Send(ANY_WRAPPER, ANY_TEXT, entityTypeId, ANY_INT, ANY_TEXT, ANY_INT, DateTime.UtcNow);

            // verify that the command is passed to the Send method of the bus and it has correct values in properties
            bus.Verify(m => m.Send(It.IsAny<IMessage>()), Times.Never);
        }

        [Theory]
        [InlineData((int)Constants.EntityType.User, EnumHelper.EntityTypeWrapper.UserCreated)]
        [InlineData((int)Constants.EntityType.User, EnumHelper.EntityTypeWrapper.UserActivated)]
        [InlineData((int)Constants.EntityType.User, EnumHelper.EntityTypeWrapper.UserDeactivated)]
        public void Should_not_send_sso_related_commands_if_disabled(int entityTypeId, EnumHelper.EntityTypeWrapper entityTypeWrapper)
        {
            // mock bus interface to verify Send method later
            var bus = new Mock<IMessageBus>();
            // mock the factory to ensure it returns the mocked version of the bus
            var messagingFactory = new Mock<IMessagingFactory>();
            messagingFactory.Setup(m => m.CreateMessageBus()).Returns(bus.Object);
            // use real factory to create an actual command
            var commandFactory = new CommandFactory(new DeactivateAccountSetter());
            var databaseManager = new Mock<IDatabaseManager>();
            databaseManager.Setup(m => m.IsSsoEnabled(It.IsAny<string>())).Returns(false);
            databaseManager.Setup(m => m.IsPublishEnabled(It.IsAny<string>(), It.IsAny<int>())).Returns(true);
            var sender = new StarChefMessageSender(messagingFactory.Object, databaseManager.Object, Mock.Of<IEventFactory>(), commandFactory);
            // the message which is received from MSMQ
            const int ANY_INT = 0;
            const string ANY_TEXT = "any";
            sender.Send(entityTypeWrapper, ANY_TEXT, entityTypeId, ANY_INT, ANY_TEXT, ANY_INT, DateTime.UtcNow);

            // verify that the command is passed to the Send method of the bus and it has correct values in properties
            bus.Verify(m => m.Send(It.IsAny<IMessage>()), Times.Never);
            bus.Verify(m => m.Publish(It.IsAny<IMessage>()), Times.Never);
        }

        [Fact]
        public void Should_not_send_sso_related_command_if_disabled_but_send_event()
        {
            // mock bus interface to verify Send method later
            var bus = new Mock<IMessageBus>();
            // mock the factory to ensure it returns the mocked version of the bus
            var messagingFactory = new Mock<IMessagingFactory>();
            messagingFactory.Setup(m => m.CreateMessageBus()).Returns(bus.Object);
            // use real factory to create an actual command
            var commandFactory = new CommandFactory(new DeactivateAccountSetter());
            var databaseManager = new Mock<IDatabaseManager>();
            databaseManager.Setup(m => m.IsSsoEnabled(It.IsAny<string>())).Returns(false);
            databaseManager.Setup(m => m.IsPublishEnabled(It.IsAny<string>(), It.IsAny<int>())).Returns(true);
            var sender = new StarChefMessageSender(messagingFactory.Object, databaseManager.Object, Mock.Of<IEventFactory>(), commandFactory);
            // the message which is received from MSMQ
            const int ANY_INT = 0;
            const string ANY_TEXT = "any";
            // note SendUserUpdatedEventAndCommand is a special kind of wrapper
            sender.Send(EnumHelper.EntityTypeWrapper.SendUserUpdatedEventAndCommand, ANY_TEXT, (int)Constants.EntityType.User, ANY_INT, ANY_TEXT, ANY_INT, DateTime.UtcNow);

            // verify that the command is passed to the Send method of the bus and it has correct values in properties
            bus.Verify(m => m.Send(It.IsAny<IMessage>()), Times.Never);
            bus.Verify(m => m.Publish(It.IsAny<IMessage>()), Times.Once);
        }

        [Theory]
        [InlineData((int)Constants.EntityType.User, EnumHelper.EntityTypeWrapper.Recipe)]
        [InlineData((int)Constants.EntityType.User, EnumHelper.EntityTypeWrapper.Ingredient)]
        [InlineData((int)Constants.EntityType.User, EnumHelper.EntityTypeWrapper.MealPeriod)]
        [InlineData((int)Constants.EntityType.User, EnumHelper.EntityTypeWrapper.Menu)]
        public void Should_send_events_when_sso_related_commands_disabled(int entityTypeId, EnumHelper.EntityTypeWrapper entityTypeWrapper)
        {
            // mock bus interface to verify Send method later
            var bus = new Mock<IMessageBus>();
            // mock the factory to ensure it returns the mocked version of the bus
            var messagingFactory = new Mock<IMessagingFactory>();
            messagingFactory.Setup(m => m.CreateMessageBus()).Returns(bus.Object);
            // use real factory to create an actual command
            var commandFactory = new CommandFactory(new DeactivateAccountSetter());
            var databaseManager = new Mock<IDatabaseManager>();
            databaseManager.Setup(m => m.IsSsoEnabled(It.IsAny<string>())).Returns(false);
            databaseManager.Setup(m => m.IsPublishEnabled(It.IsAny<string>(), It.IsAny<int>())).Returns(true);
            
            IEventFactory eventFactory = Mock.Of<IEventFactory>();
            switch (entityTypeWrapper)
            {
                case EnumHelper.EntityTypeWrapper.Recipe:                
                case EnumHelper.EntityTypeWrapper.Ingredient:
                    databaseManager.Setup(m => m.IsSetOrchestrationSentDate(It.IsAny<string>(), It.IsAny<int>())).Returns(true);
                    break;
            }

            var sender = new StarChefMessageSender(messagingFactory.Object, databaseManager.Object, eventFactory, commandFactory);
            // the message which is received from MSMQ
            const int ANY_INT = 0;
            const string ANY_TEXT = "any";
            sender.Send(entityTypeWrapper, ANY_TEXT, entityTypeId, ANY_INT, ANY_TEXT, ANY_INT, DateTime.UtcNow);

            // verify that the command is passed to the Send method of the bus and it has correct values in properties
            bus.Verify(m => m.Send(It.IsAny<IMessage>()), Times.Never);
            bus.Verify(m => m.Publish(It.IsAny<IMessage>()), Times.Once);
        }

        [Fact]
        public void Should_send_event_UpdateSupplier()
        {
            // mock bus interface to verify Send method later
            var bus = new Mock<IMessageBus>();

            // mock the factory to ensure it returns the mocked version of the bus
            var messagingFactory = new Mock<IMessagingFactory>();
            messagingFactory.Setup(m => m.CreateMessageBus()).Returns(bus.Object);

            var eventSetter = new Mock<IEventSetter<SupplierUpdatedBuilder>>();
            eventSetter
                .Setup(m => m.SetForUpdate(It.IsAny<SupplierUpdatedBuilder>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Callback<SupplierUpdatedBuilder, string, int, int>((builder, s, i1, i2) =>
                {
                    SetMandatoryFields_forUpdate(builder, 22, i2);
                });

            var eventFactory = new EventFactory(
                Mock.Of<IEventSetter<IngredientUpdatedBuilder>>(),
                Mock.Of<IEventSetter<RecipeUpdatedBuilder>>(),
                Mock.Of<IEventSetter<GroupUpdatedBuilder>>(),
                Mock.Of<IEventSetter<MenuUpdatedBuilder>>(),
                Mock.Of<IEventSetter<MealPeriodUpdatedBuilder>>(),
                eventSetter.Object,
                Mock.Of<IEventSetter<UserUpdatedBuilder>>(),
                Mock.Of<IEventSetter<SetUpdatedBuilder>>(),
                Mock.Of<IEventSetter<RecipeNutritionUpdatedBuilder>>());

            var databaseManager = new Mock<IDatabaseManager>();
            databaseManager.Setup(m => m.IsPublishEnabled(It.IsAny<string>(), It.IsAny<int>())).Returns(true);
            var sender = new StarChefMessageSender(messagingFactory.Object, databaseManager.Object, eventFactory, Mock.Of<ICommandFactory>());

            // the message which is received from MSMQ
            var msg = new UpdateMessage
            {
                Action = (int)Constants.MessageActionType.StarChefEventsUpdated,
                EntityTypeId = (int)Constants.EntityType.Supplier
            };
            
            sender.Send(EnumHelper.EntityTypeWrapper.SendSupplierUpdatedEvent, string.Empty, msg.EntityTypeId, msg.ProductID, string.Empty, msg.DatabaseID, DateTime.UtcNow);

            // verify that the event is passed to the Publish method of the bus and it has correct values in properties
            bus.Verify(m => m.Publish(It.Is<IMessage>(omsg => ((SupplierUpdated)omsg).ExternalId == "22")), Times.Once);
        }

        private static void SetMandatoryFields_forUpdate(dynamic builder, int entityId, int databaseId)
        {
            builder.SetExternalId(entityId.ToString());
            builder.SetCustomerId(databaseId.ToString());
            builder.SetCustomerName(databaseId.ToString());
        }
    }
}
