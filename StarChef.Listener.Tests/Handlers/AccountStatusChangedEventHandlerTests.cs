using log4net;
using log4net.Core;
using Moq;
using StarChef.Listener.Commands;
using StarChef.Listener.Exceptions;
using StarChef.Listener.Handlers;
using StarChef.Listener.Tests.Fixtures;
using StarChef.Listener.Tests.Helpers;
using System;
using System.Linq;
using Xunit;
using static Fourth.Orchestration.Model.People.Events;

namespace StarChef.Listener.Tests.Handlers
{
    public class AccountStatusChangedEventHandlerTests : IClassFixture<ObjectMappingFixture>
    {
        [Fact]
        public void Should_not_have_log_for_non_starchef_events()
        {
            // arrange
            var builder = AccountStatusChanged.CreateBuilder();
            builder
                .SetSource(SourceSystem.ADACO)
                .SetExternalId(Guid.Empty.ToString())
                .SetStatus(AccountStatus.ACTIVE);
            var payload = builder.Build();


            var dbCommands = new Mock<IDatabaseCommands>();
            var validator = new Mock<IEventValidator>(MockBehavior.Strict);
            validator.Setup(m => m.IsAllowedEvent(payload)).Returns(false);

            var messagingLogger = new Mock<IMessagingLogger>();
            var logChecker = new LogChecker(typeof(AccountStatusChangedEventHandler), Level.All);
            var handler = new AccountStatusChangedEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object, logChecker.GetLogger());

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.Equal(messageList.Count, 0);

            Assert.Null(ThreadContext.Properties[AccountStatusChangedEventHandler.EXTERNAL_ID]);
        }

        [Fact]
        public void All_logs_should_have_correct_external_id()
        {
            // arrange
            var builder = AccountStatusChanged.CreateBuilder();
            builder
                .SetSource(SourceSystem.STARCHEF)
                .SetExternalId(Guid.Empty.ToString())
                .SetStatus(AccountStatus.ACTIVE);
            var payload = builder.Build();


            var dbCommands = new Mock<IDatabaseCommands>();
            var validator = new Mock<IEventValidator>(MockBehavior.Strict);
            validator.Setup(m => m.IsAllowedEvent(payload)).Returns(true);

            validator.Setup(m => m.IsValidPayload(It.IsAny<object>())).Returns(true);
            var messagingLogger = new Mock<IMessagingLogger>();
            var logChecker = new LogChecker(typeof(AccountStatusChangedEventHandler), Level.All);
            var handler = new AccountStatusChangedEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object, logChecker.GetLogger());

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.All(messageList, item =>
            {
                Assert.Equal(item.Properties[AccountStatusChangedEventHandler.EXTERNAL_ID], Guid.Empty.ToString());
            });

            Assert.Null(ThreadContext.Properties[AccountStatusChangedEventHandler.EXTERNAL_ID]);
        }

        [Fact]
        public void All_logs_should_have_correct_external_id_invalid_payload()
        {
            // arrange
            var builder = AccountStatusChanged.CreateBuilder();
            builder
                .SetSource(SourceSystem.STARCHEF)
                .SetExternalId(Guid.Empty.ToString())
                .SetStatus(AccountStatus.ACTIVE);
            var payload = builder.Build();

            var dbCommands = new Mock<IDatabaseCommands>();
            var validator = new Mock<IEventValidator>(MockBehavior.Strict);
            validator.Setup(m => m.IsAllowedEvent(payload)).Returns(true);

            validator.Setup(m => m.IsValidPayload(It.IsAny<object>())).Returns(false);
            validator.Setup(m => m.GetErrors()).Returns(string.Empty);
            var messagingLogger = new Mock<IMessagingLogger>();
            var logChecker = new LogChecker(typeof(AccountStatusChangedEventHandler), Level.All);
            var handler = new AccountStatusChangedEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object, logChecker.GetLogger());

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.All(messageList, item =>
            {
                Assert.Equal(item.Properties[AccountStatusChangedEventHandler.EXTERNAL_ID], Guid.Empty.ToString());
            });

            Assert.Null(ThreadContext.Properties[AccountStatusChangedEventHandler.EXTERNAL_ID]);
        }

        [Fact]
        public void Should_log_listener_exceptions_and_have_correct_external_id()
        {
            // arrange
            var builder = AccountStatusChanged.CreateBuilder();
            builder
                .SetSource(SourceSystem.STARCHEF)
                .SetExternalId(Guid.Empty.ToString())
                .SetStatus(AccountStatus.ACTIVE);
            var payload = builder.Build();

            var dbCommands = new Mock<IDatabaseCommands>();
            var validator = new Mock<IEventValidator>(MockBehavior.Strict);
            validator.Setup(m => m.IsAllowedEvent(payload)).Returns(true);

            validator.Setup(m => m.IsValidPayload(It.IsAny<object>())).Returns(true);
            var messagingLogger = new Mock<IMessagingLogger>();
            messagingLogger.Setup(d => d.MessageProcessedSuccessfully(It.IsAny<object>(), It.IsAny<string>())).Throws(new ListenerException());
            var logChecker = new LogChecker(typeof(AccountStatusChangedEventHandler), Level.All);
            var handler = new AccountStatusChangedEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object, logChecker.GetLogger());

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.All(messageList, item =>
            {
                Assert.Equal(item.Properties[AccountStatusChangedEventHandler.EXTERNAL_ID], Guid.Empty.ToString());
            });

            Assert.NotNull(
                messageList.Where(
                    item =>
                        item.ExceptionObject != null &&
                        item.ExceptionObject.GetType() == typeof(ListenerException)).FirstOrDefault()
                );

            Assert.Null(ThreadContext.Properties[AccountStatusChangedEventHandler.EXTERNAL_ID]);
        }
    }
}
