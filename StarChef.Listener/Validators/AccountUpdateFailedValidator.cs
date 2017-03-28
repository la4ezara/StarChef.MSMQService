using System;
using StarChef.Listener.Commands;
using StarChef.Listener.Types;
using AccountUpdateFailed = Fourth.Orchestration.Model.People.Events.AccountUpdateFailed;

namespace StarChef.Listener.Validators
{
    class AccountUpdateFailedValidator : AccountEventValidator, IEventValidator
    {
        public AccountUpdateFailedValidator(IDatabaseCommands databaseCommands) : base(databaseCommands)
        {
        }

        public override bool IsEnabled(object payload)
        {
            var e = payload as AccountUpdateFailed;
            if (e == null)
                throw new ArgumentException("The type of the payload is not supported");

            return GetFromDbConfiguration(e.ExternalId, typeof(AccountUpdateFailed).Name);
        }

        public bool IsValidPayload(object payload)
        {
            if (payload == null) return false;
            if (payload.GetType() != typeof(AccountUpdateFailed)) return false;
            var e = (AccountUpdateFailed)payload;

            if (!e.HasReason)
            {
                SetLastError("Reason is missing");
                return false;
            }
            if (!e.HasExternalId)
            {
                SetLastError("ExternalId is missing");
                return false;
            }
            return true;
        }
    }
}