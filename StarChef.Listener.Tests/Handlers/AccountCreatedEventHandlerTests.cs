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
using StarChef.Listener.Types;

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
                .SetFirstName("1")
                .SetLastName("1")
                .SetEmailAddress("1")
                .SetSource(SourceSystem.STARCHEF)
                .SetExternalId(Guid.Empty.ToString());
            var payload = builder.Build();

            var validator = new AccountCreatedValidator();

            var dbCommands = new Mock<IDatabaseCommands>();
            var messagingLogger = new Mock<IMessagingLogger>();

            var handler = new AccountCreatedEventHandler(dbCommands.Object, validator, messagingLogger.Object);

            var result = handler.HandleAsync(payload, "1").Result;

            // assertions
            Assert.Equal(MessageHandlerResult.Success, result);
            dbCommands.Verify(db=>db.UpdateExternalId(It.IsAny<UserTransferObject>()), Times.Once);
            messagingLogger.Verify(db=>db.RegisterSuccess(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
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

            var validator = new AccountCreatedValidator();

            var dbCommands = new Mock<IDatabaseCommands>();
            var messagingLogger = new Mock<IMessagingLogger>();

            var handler = new AccountCreatedEventHandler(dbCommands.Object, validator, messagingLogger.Object);

            var result = handler.HandleAsync(payload, "1").Result;

            // assertions
            Assert.Equal(MessageHandlerResult.Success, result);
            dbCommands.Verify(db => db.UpdateExternalId(It.IsAny<UserTransferObject>()), Times.Never);
            messagingLogger.Verify(db => db.RegisterSuccess(It.IsAny<object>(), It.IsAny<string>()), Times.Never);
            messagingLogger.Verify(db => db.RegisterInvalidModel(It.IsAny<string>(), It.IsAny<AccountCreated>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Should_register_error_with_model()
        {
            var builder = AccountCreated.CreateBuilder();
            builder
                .SetInternalId("1")
                .SetFirstName("1")
                .SetEmailAddress("1")
                .SetSource(SourceSystem.STARCHEF)
                .SetExternalId(Guid.Empty.ToString());
            var payload = builder.Build();

            var validator = new AccountCreatedValidator();

            var dbCommands = new Mock<IDatabaseCommands>();
            var messagingLogger = new Mock<IMessagingLogger>();

            var handler = new AccountCreatedEventHandler(dbCommands.Object, validator, messagingLogger.Object);

            var result = handler.HandleAsync(payload, "1").Result;

            // assertions
            Assert.Equal(MessageHandlerResult.Fatal, result);
            messagingLogger.Verify(db => db.RegisterInvalidModel(It.IsAny<string>(), It.IsAny<AccountCreated>(), It.IsAny<string>()), Times.Once);
        }
    }
}
