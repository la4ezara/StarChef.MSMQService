using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarChef.Listener.Extensions;
using StarChef.Orchestrate.Models.TransferObjects;
using Xunit;
using AccountCreated = Fourth.Orchestration.Model.People.Events.AccountCreated;
using AccountCreateFailed = Fourth.Orchestration.Model.People.Events.AccountCreateFailed;
using AccountCreateFailedReason = Fourth.Orchestration.Model.People.Events.AccountCreateFailedReason;
using AccountUpdated = Fourth.Orchestration.Model.People.Events.AccountUpdated;
using AccountUpdateFailed = Fourth.Orchestration.Model.People.Events.AccountUpdateFailed;
using AccountUpdateFailedReason = Fourth.Orchestration.Model.People.Events.AccountUpdateFailedReason;
using SourceSystem = Fourth.Orchestration.Model.People.Events.SourceSystem;


namespace StarChef.Listener.Tests.Extensions
{
    public class MessageEventsExtensionsTests
    {
        [Fact]
        public void Should_correctly_convert_AccountCreated_to_json()
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

            var actual = payload.ToJson();
            const string expected = @"{""ExternalId"":""00000000-0000-0000-0000-000000000000"",""InternalId"":""1"",""EmailAddress"":""1"",""FirstName"":""1"",""LastName"":""1"",""Source"":""STARCHEF""}";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_correctly_convert_AccountUpdated_to_json()
        {
            var builder = AccountUpdated.CreateBuilder();
            builder
                .SetUsername("1")
                .SetFirstName("1")
                .SetLastName("1")
                .SetEmailAddress("1")
                .SetSource(SourceSystem.STARCHEF)
                .SetExternalId(Guid.Empty.ToString());
            var payload = builder.Build();

            var actual = payload.ToJson();
            const string expected = @"{""ExternalId"":""00000000-0000-0000-0000-000000000000"",""Username"":""1"",""EmailAddress"":""1"",""FirstName"":""1"",""LastName"":""1"",""Source"":""STARCHEF""}";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_correctly_convert_AccountCreateFailed_to_json()
        {
            var builder = AccountCreateFailed.CreateBuilder();
            builder
                .SetInternalId("1")
                .SetReason(AccountCreateFailedReason.INVALID_CREATE_DATA)
                .SetSource(SourceSystem.STARCHEF);
            var payload = builder.Build();

            var actual = payload.ToJson();
            const string expected = @"{""InternalId"":""1"",""Reason"":""INVALID_CREATE_DATA"",""Source"":""STARCHEF""}";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_correctly_convert_AccountUpdateFailed_to_json()
        {
            var builder = AccountUpdateFailed.CreateBuilder();
            builder
                .SetCommandId("1")
                .SetExternalId("1")
                .SetReason(AccountUpdateFailedReason.INVALID_UPDATE_DATA)
                .SetSource(SourceSystem.STARCHEF);
            var payload = builder.Build();

            var actual = payload.ToJson();
            const string expected = @"{""CommandId"":""1"",""Reason"":""INVALID_UPDATE_DATA"",""ExternalId"":""1"",""Source"":""STARCHEF""}";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_correctly_convert_AccountCreatedTransferObject_to_json()
        {
            var obj = new AccountCreatedTransferObject
            {
                LoginId = 1,
                FirstName = "1",
                LastName = "1",
                EmailAddress = "1",
                ExternalLoginId = "1",
                Username = null
            };

            var actual = obj.ToJson();
            const string expected = @"{""LoginId"":1,""ExternalLoginId"":""1"",""Username"":null,""FirstName"":""1"",""LastName"":""1"",""EmailAddress"":""1""}";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_correctly_convert_AccountUpdatedTransferObject_to_json()
        {
            var obj = new AccountUpdatedTransferObject
            {
                ExternalLoginId = "00000000-0000-0000-0000-000000000000",
                Username = "user",
                FirstName = "1",
                LastName = "1",
                EmailAddress = "1"
            };

            var actual = obj.ToJson();
            const string expected = @"{""ExternalLoginId"":""00000000-0000-0000-0000-000000000000"",""Username"":""user"",""FirstName"":""1"",""LastName"":""1"",""EmailAddress"":""1""}";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_correctly_convert_AccountCreateFailedTransferObject_to_json()
        {
            var obj = new AccountCreateFailedTransferObject
            {
                LoginId = 1,
                ErrorCode = "1",
                Description = "1"
            };

            var actual = obj.ToJson();
            const string expected = @"{""LoginId"":1,""ErrorCode"":""1"",""Description"":""1""}";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_correctly_convert_AccountUpdateFailedTransferObject_to_json()
        {
            var obj = new AccountUpdateFailedTransferObject
            {
                ExternalLoginId = "00000000-0000-0000-0000-000000000000",
                ErrorCode = "1",
                Description = "1"
            };

            var actual = obj.ToJson();
            const string expected = @"{""ExternalLoginId"":""00000000-0000-0000-0000-000000000000"",""ErrorCode"":""1"",""Description"":""1""}";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_correctly_convert_IntegerValue_to_json()
        {
            var obj = 1;
            var actual = obj.ToJson();
            const string expected = @"1";
            Assert.Equal(expected, actual);
        }

        struct TestStruct
        {
            public int IntField { get; set; }
            public string StrField { get; set; }

            public TestStruct(int intField, string strField)
            {
                IntField = intField;
                StrField = strField;
            }
        }

        [Fact]
        public void Should_correctly_convert_Struct_to_json()
        {
            var obj = new TestStruct(1, "1");
            var actual = obj.ToJson();
            const string expected = @"{""IntField"":1,""StrField"":""1""}";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Should_correctly_convert_Null_to_json()
        {
            object obj = null;
            var actual = obj.ToJson();
            const string expected = @"null";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ToXmlString_should_use_5_digits_after_point_when_actualValueHasMore()
        {
            var actual = 0.12345678.ToXmlString();
            Assert.Equal("0.12346", actual);
        }
    }
}
