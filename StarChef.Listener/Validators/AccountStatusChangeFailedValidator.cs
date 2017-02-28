using System;
using StarChef.Listener.Commands;
using StarChef.Listener.Types;
using AccountStatusChangeFailed = Fourth.Orchestration.Model.People.Events.AccountStatusChangeFailed;

namespace StarChef.Listener.Validators
{
    class AccountStatusChangeFailedValidator : EventValidator, IEventValidator
    {
        public AccountStatusChangeFailedValidator(IDatabaseCommands databaseCommands) : base(databaseCommands)
        {
        }

        public bool IsEnabled(object payload)
        {
            var e = payload as AccountStatusChangeFailed;
            if (e == null)
                throw new ArgumentException("The type of the payload is not supported");

            return GetFromDbConfiguration(e.ExternalId, typeof(AccountStatusChangeFailed).Name);
        }

        public bool IsValid(object payload)
        {
            if (payload == null) return false;
            if (payload.GetType() != typeof(AccountStatusChangeFailed)) return false;
            var e = (AccountStatusChangeFailed)payload;

            if (!e.HasExternalId)
            {
                SetLastError("ExternalId is missing");
                return false;
            }
            return true;
        }
    }
}