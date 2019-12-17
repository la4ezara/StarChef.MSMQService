using StarChef.Listener.Handlers;
using System;
using Fourth.Orchestration.Messaging;
using Moq;
using StarChef.Listener.Commands;
using Xunit;
using AccountCreated = Fourth.Orchestration.Model.People.Events.AccountCreated;
using SourceSystem = Fourth.Orchestration.Model.Common.SourceSystemId;
using StarChef.Orchestrate.Models.TransferObjects;
using StarChef.Listener.Tests.Fixtures;
using StarChef.Listener.Validators;
using StarChef.Listener.Tests.Helpers;
using log4net.Core;
using StarChef.Listener.Exceptions;
using System.Linq;
using log4net;
using System.Threading.Tasks;

namespace StarChef.Listener.Tests.Handlers
{
    public class AccountCreatedEventHandlerTests : IClassFixture<ObjectMappingFixture>
    {
        [Fact]
        public void Should_updated_valid_data_and_log_to_messaging_events()
        {
            const int LOGIN_ID = 123;
            const string USERNAME = "username";
            const string FIRST_NAME = "first_name";
            const string LAST_NAME = "last_name";
            const string EMAIL_ADDRESS = "email";
            var externalId = Guid.NewGuid().ToString();
            var builder = AccountCreated.CreateBuilder();
            builder
                .SetInternalId(LOGIN_ID.ToString())
                .SetUsername(USERNAME)
                .SetFirstName(FIRST_NAME)
                .SetLastName(LAST_NAME)
                .SetEmailAddress(EMAIL_ADDRESS)
                .SetSource(SourceSystem.STARCHEF)
                .SetExternalId(externalId);
            var payload = builder.Build();

            var dbCommands = new Mock<IDatabaseCommands>();
            dbCommands.Setup(m => m.IsUserExists(It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            dbCommands.Setup(m => m.OriginateLoginId(It.IsAny<int>(), It.IsAny<string>())).Returns(Task.FromResult(LOGIN_ID));

            var validator = new AccountCreatedValidator(dbCommands.Object);
            var messagingLogger = new Mock<IMessagingLogger>();
            var config = new Mock<IConfiguration>();
            var handler = new AccountCreatedEventHandler(dbCommands.Object, validator, config.Object, messagingLogger.Object);

            var result = handler.HandleAsync(payload, "1").Result;

            // assertions
            Assert.Equal(MessageHandlerResult.Success, result);
            dbCommands.Verify(m => m.UpdateExternalId(It.Is<AccountCreatedTransferObject>(p =>
                p.LoginId == LOGIN_ID
                && p.ExternalLoginId == externalId
                )), Times.Once);
            messagingLogger.Verify(m => m.MessageProcessedSuccessfully(It.Is<object>(p => ReferenceEquals(p, payload)), It.IsAny<string>()), Times.Once);
        }

        //[Fact]
        //public void Should_do_nothing_for_nonStarchef_event()
        //{
        //    var builder = AccountCreated.CreateBuilder();
        //    builder
        //        .SetInternalId("1")
        //        .SetFirstName("1")
        //        .SetLastName("1")
        //        .SetEmailAddress("1")
        //        .SetSource(SourceSystem.ADACO)
        //        .SetExternalId(Guid.Empty.ToString());
        //    var payload = builder.Build();

        //    var dbCommands = new Mock<IDatabaseCommands>(MockBehavior.Strict); // ensure there is no setup, this  object should not been called
        //    var validator = new AccountCreatedValidator(dbCommands.Object);
        //    var messagingLogger = new Mock<IMessagingLogger>(MockBehavior.Strict);
        //    var config = new Mock<IConfiguration>();
        //    var handler = new AccountCreatedEventHandler(dbCommands.Object, validator, config.Object, messagingLogger.Object);

        //    var result = handler.HandleAsync(payload, "1").Result;

        //    // assertions
        //    Assert.Equal(MessageHandlerResult.Success, result);
        //}

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
            var config = new Mock<IConfiguration>();
            var handler = new AccountCreatedEventHandler(dbCommands.Object, validator.Object, config.Object, messagingLogger.Object);

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
            var config = new Mock<IConfiguration>();
            var logChecker = new LogChecker(typeof(AccountCreatedEventHandler), Level.All);
            var handler = new AccountCreatedEventHandler(dbCommands.Object, validator.Object, config.Object, messagingLogger.Object, logChecker.GetLogger());

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.Empty(messageList);

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
            var config = new Mock<IConfiguration>();
            var logChecker = new LogChecker(typeof(AccountCreatedEventHandler), Level.All);
            var handler = new AccountCreatedEventHandler(dbCommands.Object, validator.Object, config.Object, messagingLogger.Object, logChecker.GetLogger());

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.All(messageList, item =>
            {
                Assert.Equal("1", item.Properties[AccountCreatedEventHandler.INTERNAL_ID]);
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
            var config = new Mock<IConfiguration>();
            var logChecker = new LogChecker(typeof(AccountCreatedEventHandler), Level.All);
            var handler = new AccountCreatedEventHandler(dbCommands.Object, validator.Object, config.Object, messagingLogger.Object, logChecker.GetLogger());

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.All(messageList, item =>
            {
                Assert.Equal("1", item.Properties[AccountCreatedEventHandler.INTERNAL_ID]);
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
            var config = new Mock<IConfiguration>();
            var logChecker = new LogChecker(typeof(AccountCreatedEventHandler), Level.All);
            var handler = new AccountCreatedEventHandler(dbCommands.Object, validator.Object, config.Object, messagingLogger.Object, logChecker.GetLogger());

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.All(messageList, item =>
            {
                Assert.Equal("1", item.Properties[AccountCreatedEventHandler.INTERNAL_ID]);
            });

            Assert.NotNull(
                messageList.Where(
                    item => 
                        item.ExceptionObject != null &&
                        item.ExceptionObject.GetType() == typeof(ListenerException)).FirstOrDefault()
                );

            Assert.Null(ThreadContext.Properties[AccountCreatedEventHandler.INTERNAL_ID]);
        }

        [Fact]
        public void Should_create_a_new_user()
        {
            const int LOGIN_ID = 123;
            const string USERNAME = "username";
            const string FIRST_NAME = "first_name";
            const string LAST_NAME = "last_name";
            const string EMAIL_ADDRESS = "email";
            var externalId = Guid.NewGuid().ToString();
            var builder = AccountCreated.CreateBuilder();
            builder
                .SetInternalId(LOGIN_ID.ToString())
                .SetUsername(USERNAME)
                .SetFirstName(FIRST_NAME)
                .SetLastName(LAST_NAME)
                .SetEmailAddress(EMAIL_ADDRESS)
                .SetSource(SourceSystem.STARCHEF)
                .SetExternalId(externalId);
            var payload = builder.Build();

            var validator = new Mock<IEventValidator>();
            validator.Setup(m => m.IsAllowedEvent(It.IsAny<object>())).Returns(true);
            validator.Setup(m => m.IsValidPayload(It.IsAny<object>())).Returns(true);

            var messagingLogger = new Mock<IMessagingLogger>();
            
            var dbCommands = new Mock<IDatabaseCommands>();
            dbCommands.Setup(m => m.IsUserExists(It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(false)); // user is not exists

            var config = new Mock<IConfiguration>();
            var handler = new AccountCreatedEventHandler(dbCommands.Object, validator.Object, config.Object, messagingLogger.Object);

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            // assertions
            Assert.Equal(MessageHandlerResult.Success, result);
            // while check loginId AND loginName are used
            dbCommands.Verify(m => m.IsUserExists(It.Is<int?>(p => p.Value == LOGIN_ID), It.Is<string>(p => p == null), It.Is<string>(p => p == USERNAME)), Times.Once);
            dbCommands.Verify(m => m.AddUser(It.Is<AccountCreatedTransferObject>(p =>
                p.EmailAddress == EMAIL_ADDRESS
                && p.Username == USERNAME
                && p.FirstName == FIRST_NAME
                && p.LastName == LAST_NAME
                && p.ExternalLoginId == externalId
                )), Times.Once);
        }
    }
}
