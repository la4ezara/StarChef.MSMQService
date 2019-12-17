using System;
using System.Collections.Generic;
using Moq;
using StarChef.Listener.Exceptions;
using StarChef.Listener.Extensions;
using StarChef.Listener.Tests.Helpers;
using StarChef.Orchestrate.Models.TransferObjects;
using Xunit;
using AccountCreated = Fourth.Orchestration.Model.People.Events.AccountCreated;
using AccountCreateFailed = Fourth.Orchestration.Model.People.Events.AccountCreateFailed;
using AccountCreateFailedReason = Fourth.Orchestration.Model.People.Events.AccountCreateFailedReason;
using AccountUpdated = Fourth.Orchestration.Model.People.Events.AccountUpdated;
using AccountUpdateFailed = Fourth.Orchestration.Model.People.Events.AccountUpdateFailed;
using AccountStatusChanged = Fourth.Orchestration.Model.People.Events.AccountStatusChanged;
using AccountStatusChangeFailed = Fourth.Orchestration.Model.People.Events.AccountStatusChangeFailed;
using AccountUpdateFailedReason = Fourth.Orchestration.Model.People.Events.AccountUpdateFailedReason;
using IMessage = Google.ProtocolBuffers.IMessage;
using SourceSystem = Fourth.Orchestration.Model.Common.SourceSystemId;


namespace StarChef.Listener.Tests.Extensions
{
    public class MessageEventsExtensionsTests
    {
        #region ToJson

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

            var actual = MessageEventsExtensions.ToJson(payload);
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

            var actual = MessageEventsExtensions.ToJson(payload);
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

            var actual = MessageEventsExtensions.ToJson(payload);
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

            var actual = MessageEventsExtensions.ToJson(payload);
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
                Username = "1",
                ExternalCustomerId = "1",
                PermissionSets = new List<string>()
            };

            var actual = obj.ToJson();
            const string expected = @"{""LoginId"":1,""ExternalLoginId"":""1"",""Username"":""1"",""FirstName"":""1"",""LastName"":""1"",""EmailAddress"":""1"",""ExternalCustomerId"":""1"",""PermissionSets"":[]}";
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
                EmailAddress = "1",
                PermissionSets = new List<string>()
            };

            var actual = obj.ToJson();
            const string expected = @"{""ExternalLoginId"":""00000000-0000-0000-0000-000000000000"",""Username"":""user"",""FirstName"":""1"",""LastName"":""1"",""EmailAddress"":""1"",""PermissionSets"":[]}";
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

        #endregion

        #region ToXmlString

        [Fact]
        public void ToXmlString_should_use_8_digits_after_decimal_point()
        {
            var actual = 0.123456789.ToXmlString();
            Assert.Equal("0.12345679", actual);
        }

        [Fact]
        public void ToXmlString_should_use_dynamic_format()
        {
            var actual = 0.899999999.ToXmlString();
            Assert.Equal("0.90", actual);
        }

        #endregion

        #region IsStarChefEvent

        [Theory]
        [MemberData(nameof(StarChefEvents))]
        public void IsStarChefEvent_should_return_true_for_StarChef(IMessage message)
        {
            var actual = message.IsStarChefEvent();

            Assert.True(actual);
        }

        [Theory]
        [MemberData(nameof(NonStarChefEvents))]
        public void IsStarChefEvent_should_return_false_for_nonStarChef(IMessage message)
        {
            var actual = message.IsStarChefEvent();

            Assert.False(actual);
        }

        [Fact]
        public void IsStarChefEvent_should_return_false_for_null()
        {
            IMessage nullMessage = null;
            var actual = nullMessage.IsStarChefEvent();

            Assert.False(actual);
        }

        [Fact]
        public void IsStarChefEvent_should_fail_for_payload_without_source()
        {
            var messageWithoutSource = Mock.Of<IMessage>();

            Assert.Throws<ListenerException>(() => messageWithoutSource.IsStarChefEvent());
        }

        #endregion

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

        private static object[] ConstructEventWithSource<T>(SourceSystem sourceSystem) where T : IMessage
        {
            var payload = PayloadHelpers.Construct<T>();
            PayloadHelpers.SetField(payload, "source_", sourceSystem); // the name of field is found with decompiler
            return new object[] { payload };
        }

        public static IEnumerable<object[]> StarChefEvents()
        {
            const SourceSystem SOURCE_SYSTEM = SourceSystem.STARCHEF;
            yield return ConstructEventWithSource<AccountCreated>(SOURCE_SYSTEM);
            yield return ConstructEventWithSource<AccountCreateFailed>(SOURCE_SYSTEM);
            yield return ConstructEventWithSource<AccountUpdated>(SOURCE_SYSTEM);
            yield return ConstructEventWithSource<AccountUpdateFailed>(SOURCE_SYSTEM);
            yield return ConstructEventWithSource<AccountStatusChanged>(SOURCE_SYSTEM);
            yield return ConstructEventWithSource<AccountStatusChangeFailed>(SOURCE_SYSTEM);
        }

        public static IEnumerable<object[]> NonStarChefEvents()
        {
            const SourceSystem SOURCE_SYSTEM = SourceSystem.R9; // some source not equal to StarChef
            yield return ConstructEventWithSource<AccountCreated>(SOURCE_SYSTEM);
            yield return ConstructEventWithSource<AccountCreateFailed>(SOURCE_SYSTEM);
            yield return ConstructEventWithSource<AccountUpdated>(SOURCE_SYSTEM);
            yield return ConstructEventWithSource<AccountUpdateFailed>(SOURCE_SYSTEM);
            yield return ConstructEventWithSource<AccountStatusChanged>(SOURCE_SYSTEM);
            yield return ConstructEventWithSource<AccountStatusChangeFailed>(SOURCE_SYSTEM);
        }
    }
}
