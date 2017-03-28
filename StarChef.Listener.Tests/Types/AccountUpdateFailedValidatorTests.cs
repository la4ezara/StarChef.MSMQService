using Moq;
using StarChef.Listener.Commands;
using AccountUpdateFailedReason = Fourth.Orchestration.Model.People.Events.AccountUpdateFailedReason;
using AccountUpdateFailed = Fourth.Orchestration.Model.People.Events.AccountUpdateFailed;
using StarChef.Listener.Types;
using StarChef.Listener.Validators;
using Xunit;

namespace StarChef.Listener.Tests.Types
{
    public class AccountUpdateFailedValidatorTests
    {
        [Fact]
        public void Should_return_true_for_valid_model()
        {
            var builder = AccountUpdateFailed.CreateBuilder();
            builder
                .SetExternalId("1")
                .SetCommandId("1")
                .SetReason(AccountUpdateFailedReason.INVALID_UPDATE_DATA);
            var accountCreated = builder.Build();

            var validator = new AccountUpdateFailedValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsValidPayload(accountCreated);

            Assert.True(actual);
        }
    }
}