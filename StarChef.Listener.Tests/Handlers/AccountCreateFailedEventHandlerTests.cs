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
using StarChef.Listener.Tests.Handlers.Fakes;
using StarChef.Listener.Types;
using StarChef.Listener.Validators;

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

            var validator = new AccountCreateFailedValidator();
            var dbCommands = new TestDatabaseCommands();
            var messagingLogger = new TestMessagingLogger();
            var handler = new AccountCreateFailedEventHandler(dbCommands, validator, messagingLogger);

            var result = handler.HandleAsync(payload, "1").Result;

            // assertions
            Assert.Equal(MessageHandlerResult.Success, result);
            Assert.True(messagingLogger.IsFailedMessageRegistered);
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

            var validator = new AccountCreateFailedValidator();
            var dbCommands = new TestDatabaseCommands();
            var messagingLogger = new TestMessagingLogger();
            var handler = new AccountCreateFailedEventHandler(dbCommands, validator, messagingLogger);

            var result = handler.HandleAsync(payload, "1").Result;

            // assertions
            Assert.Equal(MessageHandlerResult.Success, result);
            Assert.False(dbCommands.IsCalledAnyMethod);
            Assert.False(messagingLogger.IsCalledAnyMethod);
        }
    }
}
