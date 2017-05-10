using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using StarChef.Listener.Commands;
using StarChef.Listener.Validators;
using Xunit;

using AccountCreateFailed = Fourth.Orchestration.Model.People.Events.AccountCreateFailed;
using AccountCreateFailedReason = Fourth.Orchestration.Model.People.Events.AccountCreateFailedReason;

namespace StarChef.Listener.Tests.Validators
{
    public class AccountCreateFailedValidatorTests
    {
        [Fact]
        public void Should_catch_nonInt_IntenalId()
        {
            var builder = AccountCreateFailed.CreateBuilder();
            const long MORE_THAN_INT = (((long)int.MaxValue) + 1);
            builder.SetReason(AccountCreateFailedReason.INVALID_CREATE_DATA)
                .SetInternalId(MORE_THAN_INT.ToString());
            var payload = builder.Build();

            var validator = new AccountCreateFailedValidator(Mock.Of<IDatabaseCommands>());
            var result = validator.IsValidPayload(payload);

            Assert.False(result);
            Assert.Equal("InternalId is not Int32: " + MORE_THAN_INT, validator.GetErrors());
        }

        [Fact]
        public void Should_return_true_for_valid_model()
        {
            var builder = AccountCreateFailed.CreateBuilder();
            builder
                .SetInternalId("1")
                .SetReason(AccountCreateFailedReason.INVALID_CREATE_DATA);
            var accountCreated = builder.Build();

            var validator = new AccountCreateFailedValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsValidPayload(accountCreated);

            Assert.True(actual);
        }
    }
}
