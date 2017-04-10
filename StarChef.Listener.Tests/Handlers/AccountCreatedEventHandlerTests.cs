using StarChef.Listener.Handlers;
using System;
using Fourth.Orchestration.Messaging;
using Moq;
using StarChef.Listener.Commands;
using Xunit;
using AccountCreated = Fourth.Orchestration.Model.People.Events.AccountCreated;
using SourceSystem = Fourth.Orchestration.Model.People.Events.SourceSystem;
using StarChef.Orchestrate.Models.TransferObjects;
using StarChef.Listener.Tests.Fixtures;
using StarChef.Listener.Tests.Handlers.Fakes;
using StarChef.Listener.Validators;
using StarChef.Listener.Tests.Helpers;
using log4net.Core;
using StarChef.Listener.Exceptions;
using System.Linq;
using log4net;

namespace StarChef.Listener.Tests.Handlers
{
    public class AccountCreatedEventHandlerTests : IClassFixture<ObjectMappingFixture>
    {
        [Fact]
        public void Should_updated_valid_data_and_log_to_messaging_events()
        {
            var builder = AccountCreated.CreateBuilder();
            builder
                .SetInternalId("1")
                .SetUsername("1")
                .SetFirstName("1")
                .SetLastName("1")
                .SetEmailAddress("1")
                .SetSource(SourceSystem.STARCHEF)
                .SetExternalId(Guid.Empty.ToString());
            var payload = builder.Build();

            var dbCommands = new TestDatabaseCommands();
            var validator = new AccountCreatedValidator(dbCommands);
            var messagingLogger = new TestMessagingLogger();
            var handler = new AccountCreatedEventHandler(dbCommands, validator, messagingLogger);

            var result = handler.HandleAsync(payload, "1").Result;

            // assertions
            Assert.Equal(MessageHandlerResult.Success, result);
            Assert.True(dbCommands.IsExternalIdUpdated);
            Assert.True(messagingLogger.IsSuccessfulOperationRegistered);
        }

        [Fact]
        public void Should_do_nothing_for_nonStarchef_event()
        {
            var builder = AccountCreated.CreateBuilder();
            builder
                .SetInternalId("1")
                .SetFirstName("1")
                .SetLastName("1")
                .SetEmailAddress("1")
                .SetSource(SourceSystem.ADACO)
                .SetExternalId(Guid.Empty.ToString());
            var payload = builder.Build();

            var dbCommands = new TestDatabaseCommands();
            var validator = new AccountCreatedValidator(dbCommands);
            var messagingLogger = new TestMessagingLogger();
            var handler = new AccountCreatedEventHandler(dbCommands, validator, messagingLogger);

            var result = handler.HandleAsync(payload, "1").Result;

            // assertions
            Assert.Equal(MessageHandlerResult.Success, result);
            Assert.False(dbCommands.IsCalledAnyMethod);
            Assert.False(messagingLogger.IsCalledAnyMethod);
        }

        [Fact]
        public void Should_register_error_with_model()
        {
            // arrange
            var payload = PayloadHelpers.Construct<AccountCreated>();
            var dbCommands = new Mock<IDatabaseCommands>();
            var validator = new Mock<IEventValidator>();
            validator.Setup(m => m.IsAllowedEvent(It.IsAny<object>())).Returns(true);
            validator.Setup(m => m.IsValidPayload(It.IsAny<object>())).Returns(false);
            var messagingLogger = new Mock<IMessagingLogger>();
            var handler = new AccountCreatedEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object);

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            // assert
            Assert.Equal(MessageHandlerResult.Fatal, result);
            messagingLogger.Verify(m => m.ReceivedInvalidModel(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()), Times.Once);
        }
        
        [Fact]
        public void Should_not_have_log_for_non_starchef_events()
        {
            // arrange
            var builder = AccountCreated.CreateBuilder();
            builder
                .SetInternalId("1")
                .SetFirstName("1")
                .SetLastName("1")
                .SetEmailAddress("1")
                .SetSource(SourceSystem.ADACO)
                .SetExternalId(Guid.Empty.ToString());
            var payload = builder.Build();

            var dbCommands = new Mock<IDatabaseCommands>();
            var validator = new Mock<IEventValidator>(MockBehavior.Strict);
            validator.Setup(m => m.IsAllowedEvent(payload)).Returns(false);

            var messagingLogger = new Mock<IMessagingLogger>();
            var logChecker = new LogChecker(typeof(AccountCreatedEventHandler), Level.All);
            var handler = new AccountCreatedEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object, logChecker.GetLogger());

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.Equal(messageList.Count, 0);

