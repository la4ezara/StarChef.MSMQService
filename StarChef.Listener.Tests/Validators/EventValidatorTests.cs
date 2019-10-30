using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
using StarChef.Listener.Validators;

namespace StarChef.Listener.Tests.Types
{
    public class EventValidatorTests
    {
        [Theory(DisplayName = "Account validator should return True for StarChef source")]
        [MemberData(nameof(AccountEventsWithStarChefSourceSystem))]
        public void Should_return_True_for_event_with_StartChef_source(object payload, object validator)
        {
            var actual = ((EventValidator)validator).IsAllowedEvent(payload);

            Assert.True(actual);
        }

        //[Theory(DisplayName = "Account validator should return False for non StarChef source")]
        //[MemberData(nameof(AccountEventsWithNonStarChefSourceSystem))]
        //public void Should_return_False_for_event_with_nonStartChef_source(object payload, object validator)
        //{
        //    var actual = ((EventValidator)validator).IsAllowedEvent(payload);

        //    Assert.False(actual);
        //}

        //[Theory(DisplayName = "Account validator should return False if source is not set")]
        //[MemberData(nameof(AccountEventsWithoutSourceSystem))]
        //public void Should_return_False_if_source_is_not_set_for_event(object payload, object validator)
        //{
        //    var actual = ((EventValidator)validator).IsAllowedEvent(payload);

        //    Assert.False(actual);
        //}

        public static IEnumerable<object[]> AccountEventsWithStarChefSourceSystem()
        {
            var acb = AccountCreated.CreateBuilder();
            acb.SetExternalId("1").SetSource(SourceSystem.STARCHEF);
            yield return new object[] { acb.Build(), new AccountCreatedValidator(Mock.Of<IDatabaseCommands>()) };

            var aub = AccountUpdated.CreateBuilder();
            aub.SetExternalId("1").SetSource(SourceSystem.STARCHEF);
            yield return new object[] { aub.Build(), new AccountUpdatedValidator(Mock.Of<IDatabaseCommands>()) };

            var ascb = AccountStatusChanged.CreateBuilder();
            ascb.SetExternalId("1").SetStatus(AccountStatus.ACTIVE).SetSource(SourceSystem.STARCHEF);
            yield return new object[] { ascb.Build(), new AccountStatusChangedValidator(Mock.Of<IDatabaseCommands>()) };

            var acfb = AccountCreateFailed.CreateBuilder();
            acfb.SetInternalId("1").SetReason(AccountCreateFailedReason.INVALID_CREATE_DATA).SetSource(SourceSystem.STARCHEF);
            yield return new object[] { acfb.Build(), new AccountCreateFailedValidator(Mock.Of<IDatabaseCommands>()) };

            var aufb = AccountUpdateFailed.CreateBuilder();
            aufb.SetExternalId("1").SetCommandId("1").SetReason(AccountUpdateFailedReason.INVALID_UPDATE_DATA).SetSource(SourceSystem.STARCHEF);
            yield return new object[] { aufb.Build(), new AccountUpdateFailedValidator(Mock.Of<IDatabaseCommands>()) };

            var ascfb = AccountStatusChangeFailed.CreateBuilder();
            ascfb.SetExternalId("1").SetSource(SourceSystem.STARCHEF);
            yield return new object[] { ascfb.Build(), new AccountStatusChangeFailedValidator(Mock.Of<IDatabaseCommands>()) };
        }

        public static IEnumerable<object[]> AccountEventsWithNonStarChefSourceSystem()
        {
            var acb = AccountCreated.CreateBuilder();
            acb.SetExternalId("1").SetSource(SourceSystem.ADACO);
            yield return new object[] { acb.Build(), new AccountCreatedValidator(Mock.Of<IDatabaseCommands>())};

            var aub = AccountUpdated.CreateBuilder();
            aub.SetExternalId("1").SetSource(SourceSystem.ADACO);
            yield return new object[] { aub.Build(), new AccountUpdatedValidator(Mock.Of<IDatabaseCommands>()) };

            var ascb = AccountStatusChanged.CreateBuilder();
            ascb.SetExternalId("1").SetStatus(AccountStatus.ACTIVE).SetSource(SourceSystem.ADACO);
            yield return new object[] { ascb.Build(), new AccountStatusChangedValidator(Mock.Of<IDatabaseCommands>()) };

            var acfb = AccountCreateFailed.CreateBuilder();
            acfb.SetInternalId("1").SetReason(AccountCreateFailedReason.INVALID_CREATE_DATA).SetSource(SourceSystem.ADACO);
            yield return new object[] { acfb.Build(), new AccountCreateFailedValidator(Mock.Of<IDatabaseCommands>()) };

            var aufb = AccountUpdateFailed.CreateBuilder();
            aufb.SetExternalId("1").SetCommandId("1").SetReason(AccountUpdateFailedReason.INVALID_UPDATE_DATA).SetSource(SourceSystem.ADACO);
            yield return new object[] { aufb.Build(), new AccountUpdateFailedValidator(Mock.Of<IDatabaseCommands>()) };

            var ascfb = AccountStatusChangeFailed.CreateBuilder();
            ascfb.SetExternalId("1").SetSource(SourceSystem.ADACO);
            yield return new object[] { ascfb.Build(), new AccountStatusChangeFailedValidator(Mock.Of<IDatabaseCommands>()) };
        }

        public static IEnumerable<object[]> AccountEventsWithoutSourceSystem()
        {
            var acb = AccountCreated.CreateBuilder();
            acb.SetExternalId("1");
            yield return new object[] { acb.Build(), new AccountCreatedValidator(Mock.Of<IDatabaseCommands>()) };

            var aub = AccountUpdated.CreateBuilder();
            aub.SetExternalId("1");
            yield return new object[] { aub.Build(), new AccountUpdatedValidator(Mock.Of<IDatabaseCommands>()) };

            var ascb = AccountStatusChanged.CreateBuilder();
            ascb.SetExternalId("1").SetStatus(AccountStatus.ACTIVE);
            yield return new object[] { ascb.Build(), new AccountStatusChangedValidator(Mock.Of<IDatabaseCommands>()) };

            var acfb = AccountCreateFailed.CreateBuilder();
            acfb.SetInternalId("1").SetReason(AccountCreateFailedReason.INVALID_CREATE_DATA);
            yield return new object[] { acfb.Build(), new AccountCreateFailedValidator(Mock.Of<IDatabaseCommands>()) };

            var aufb = AccountUpdateFailed.CreateBuilder();
            aufb.SetExternalId("1").SetCommandId("1").SetReason(AccountUpdateFailedReason.INVALID_UPDATE_DATA);
            yield return new object[] { aufb.Build(), new AccountUpdateFailedValidator(Mock.Of<IDatabaseCommands>()) };

            var ascfb = AccountStatusChangeFailed.CreateBuilder();
            ascfb.SetExternalId("1");
            yield return new object[] { ascfb.Build(), new AccountStatusChangeFailedValidator(Mock.Of<IDatabaseCommands>()) };
        }
    }
}