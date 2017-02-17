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
using StarChef.Listener.Types;
using StarChef.Listener.Validators;

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
            var builder = AccountCreated.CreateBuilder();
            builder
                .SetInternalId("1")
                .SetFirstName("1")
                .SetEmailAddress("1")
                .SetSource(SourceSystem.STARCHEF)
                .SetExternalId(Guid.Empty.ToString());
            var payload = builder.Build();

            var dbCommands = new Mock<IDatabaseCommands>();
            var validator = new AccountCreatedValidator(dbCommands.Object);
            var messagingLogger = new TestMessagingLogger();
            var handler = new AccountCreatedEventHandler(dbCommands.Object, validator, messagingLogger);

            var result = handler.HandleAsync(payload, "1").Result;

            // assertions
            Assert.Equal(MessageHandlerResult.Fatal, result);
            Assert.True(messagingLogger.IsInvalidModelRegistered);
        }
    }
}
