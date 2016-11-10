using AccountCreateFailedReason = Fourth.Orchestration.Model.People.Events.AccountCreateFailedReason;
using AccountCreateFailed = Fourth.Orchestration.Model.People.Events.AccountCreateFailed;
using StarChef.Listener.Types;
using Xunit;

namespace StarChef.Listener.Tests.Types
{
    public class AccountCreateFailedValidatorTests
    {
        [Fact]
        public void Should_return_true_for_valid_model()
        {
            var builder = AccountCreateFailed.CreateBuilder();
            builder
                .SetInternalId("1")
                .SetReason(AccountCreateFailedReason.INVALID_CREATE_DATA);
            var accountCreated = builder.Build();

            var validator = new AccountCreateFailedValidator();
            var actual = validator.IsValid(accountCreated);

            Assert.True(actual);
        }
    }
}