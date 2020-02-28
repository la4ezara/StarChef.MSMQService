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

			builder.AddPermissionSets("Star_Chef");
			var payload = builder.Build();

            var validator = new AccountCreatedValidator(Mock.Of<IDatabaseCommands>());
            var result = validator.IsValidPayload(payload);

            Assert.False(result);
            Assert.Equal("InternalId is not Int32: "+ MORE_THAN_INT, validator.GetErrors());
        }
        [Fact]
        public void Should_return_true_for_valid_model()
        {
            var builder = AccountCreated.CreateBuilder();
            builder
                .SetInternalId("1")
                .SetUsername("1")
                .SetFirstName("1")
                .SetLastName("1")
                .SetEmailAddress("1")
                .SetExternalId("1");
			builder.AddPermissionSets("Star_Chef");

			var accountCreated = builder.Build();

            var validator = new AccountCreatedValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsValidPayload(accountCreated);

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
			builder.AddPermissionSets("Star_Chef");
			var accountCreated = builder.Build();

            var validator = new AccountCreatedValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsValidPayload(accountCreated);

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
			builder.AddPermissionSets("Menu_Cycles");
			var accountCreated = builder.Build();

            var validator = new AccountCreatedValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsValidPayload(accountCreated);

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
			builder.AddPermissionSets("Star_Chef");
			var accountCreated = builder.Build();

            var validator = new AccountCreatedValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsValidPayload(accountCreated);

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
			builder.AddPermissionSets("Star_Chef");
			var accountCreated = builder.Build();

            var validator = new AccountCreatedValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsValidPayload(accountCreated);

            Assert.False(actual);
        }

        [Fact]
        public void Should_return_false_when_Username_too_long()
        {
            var builder = AccountCreated.CreateBuilder();
            const int MAX_LENGTH = 50;
            var value = Enumerable.Repeat("a", MAX_LENGTH + 1).Aggregate((a, b) => a + b);
            builder
                .SetUsername(value)
                .SetInternalId("1")
                .SetFirstName("any")
                .SetLastName("any")
                .SetEmailAddress("any")
                .SetExternalId("any");
			builder.AddPermissionSets("Star_Chef");
			var accountCreated = builder.Build();

            var validator = new AccountCreatedValidator(Mock.Of<IDatabaseCommands>());
            var actual = validator.IsValidPayload(accountCreated);

            Assert.False(actual);
            Assert.Equal("Username exceeds the maximum length of 50 characters.", validator.GetErrors());
        }

		[Fact]
		public void Should_return_false_when_PermissionSets_donot_contain_StarchefOrMenuCycle()
		{
			var builder = AccountCreated.CreateBuilder();
			builder
				.SetUsername("test")
				.SetInternalId("1")
				.SetFirstName("any")
				.SetLastName("any")
				.SetEmailAddress("any")
				.SetExternalId("any");

			builder.AddPermissionSets("Star_Char");

			var accountCreated = builder.Build();

			var validator = new AccountCreatedValidator(Mock.Of<IDatabaseCommands>());
			var actual = validator.IsValidPayload(accountCreated);

			Assert.False(actual);
			Assert.Equal("PermissionSets don't contain Star_Chef or Menu_Cycles set.", validator.GetErrors());
		}
    }
}