            Assert.Null(ThreadContext.Properties[AccountCreatedEventHandler.INTERNAL_ID]);
        }

        [Fact]
        public void All_logs_should_have_correct_internal_id()
        {
            // arrange
            var builder = AccountCreated.CreateBuilder();
            builder
                .SetInternalId("1")
                .SetFirstName("1")
                .SetLastName("1")
                .SetEmailAddress("1")
                .SetSource(SourceSystem.STARCHEF)
                .SetExternalId(Guid.Empty.ToString());
            var payload = builder.Build();

            var dbCommands = new Mock<IDatabaseCommands>();
            var validator = new Mock<IEventValidator>(MockBehavior.Strict);
            validator.Setup(m => m.IsAllowedEvent(payload)).Returns(true);

            validator.Setup(m => m.IsValidPayload(It.IsAny<object>())).Returns(true);
            var messagingLogger = new Mock<IMessagingLogger>();
            var logChecker = new LogChecker(typeof(AccountCreatedEventHandler), Level.All);
            var handler = new AccountCreatedEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object, logChecker.GetLogger());

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.All(messageList, item =>
            {
                Assert.Equal(item.Properties[AccountCreatedEventHandler.INTERNAL_ID], "1");
            });

            Assert.Null(ThreadContext.Properties[AccountCreatedEventHandler.INTERNAL_ID]);
        }

        [Fact]
        public void All_logs_should_have_correct_internal_id_invalid_payload()
        {
            // arrange
            var builder = AccountCreated.CreateBuilder();
            builder
                .SetInternalId("1")
                .SetFirstName("1")
                .SetLastName("1")
                .SetEmailAddress("1")
                .SetSource(SourceSystem.STARCHEF)
                .SetExternalId(Guid.Empty.ToString());
            var payload = builder.Build();

            var dbCommands = new Mock<IDatabaseCommands>();
            var validator = new Mock<IEventValidator>(MockBehavior.Strict);
            validator.Setup(m => m.IsAllowedEvent(payload)).Returns(true);

            validator.Setup(m => m.IsValidPayload(It.IsAny<object>())).Returns(false);
            validator.Setup(m => m.GetErrors()).Returns(string.Empty);
            var messagingLogger = new Mock<IMessagingLogger>();
            var logChecker = new LogChecker(typeof(AccountCreatedEventHandler), Level.All);
            var handler = new AccountCreatedEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object, logChecker.GetLogger());

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.All(messageList, item =>
            {
                Assert.Equal(item.Properties[AccountCreatedEventHandler.INTERNAL_ID], "1");
            });

            Assert.Null(ThreadContext.Properties[AccountCreatedEventHandler.INTERNAL_ID]);
        }

        [Fact]
        public void Should_log_listener_exceptions_and_have_correct_internal_id()
        {
            // arrange
            var builder = AccountCreated.CreateBuilder();
            builder
                .SetInternalId("1")
                .SetFirstName("1")
                .SetLastName("1")
                .SetEmailAddress("1")
                .SetSource(SourceSystem.STARCHEF)
                .SetExternalId(Guid.Empty.ToString());
            var payload = builder.Build();

            var dbCommands = new Mock<IDatabaseCommands>();
            dbCommands.Setup(d => d.IsUserExists(It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>())).Throws(new ListenerException());
            var validator = new Mock<IEventValidator>(MockBehavior.Strict);
            validator.Setup(m => m.IsAllowedEvent(payload)).Returns(true);

            validator.Setup(m => m.IsValidPayload(It.IsAny<object>())).Returns(true);
            var messagingLogger = new Mock<IMessagingLogger>();
            var logChecker = new LogChecker(typeof(AccountCreatedEventHandler), Level.All);
            var handler = new AccountCreatedEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object, logChecker.GetLogger());

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.All(messageList, item =>
            {
                Assert.Equal(item.Properties[AccountCreatedEventHandler.INTERNAL_ID], "1");
            });

            Assert.NotNull(
                messageList.Where(
                    item => 
                        item.ExceptionObject != null &&
                        item.ExceptionObject.GetType() == typeof(ListenerException)).FirstOrDefault()
                );

            Assert.Null(ThreadContext.Properties[AccountCreatedEventHandler.INTERNAL_ID]);
        }
    }
}
