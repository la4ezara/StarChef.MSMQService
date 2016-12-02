using System;
using StarChef.Listener.Types;
using AccountStatusChanged = Fourth.Orchestration.Model.People.Events.AccountStatusChanged;

namespace StarChef.Listener.Validators
{
    class AccountStatusChangedValidator : EventValidator, IEventValidator
    {
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