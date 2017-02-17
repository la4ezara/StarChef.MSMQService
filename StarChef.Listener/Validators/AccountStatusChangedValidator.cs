using System;
using StarChef.Listener.Commands;
using StarChef.Listener.Types;
using AccountStatusChanged = Fourth.Orchestration.Model.People.Events.AccountStatusChanged;

namespace StarChef.Listener.Validators
{
    class AccountStatusChangedValidator : EventValidator, IEventValidator
    {
        public AccountStatusChangedValidator(IDatabaseCommands databaseCommands) : base(databaseCommands)
        {
        }

        public bool IsValid(object payload)
        {
            if (payload == null) return false;
            if (payload.GetType() != typeof(AccountStatusChanged)) return false;
            var e = (AccountStatusChanged)payload;

            if (!e.HasExternalId)
            {
                SetLastError("ExternalId is missing");
                return false;
            }
            return true;
        }
    }
}