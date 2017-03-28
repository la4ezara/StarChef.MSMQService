using System;
using StarChef.Listener.Commands;
using StarChef.Listener.Types;
using AccountCreateFailed = Fourth.Orchestration.Model.People.Events.AccountCreateFailed;

namespace StarChef.Listener.Validators
{
    class AccountCreateFailedValidator : AccountEventValidator, IEventValidator
    {
        public AccountCreateFailedValidator(IDatabaseCommands databaseCommands) : base(databaseCommands)
        {
        }

        public override bool IsEnabled(object payload)
        {
            var e = payload as AccountCreateFailed;
            if (e == null)
                throw new ArgumentException("The type of the payload is not supported");

            var loginId = int.Parse(e.InternalId);
            return GetFromDbConfiguration(loginId, typeof(AccountCreateFailed).Name);
        }

        public bool IsValid(object payload)
        {
            if (payload == null) return false;
            if (payload.GetType() != typeof(AccountCreateFailed)) return false;
            var e = (AccountCreateFailed)payload;

            if (!e.HasInternalId)
            {
                SetLastError("InternalId is missing");
                return false;
            }
            if (!e.HasReason)
            {
                SetLastError("Reason is missing");
                return false;
            }
            return true;
        }
    }
}