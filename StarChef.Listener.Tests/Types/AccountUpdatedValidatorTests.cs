using AccountUpdated = Fourth.Orchestration.Model.People.Events.AccountUpdated;
using StarChef.Listener.Types;
using StarChef.Listener.Validators;
using Xunit;
using Moq;
using StarChef.Listener.Commands;

namespace StarChef.Listener.Tests.Types
{
    public class AccountUpdatedValidatorTests
    {
        [Fact]
        public void Should_return_true_for_valid_model()
        {
            var builder = AccountUpdated.CreateBuilder();
            builder
                .SetUsername("1")
                .SetFirstName("1")
                .SetLastName("1")
                .SetEmailAddress("1")
                .SetExternalId("1");
            var accountCreated = builder.Build();

            var validator = new AccountUpdatedValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsValidPayload(accountCreated);

            Assert.True(actual);
        }

        [Fact]
        public void Should_return_false_when_Username_missing()
        {
            var builder = AccountUpdated.CreateBuilder();
            builder
                .SetFirstName("1")
                .SetLastName("1")
                .SetEmailAddress("1")
                .SetExternalId("1"); ;
            var accountCreated = builder.Build();

            var validator = new AccountUpdatedValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsValidPayload(accountCreated);

            Assert.False(actual);
        }

        [Fact]
        public void Should_return_false_when_FirstName_missing()
        {
            var builder = AccountUpdated.CreateBuilder();
            builder
                .SetUsername("1")
                .SetLastName("1")
                .SetEmailAddress("1")
                .SetExternalId("1");
            var accountCreated = builder.Build();

            var validator = new AccountUpdatedValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsValidPayload(accountCreated);

            Assert.False(actual);
        }

        [Fact]
        public void Should_return_false_when_LastName_missing()
        {
            var builder = AccountUpdated.CreateBuilder();
            builder
                .SetUsername("1")
                .SetFirstName("1")
                .SetEmailAddress("1")
                .SetExternalId("1");
            var accountCreated = builder.Build();

            var validator = new AccountUpdatedValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsValidPayload(accountCreated);

            Assert.False(actual);
        }

        [Fact]
        public void Should_return_false_when_EmailAddress_missing()
        {
            var builder = AccountUpdated.CreateBuilder();
            builder
                .SetUsername("1")
                .SetFirstName("1")
                .SetLastName("1")
                .SetExternalId("1");
            var accountCreated = builder.Build();

            var validator = new AccountUpdatedValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsValidPayload(accountCreated);

            Assert.False(actual);
        }
    }
}