using StarChef.Listener.Handlers;
using System;
using Fourth.Orchestration.Messaging;
using Moq;
using StarChef.Listener.Commands;
using Xunit;
using AccountUpdateFailedReason = Fourth.Orchestration.Model.People.Events.AccountUpdateFailedReason;
using AccountUpdateFailed = Fourth.Orchestration.Model.People.Events.AccountUpdateFailed;
using SourceSystem = Fourth.Orchestration.Model.People.Events.SourceSystem;
using StarChef.Listener.Tests.Fixtures;
using StarChef.Listener.Tests.Handlers.Fakes;
using StarChef.Listener.Types;
using StarChef.Listener.Validators;
using log4net;
using log4net.Core;
using StarChef.Listener.Tests.Helpers;

namespace StarChef.Listener.Tests.Handlers
{
    public class AccountUpdateFailedEventHandlerTests : IClassFixture<ObjectMappingFixture>
    {
        [Fact]
        public void Should_updated_valid_data_and_log_to_messaging_events()
        {
            var builder = AccountUpdateFailed.CreateBuilder();
            builder
                .SetCommandId("1")
                .SetExternalId("1")
                .SetReason(AccountUpdateFailedReason.INVALID_UPDATE_DATA)
                .SetSource(SourceSystem.STARCHEF);
            var payload = builder.Build();

            var dbCommands = new TestDatabaseCommands();
            var validator = new AccountUpdateFailedValidator(dbCommands);
            var messagingLogger = new TestMessagingLogger();
            var handler = new AccountUpdateFailedEventHandler(dbCommands, validator, messagingLogger);

            var result = handler.HandleAsync(payload, "1").Result;

            // assertions
            Assert.Equal(MessageHandlerResult.Success, result);
            Assert.True(messagingLogger.IsFailedMessageRegistered);
        }

        [Fact]
        public void Should_do_nothing_for_nonStarchef_event()
        {
            var builder = AccountUpdateFailed.CreateBuilder();
            builder
                .SetCommandId("1")
                .SetExternalId("1")
                .SetReason(AccountUpdateFailedReason.INVALID_UPDATE_DATA)
                .SetSource(SourceSystem.ADACO);
            var payload = builder.Build();

            var dbCommands = new TestDatabaseCommands();
            var validator = new AccountUpdateFailedValidator(dbCommands);
            var messagingLogger = new TestMessagingLogger();
            var handler = new AccountUpdateFailedEventHandler(dbCommands, validator, messagingLogger);

            var result = handler.HandleAsync(payload, "1").Result;

            // assertions
            Assert.Equal(MessageHandlerResult.Success, result);
            Assert.False(dbCommands.IsCalledAnyMethod);
            Assert.False(messagingLogger.IsCalledAnyMethod);
        }
        
        [Fact]
        public void Should_not_have_log_for_non_starchef_events()
        {
            // arrange
            var builder = AccountUpdateFailed.CreateBuilder();
            builder
                .SetCommandId("1")
                .SetExternalId("1")
                .SetReason(AccountUpdateFailedReason.INVALID_UPDATE_DATA)
                .SetSource(SourceSystem.ADACO);
            var payload = builder.Build();

            var dbCommands = new Mock<IDatabaseCommands>();
            var validator = new Mock<IEventValidator>(MockBehavior.Strict);
            validator.Setup(m => m.IsStarChefEvent(payload)).Returns(false);

            var messagingLogger = new Mock<IMessagingLogger>();
            var handler = new AccountUpdateFailedEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object);
            var logChecker = new LogChecker(handler.GetType(), Level.All);

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.Equal(messageList.Count, 0);

            Assert.Null(ThreadContext.Properties[AccountUpdateFailedEventHandler.EXTERNAL_ID]);
        }

        [Fact]
        public void All_logs_should_have_correct_external_id()
        {
            // arrange
            var builder = AccountUpdateFailed.CreateBuilder();
            builder
                .SetCommandId("1")
                .SetExternalId("1")
                .SetReason(AccountUpdateFailedReason.INVALID_UPDATE_DATA)
                .SetSource(SourceSystem.STARCHEF);
            var payload = builder.Build();

            var dbCommands = new Mock<IDatabaseCommands>();
            var validator = new Mock<IEventValidator>(MockBehavior.Strict);
            validator.Setup(m => m.IsStarChefEvent(payload)).Returns(true);

            validator.Setup(m => m.IsValid(It.IsAny<object>())).Returns(true);
            var messagingLogger = new Mock<IMessagingLogger>();
            var handler = new AccountUpdateFailedEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object);
            var logChecker = new LogChecker(handler.GetType(), Level.All);

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.All(messageList, item =>
            {
                Assert.Equal(item.Properties[AccountUpdateFailedEventHandler.EXTERNAL_ID], "1");
            });

            Assert.Null(ThreadContext.Properties[AccountUpdateFailedEventHandler.EXTERNAL_ID]);
        }

        [Fact]
        public void All_logs_should_have_correct_external_id_invalid_payload()
        {
            // arrange
            var builder = AccountUpdateFailed.CreateBuilder();
            builder
                .SetCommandId("1")
                .SetExternalId("1")
                .SetReason(AccountUpdateFailedReason.INVALID_UPDATE_DATA)
                .SetSource(SourceSystem.STARCHEF);
            var payload = builder.Build();

            var dbCommands = new Mock<IDatabaseCommands>();
            var validator = new Mock<IEventValidator>(MockBehavior.Strict);
            validator.Setup(m => m.IsStarChefEvent(payload)).Returns(true);

            validator.Setup(m => m.IsValid(It.IsAny<object>())).Returns(false);
            validator.Setup(m => m.GetErrors()).Returns(string.Empty);
            var messagingLogger = new Mock<IMessagingLogger>();
            var handler = new AccountUpdateFailedEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object);
            var logChecker = new LogChecker(handler.GetType(), Level.All);

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.All(messageList, item =>
            {
                Assert.Equal(item.Properties[AccountUpdateFailedEventHandler.EXTERNAL_ID], "1");
            });

            Assert.Null(ThreadContext.Properties[AccountUpdateFailedEventHandler.EXTERNAL_ID]);
        }
    }
}
