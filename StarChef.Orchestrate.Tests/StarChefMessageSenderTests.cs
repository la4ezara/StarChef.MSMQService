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
using Xunit;

using DeactivateAccount = Fourth.Orchestration.Model.People.Commands.DeactivateAccount;
using DeactivateAccountBuilder = Fourth.Orchestration.Model.People.Commands.DeactivateAccount.Builder;
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
    }
}
