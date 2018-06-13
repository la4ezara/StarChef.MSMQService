using StarChef.Listener.Handlers;
using System;
using Fourth.Orchestration.Messaging;
using Moq;
using StarChef.Listener.Commands;
using Xunit;
using AccountCreateFailedReason = Fourth.Orchestration.Model.People.Events.AccountCreateFailedReason;
using AccountCreateFailed = Fourth.Orchestration.Model.People.Events.AccountCreateFailed;
using SourceSystem = Fourth.Orchestration.Model.People.Events.SourceSystem;
using StarChef.Listener.Tests.Fixtures;
using StarChef.Listener.Types;
using StarChef.Listener.Validators;
using log4net;
using StarChef.Listener.Tests.Helpers;
using log4net.Core;
using StarChef.Orchestrate.Models.TransferObjects;

namespace StarChef.Listener.Tests.Handlers
{
    public class AccountCreateFailedEventHandlerTests : IClassFixture<ObjectMappingFixture>
    {
        [Fact]
        public void Should_updated_valid_data_and_log_to_messaging_events()
        {
            var builder = AccountCreateFailed.CreateBuilder();
            builder
                .SetInternalId("1")
                .SetReason(AccountCreateFailedReason.INVALID_CREATE_DATA)
                .SetSource(SourceSystem.STARCHEF);
            var payload = builder.Build();

            var dbCommands = new Mock<IDatabaseCommands>();
            var validator = new AccountCreateFailedValidator(dbCommands.Object);
            var messagingLogger = new Mock<IMessagingLogger>();
            var handler = new AccountCreateFailedEventHandler(dbCommands.Object, validator, messagingLogger.Object);

            var result = handler.HandleAsync(payload, "1").Result;

            // assertions
            Assert.Equal(MessageHandlerResult.Success, result);
            messagingLogger.Verify(m => m.ReceivedFailedMessage(It.IsAny<FailedTransferObject>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Should_do_nothing_for_nonStarchef_event()
        {
            var builder = AccountCreateFailed.CreateBuilder();
            builder
                .SetInternalId("1")
                .SetReason(AccountCreateFailedReason.INVALID_CREATE_DATA)
                .SetSource(SourceSystem.ADACO);
            var payload = builder.Build();

            var dbCommands = new Mock<IDatabaseCommands>(MockBehavior.Strict);
            var validator = new AccountCreateFailedValidator(dbCommands.Object);
            var messagingLogger = new Mock<IMessagingLogger>(MockBehavior.Strict);
            var handler = new AccountCreateFailedEventHandler(dbCommands.Object, validator, messagingLogger.Object);

            var result = handler.HandleAsync(payload, "1").Result;

            // assertions
            Assert.Equal(MessageHandlerResult.Success, result);
        }
        
        [Fact]
        public void Should_not_have_log_for_non_starchef_events()
        {
            // arrange
            var builder = AccountCreateFailed.CreateBuilder();
            builder
                .SetInternalId("1")
                .SetReason(AccountCreateFailedReason.INVALID_CREATE_DATA)
                .SetSource(SourceSystem.ADACO);
            var payload = builder.Build();

            var dbCommands = new Mock<IDatabaseCommands>();
            var validator = new Mock<IEventValidator>(MockBehavior.Strict);
            validator.Setup(m => m.IsAllowedEvent(payload)).Returns(false);

            var messagingLogger = new Mock<IMessagingLogger>();
            var logChecker = new LogChecker(typeof(AccountCreateFailedEventHandler), Level.All);
            var handler = new AccountCreateFailedEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object, logChecker.GetLogger());

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.Empty(messageList);

            Assert.Null(ThreadContext.Properties[AccountCreateFailedEventHandler.INTERNAL_ID]);
        }

        [Fact]
        public void All_logs_should_have_correct_internal_id()
        {
            // arrange
            var builder = AccountCreateFailed.CreateBuilder();
            builder
                .SetInternalId("1")
                .SetReason(AccountCreateFailedReason.INVALID_CREATE_DATA)
                .SetSource(SourceSystem.STARCHEF);
            var payload = builder.Build();

            var dbCommands = new Mock<IDatabaseCommands>();
            var validator = new Mock<IEventValidator>(MockBehavior.Strict);
            validator.Setup(m => m.IsAllowedEvent(payload)).Returns(true);

            validator.Setup(m => m.IsValidPayload(It.IsAny<object>())).Returns(true);
            var messagingLogger = new Mock<IMessagingLogger>();
            var logChecker = new LogChecker(typeof(AccountCreateFailedEventHandler), Level.All);
            var handler = new AccountCreateFailedEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object, logChecker.GetLogger());

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.All(messageList, item =>
            {
                Assert.Equal("1", item.Properties[AccountCreateFailedEventHandler.INTERNAL_ID]);
            });

            Assert.Null(ThreadContext.Properties[AccountCreateFailedEventHandler.INTERNAL_ID]);
        }

        [Fact]
        public void All_logs_should_have_correct_internal_id_invalid_payload()
        {
            // arrange
            var builder = AccountCreateFailed.CreateBuilder();
            builder
                .SetInternalId("1")
                .SetReason(AccountCreateFailedReason.INVALID_CREATE_DATA)
                .SetSource(SourceSystem.STARCHEF);
            var payload = builder.Build();

            var dbCommands = new Mock<IDatabaseCommands>();
            var validator = new Mock<IEventValidator>(MockBehavior.Strict);
            validator.Setup(m => m.IsAllowedEvent(payload)).Returns(true);

            validator.Setup(m => m.IsValidPayload(It.IsAny<object>())).Returns(false);
            validator.Setup(m => m.GetErrors()).Returns(string.Empty);
            var messagingLogger = new Mock<IMessagingLogger>();
            var logChecker = new LogChecker(typeof(AccountCreateFailedEventHandler), Level.All);
            var handler = new AccountCreateFailedEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object, logChecker.GetLogger());

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.All(messageList, item =>
            {
                Assert.Equal("1", item.Properties[AccountCreateFailedEventHandler.INTERNAL_ID]);
            });

            Assert.Null(ThreadContext.Properties[AccountCreateFailedEventHandler.INTERNAL_ID]);
        }
    }
}
