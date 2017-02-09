using Fourth.Orchestration.Messaging;
using Moq;
using StarChef.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using StarChef.MSMQService;
using Google.ProtocolBuffers;
using Microsoft.WindowsAzure.Storage.Table;
using Xunit;

using DeactivateAccount = Fourth.Orchestration.Model.People.Commands.DeactivateAccount;
using SupplierUpdated = Fourth.Orchestration.Model.Menus.Events.SupplierUpdated;

using IngredientUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.IngredientUpdated.Builder;
using RecipeUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.RecipeUpdated.Builder;
using GroupUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.GroupUpdated.Builder;
using MenuUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.MenuUpdated.Builder;
using MealPeriodUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.MealPeriodUpdated.Builder;
using SupplierUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.SupplierUpdated.Builder;
using UserUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.UserUpdated.Builder;

using StarChef.Data;
using UpdateMessage = StarChef.MSMQService.UpdateMessage;
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
            var sender = new StarChefMessageSender(messagingFactory.Object, Mock.Of<IDatabaseManager>(), Mock.Of<IEventFactory>(), commandFactory);
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
                Mock.Of<IEventSetter<UserUpdatedBuilder>>());

            var sender = new StarChefMessageSender(messagingFactory.Object, Mock.Of<IDatabaseManager>(), eventFactory, Mock.Of<ICommandFactory>());
            
            // the message which is received from MSMQ
            var msg = new UpdateMessage
            {
                Action = (int)Constants.MessageActionType.EntityUpdated
            };
            
            sender.PublishUpdateEvent(msg);

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
