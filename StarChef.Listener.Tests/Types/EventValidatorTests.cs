using StarChef.Listener.Types;
using Xunit;
using AccountCreated = Fourth.Orchestration.Model.People.Events.AccountCreated;
using AccountCreateFailed = Fourth.Orchestration.Model.People.Events.AccountCreateFailed;
using AccountUpdated = Fourth.Orchestration.Model.People.Events.AccountUpdated;
using AccountUpdateFailed = Fourth.Orchestration.Model.People.Events.AccountUpdateFailed;
using AccountStatusChanged = Fourth.Orchestration.Model.People.Events.AccountStatusChanged;
using AccountStatusChangeFailed = Fourth.Orchestration.Model.People.Events.AccountStatusChangeFailed;

using AccountCreateFailedReason = Fourth.Orchestration.Model.People.Events.AccountCreateFailedReason;
using AccountUpdateFailedReason = Fourth.Orchestration.Model.People.Events.AccountUpdateFailedReason;
using AccountStatus = Fourth.Orchestration.Model.People.Events.AccountStatus;
using SourceSystem = Fourth.Orchestration.Model.People.Events.SourceSystem;
using StarChef.Listener.Commands;
using Moq;

namespace StarChef.Listener.Tests.Types
{
    public class EventValidatorTests
    {
        class TestEventValidator : EventValidator
        {
            public TestEventValidator(IDatabaseCommands databaseCommands) : base(databaseCommands)
            {
            }
        }

        #region StartChef events

        [Fact]
        public void Should_return_true_for_AccountCreated_event_with_StartChef_source()
        {
            var builder = AccountCreated.CreateBuilder();
            builder
                .SetExternalId("1")
                .SetSource(SourceSystem.STARCHEF);
            var accountCreated = builder.Build();

            var validator = new TestEventValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsStarChefEvent(accountCreated);

            Assert.True(actual);
        }

        [Fact]
        public void Should_return_true_for_AccountUpdated_event_with_StartChef_source()
        {
            var builder = AccountUpdated.CreateBuilder();
            builder
                .SetExternalId("1")
                .SetSource(SourceSystem.STARCHEF);
            var accountCreated = builder.Build();

            var validator = new TestEventValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsStarChefEvent(accountCreated);

            Assert.True(actual);
        }

        [Fact]
        public void Should_return_true_for_AccountStatusChanged_event_with_StartChef_source()
        {
            var builder = AccountStatusChanged.CreateBuilder();
            builder
                .SetExternalId("1")
                .SetStatus(AccountStatus.ACTIVE)
                .SetSource(SourceSystem.STARCHEF);
            var accountCreated = builder.Build();

            var validator = new TestEventValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsStarChefEvent(accountCreated);

            Assert.True(actual);
        }

        [Fact]
        public void Should_return_true_for_AccountCreateFailed_event_with_StartChef_source()
        {
            var builder = AccountCreateFailed.CreateBuilder();
            builder
                .SetInternalId("1")
                .SetReason(AccountCreateFailedReason.INVALID_CREATE_DATA)
                .SetSource(SourceSystem.STARCHEF);
            var accountCreated = builder.Build();

            var validator = new TestEventValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsStarChefEvent(accountCreated);

            Assert.True(actual);
        }

        [Fact]
        public void Should_return_true_for_AccountUpdateFailed_event_with_StartChef_source()
        {
            var builder = AccountUpdateFailed.CreateBuilder();
            builder
                .SetExternalId("1")
                .SetCommandId("1")
                .SetReason(AccountUpdateFailedReason.INVALID_UPDATE_DATA)
                .SetSource(SourceSystem.STARCHEF);
            var accountCreated = builder.Build();

            var validator = new TestEventValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsStarChefEvent(accountCreated);

            Assert.True(actual);
        }

        [Fact]
        public void Should_return_true_for_AccountStatusChangeFailed_event_with_StartChef_source()
        {
            var builder = AccountStatusChangeFailed.CreateBuilder();
            builder
                .SetExternalId("1")
                .SetSource(SourceSystem.STARCHEF);
            var accountCreated = builder.Build();

            var validator = new TestEventValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsStarChefEvent(accountCreated);

            Assert.True(actual);
        }

        #endregion

        #region Non StarChef events

        [Fact]
        public void Should_return_False_for_AccountCreated_event_with_nonStartChef_source()
        {
            var builder = AccountCreated.CreateBuilder();
            builder
                .SetExternalId("1")
                .SetSource(SourceSystem.ADACO);
            var accountCreated = builder.Build();

            var validator = new TestEventValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsStarChefEvent(accountCreated);

            Assert.False(actual);
        }

        [Fact]
        public void Should_return_False_for_AccountUpdated_event_with_nonStartChef_source()
        {
            var builder = AccountUpdated.CreateBuilder();
            builder
                .SetExternalId("1")
                .SetSource(SourceSystem.ADACO);
            var accountCreated = builder.Build();

            var validator = new TestEventValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsStarChefEvent(accountCreated);

            Assert.False(actual);
        }

        [Fact]
        public void Should_return_False_for_AccountStatusChanged_event_with_nonStartChef_source()
        {
            var builder = AccountStatusChanged.CreateBuilder();
            builder
                .SetExternalId("1")
                .SetStatus(AccountStatus.ACTIVE)
                .SetSource(SourceSystem.ADACO);
            var accountCreated = builder.Build();

            var validator = new TestEventValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsStarChefEvent(accountCreated);

            Assert.False(actual);
        }

        [Fact]
        public void Should_return_False_for_AccountCreateFailed_event_with_nonStartChef_source()
        {
            var builder = AccountCreateFailed.CreateBuilder();
            builder
                .SetInternalId("1")
                .SetReason(AccountCreateFailedReason.INVALID_CREATE_DATA)
                .SetSource(SourceSystem.ADACO);
            var accountCreated = builder.Build();

            var validator = new TestEventValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsStarChefEvent(accountCreated);

            Assert.False(actual);
        }

        [Fact]
        public void Should_return_False_for_AccountUpdateFailed_event_with_nonStartChef_source()
        {
            var builder = AccountUpdateFailed.CreateBuilder();
            builder
                .SetExternalId("1")
                .SetCommandId("1")
                .SetReason(AccountUpdateFailedReason.INVALID_UPDATE_DATA)
                .SetSource(SourceSystem.ADACO);
            var accountCreated = builder.Build();

            var validator = new TestEventValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsStarChefEvent(accountCreated);

            Assert.False(actual);
        }

        [Fact]
        public void Should_return_False_for_AccountStatusChangeFailed_event_with_nonStartChef_source()
        {
            var builder = AccountStatusChangeFailed.CreateBuilder();
            builder
                .SetExternalId("1")
                .SetSource(SourceSystem.ADACO);
            var accountCreated = builder.Build();

            var validator = new TestEventValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsStarChefEvent(accountCreated);

            Assert.False(actual);
        }

        #endregion

        [Fact]
        public void Should_return_false_if_source_is_not_set_for_event()
        {
            var builder = AccountCreated.CreateBuilder();
            builder
                .SetExternalId("1");
            var accountCreated = builder.Build();

            var validator = new TestEventValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsStarChefEvent(accountCreated);

            Assert.False(actual);
        }
    }
}