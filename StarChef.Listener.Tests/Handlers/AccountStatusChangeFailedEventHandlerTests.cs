﻿using log4net;
using log4net.Core;
using Moq;
using StarChef.Listener.Commands;
using StarChef.Listener.Handlers;
using StarChef.Listener.Tests.Fixtures;
using StarChef.Listener.Tests.Helpers;
using Xunit;
using static Fourth.Orchestration.Model.People.Events;
using SourceSystem = Fourth.Orchestration.Model.Common.SourceSystemId;

namespace StarChef.Listener.Tests.Handlers
{
    public class AccountStatusChangeFailedEventHandlerTests : IClassFixture<ObjectMappingFixture>
    {
        [Fact]
        public void Should_not_have_log_for_non_starchef_events()
        {
            // arrange
            var builder = AccountStatusChangeFailed.CreateBuilder();
            builder
                .SetCommandId("1")
                .SetExternalId("1")
                .SetSource(SourceSystem.ADACO);
            var payload = builder.Build();

            var dbCommands = new Mock<IDatabaseCommands>();
            var validator = new Mock<IEventValidator>(MockBehavior.Strict);
            validator.Setup(m => m.IsAllowedEvent(payload)).Returns(false);


            var messagingLogger = new Mock<IMessagingLogger>();
            var logChecker = new LogChecker(typeof(AccountStatusChangeFailedEventHandler), Level.All);
            var handler = new AccountStatusChangeFailedEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object, logChecker.GetLogger());

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.Empty(messageList);

            Assert.Null(ThreadContext.Properties[AccountStatusChangeFailedEventHandler.EXTERNAL_ID]);
        }

        [Fact]
        public void All_logs_should_have_correct_external_id()
        {
            // arrange
            var builder = AccountStatusChangeFailed.CreateBuilder();
            builder
                .SetCommandId("1")
                .SetExternalId("1")
                .SetSource(SourceSystem.STARCHEF);
            var payload = builder.Build();

            var dbCommands = new Mock<IDatabaseCommands>();
            var validator = new Mock<IEventValidator>(MockBehavior.Strict);
            validator.Setup(m => m.IsAllowedEvent(payload)).Returns(true);
            validator.Setup(m => m.IsValidPayload(It.IsAny<object>())).Returns(true);
            var messagingLogger = new Mock<IMessagingLogger>();
            var logChecker = new LogChecker(typeof(AccountStatusChangeFailedEventHandler), Level.All);
            var handler = new AccountStatusChangeFailedEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object, logChecker.GetLogger());

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.All(messageList, item =>
            {
                Assert.Equal("1", item.Properties[AccountStatusChangeFailedEventHandler.EXTERNAL_ID]);
            });

            Assert.Null(ThreadContext.Properties[AccountStatusChangeFailedEventHandler.EXTERNAL_ID]);
        }

        [Fact]
        public void All_logs_should_have_correct_external_id_invalid_payload()
        {
            // arrange
            var builder = AccountStatusChangeFailed.CreateBuilder();
            builder
                .SetCommandId("1")
                .SetExternalId("1")
                .SetSource(SourceSystem.STARCHEF);
            var payload = builder.Build();

            var dbCommands = new Mock<IDatabaseCommands>();
            var validator = new Mock<IEventValidator>(MockBehavior.Strict);
            validator.Setup(m => m.IsAllowedEvent(payload)).Returns(true);

            validator.Setup(m => m.IsValidPayload(It.IsAny<object>())).Returns(false);
            validator.Setup(m => m.GetErrors()).Returns(string.Empty);
            var messagingLogger = new Mock<IMessagingLogger>();
            var logChecker = new LogChecker(typeof(AccountStatusChangeFailedEventHandler), Level.All);
            var handler = new AccountStatusChangeFailedEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object, logChecker.GetLogger());

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.All(messageList, item =>
            {
                Assert.Equal("1", item.Properties[AccountStatusChangeFailedEventHandler.EXTERNAL_ID]);
            });

            Assert.Null(ThreadContext.Properties[AccountStatusChangeFailedEventHandler.EXTERNAL_ID]);
        }
    }
}
