using AccountCreated = Fourth.Orchestration.Model.People.Events.AccountCreated;
using StarChef.Listener.Types;
using StarChef.Listener.Validators;
using Xunit;

namespace StarChef.Listener.Tests.Types
{
    public class AccountCreatedValidatorTests
    {
        [Fact]
        public void Should_return_true_for_valid_model()
        {
            var builder = AccountCreated.CreateBuilder();
            builder
                .SetInternalId("1")
                .SetFirstName("1")
                .SetLastName("1")
                .SetEmailAddress("1")
                .SetExternalId("1");
            var accountCreated = builder.Build();

            var validator = new AccountCreatedValidator();
            var actual = validator.IsValid(accountCreated);

            Assert.True(actual);
        }

        [Fact]
        public void Should_return_false_when_InternalId_missing()
        {
            var builder = AccountCreated.CreateBuilder();
            builder
                .SetFirstName("1")
                .SetLastName("1")
                .SetEmailAddress("1")
                .SetExternalId("1");
            var accountCreated = builder.Build();

            var validator = new AccountCreatedValidator();
            var actual = validator.IsValid(accountCreated);

            Assert.False(actual);
        }

        [Fact]
        public void Should_return_false_when_FirstName_missing()
        {
            var builder = AccountCreated.CreateBuilder();
            builder
                .SetInternalId("1")
                .SetLastName("1")
                .SetEmailAddress("1")
                .SetExternalId("1");
            var accountCreated = builder.Build();

            var validator = new AccountCreatedValidator();
            var actual = validator.IsValid(accountCreated);

            Assert.False(actual);
        }

        [Fact]
        public void Should_return_false_when_LastName_missing()
        {
            var builder = AccountCreated.CreateBuilder();
            builder
                .SetInternalId("1")
                .SetFirstName("1")
                .SetEmailAddress("1")
                .SetExternalId("1");
            var accountCreated = builder.Build();

            var validator = new AccountCreatedValidator();
            var actual = validator.IsValid(accountCreated);

            Assert.False(actual);
        }

        [Fact]
        public void Should_return_false_when_EmailAddress_missing()
        {
            var builder = AccountCreated.CreateBuilder();
            builder
                .SetInternalId("1")
                .SetFirstName("1")
                .SetLastName("1")
                .SetExternalId("1");
            var accountCreated = builder.Build();

            var validator = new AccountCreatedValidator();
            var actual = validator.IsValid(accountCreated);

            Assert.False(actual);
        }
    }
}