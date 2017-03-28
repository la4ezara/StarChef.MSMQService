using System;
using System.Linq;
using System.Management.Instrumentation;
using StarChef.Listener.Commands;
using AccountCreated = Fourth.Orchestration.Model.People.Events.AccountCreated;
using AccountCreateFailed = Fourth.Orchestration.Model.People.Events.AccountCreateFailed;
using AccountUpdated = Fourth.Orchestration.Model.People.Events.AccountUpdated;
using AccountUpdateFailed = Fourth.Orchestration.Model.People.Events.AccountUpdateFailed;
using AccountStatusChanged = Fourth.Orchestration.Model.People.Events.AccountStatusChanged;
using AccountStatusChangeFailed = Fourth.Orchestration.Model.People.Events.AccountStatusChangeFailed;
using SourceSystem = Fourth.Orchestration.Model.People.Events.SourceSystem;

namespace StarChef.Listener.Types
{
    abstract class AccountEventValidator : EventValidator
    {
        public AccountEventValidator(IDatabaseCommands databaseCommands) : base(databaseCommands)
        {
        }

        public override bool IsAllowedEvent(object payload)
        {
            var supportedEvents = new[]
            {
                typeof (AccountCreated),
                typeof (AccountCreateFailed),
                typeof (AccountUpdated),
                typeof (AccountUpdateFailed),
                typeof (AccountStatusChanged),
                typeof (AccountStatusChangeFailed),
            };
            if (payload == null) return false;

            if (!supportedEvents.Contains(payload.GetType())) return false;

            dynamic p = payload; // Let's assign to dynamic var because there is no common type for the events, but they have common properties
            return p.HasSource && new[] { SourceSystem.STARCHEF /* other allowed source systems */}.Contains((SourceSystem)p.Source);
        }
    }
}
