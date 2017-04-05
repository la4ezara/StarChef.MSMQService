using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarChef.Listener.Validators;
using Moq;
using StarChef.Listener.Commands;
using StarChef.Listener.Tests.Helpers;
using Xunit;
using AccountCreated = Fourth.Orchestration.Model.People.Events.AccountCreated;

namespace StarChef.Listener.Tests.Validators
{
    public class AccountCreatedValidatorTests
    {
        [Fact]
        public void Should_catch_nonInt_IntenalId()
        {
            var builder = AccountCreated.CreateBuilder();
            const long MORE_THAN_INT = (((long)int.MaxValue)+1);
            builder.SetExternalId("mandatory")
                .SetInternalId(MORE_THAN_INT.ToString());
            var payload = builder.Build();

            var validator = new AccountCreatedValidator(Mock.Of<IDatabaseCommands>());
            var result = validator.IsValidPayload(payload);

            Assert.False(result);
            Assert.Equal("InternalId is not Int32: "+ MORE_THAN_INT, validator.GetErrors());
        }
    }
}
