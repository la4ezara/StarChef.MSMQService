﻿using StarChef.Listener.Handlers;
using System;
using Fourth.Orchestration.Messaging;
using Moq;
using StarChef.Listener.Commands;
using Xunit;
using AccountUpdated = Fourth.Orchestration.Model.People.Events.AccountUpdated;
using SourceSystem = Fourth.Orchestration.Model.Common.SourceSystemId;
using StarChef.Orchestrate.Models.TransferObjects;
using StarChef.Listener.Tests.Fixtures;
using StarChef.Listener.Tests.Helpers;
using StarChef.Listener.Types;
using StarChef.Listener.Validators;
using log4net;
using log4net.Core;
using System.Linq;
using StarChef.Listener.Exceptions;

namespace StarChef.Listener.Tests.Handlers
{
    public class AccountUpdatedEventHandlerTests : IClassFixture<ObjectMappingFixture>
    {
        //[Fact]
        //public void Should_do_nothing_for_nonStarchef_event()
        //{
        //    var builder = AccountUpdated.CreateBuilder();
        //    builder
        //        .SetUsername("1")
        //        .SetFirstName("1")
        //        .SetLastName("1")
        //        .SetEmailAddress("1")
        //        .SetSource(SourceSystem.ADACO)
        //        .SetExternalId(Guid.Empty.ToString());
        //    var payload = builder.Build();

        //    var dbCommands = new Mock<IDatabaseCommands>(MockBehavior.Strict);
        //    var validator = new AccountUpdatedValidator(dbCommands.Object);
        //    var messagingLogger = new Mock<IMessagingLogger>(MockBehavior.Strict);

        //    var handler = new AccountUpdatedEventHandler(dbCommands.Object, validator, messagingLogger.Object);

        //    var result = handler.HandleAsync(payload, "1").Result;

        //    // assertions
        //    Assert.Equal(MessageHandlerResult.Success, result);
        //}

        [Fact]
        public void Should_register_error_with_model()
        {
            // arrange
            var payload = PayloadHelpers.Construct<AccountUpdated>();
            var dbCommands = new Mock<IDatabaseCommands>();
            var validator = new Mock<IEventValidator>();
            validator.Setup(m => m.IsAllowedEvent(It.IsAny<object>())).Returns(true);
            validator.Setup(m => m.IsValidPayload(It.IsAny<object>())).Returns(false);
            var messagingLogger = new Mock<IMessagingLogger>();
            var handler = new AccountUpdatedEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object);

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
            var builder = AccountUpdated.CreateBuilder();
            builder
                .SetUsername("1")
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
            var logChecker = new LogChecker(typeof(AccountUpdatedEventHandler), Level.All);
            var handler = new AccountUpdatedEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object, logChecker.GetLogger());

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.Empty(messageList);

            Assert.Null(ThreadContext.Properties[AccountUpdatedEventHandler.EXTERNAL_ID]);
        }

        [Fact]
        public void All_logs_should_have_correct_external_id()
        {
            // arrange
            var builder = AccountUpdated.CreateBuilder();
            builder
                .SetUsername("1")
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
            var logChecker = new LogChecker(typeof(AccountUpdatedEventHandler), Level.All);
            var handler = new AccountUpdatedEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object, logChecker.GetLogger());

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.All(messageList, item =>
            {
                Assert.Equal(item.Properties[AccountUpdatedEventHandler.EXTERNAL_ID], Guid.Empty.ToString());
            });

            Assert.Null(ThreadContext.Properties[AccountUpdatedEventHandler.EXTERNAL_ID]);
        }

        [Fact]
        public void All_logs_should_have_correct_external_id_invalid_payload()
        {
            // arrange
            var builder = AccountUpdated.CreateBuilder();
            builder
                .SetUsername("1")
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
            var logChecker = new LogChecker(typeof(AccountUpdatedEventHandler), Level.All);
            var handler = new AccountUpdatedEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object, logChecker.GetLogger());

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.All(messageList, item =>
            {
                Assert.Equal(item.Properties[AccountUpdatedEventHandler.EXTERNAL_ID], Guid.Empty.ToString());
            });

            Assert.Null(ThreadContext.Properties[AccountUpdatedEventHandler.EXTERNAL_ID]);
        }

        [Fact]
        public void Should_log_listener_exceptions_and_have_correct_external_id()
        {
            // arrange
            var builder = AccountUpdated.CreateBuilder();
            builder
                .SetUsername("1")
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
            messagingLogger.Setup(d => d.MessageProcessedSuccessfully(It.IsAny<object>(), It.IsAny<string>())).Throws(new ListenerException());
            var logChecker = new LogChecker(typeof(AccountUpdatedEventHandler), Level.All);
            var handler = new AccountUpdatedEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object,logChecker.GetLogger());

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.All(messageList, item =>
            {
                Assert.Equal(item.Properties[AccountUpdatedEventHandler.EXTERNAL_ID], Guid.Empty.ToString());
            });

            Assert.NotNull(
                messageList.Where(
                    item =>
                        item.ExceptionObject != null &&
                        item.ExceptionObject.GetType() == typeof(ListenerException)).FirstOrDefault()
                );

            Assert.Null(ThreadContext.Properties[AccountUpdatedEventHandler.EXTERNAL_ID]);
        }
    }
}
