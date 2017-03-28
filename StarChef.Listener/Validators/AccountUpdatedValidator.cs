using System;
using StarChef.Listener.Commands;
using StarChef.Listener.Types;
using AccountUpdated = Fourth.Orchestration.Model.People.Events.AccountUpdated;

namespace StarChef.Listener.Validators
{
    class AccountUpdatedValidator : AccountEventValidator, IEventValidator
    {
        public AccountUpdatedValidator(IDatabaseCommands databaseCommands) : base(databaseCommands)
        {
        }

        public override bool IsEnabled(object payload)
        {
            var e = payload as AccountUpdated;
            if (e == null)
                throw new ArgumentException("The type of the payload is not supported");

            return GetFromDbConfiguration(e.ExternalId, typeof(AccountUpdated).Name);
        }

        public bool IsValid(object payload)
        {
            if (payload == null) return false;
            if (payload.GetType() != typeof(AccountUpdated)) return false;
            var e = (AccountUpdated)payload;

            if (!e.HasUsername)
            {
                SetLastError("Username is missing");
                return false;
            }
            if (!e.HasExternalId)
            {
                SetLastError("ExternalId is missing");
                return false;
            }
            if (!e.HasFirstName)
            {
                SetLastError("FirstName is missing");
                return false;
            }
            if (!e.HasLastName)
            {
                SetLastError("LastName is missing");
                return false;
            }
            if (!e.HasEmailAddress)
            {
                SetLastError("EmailAddress is missing");
                return false;
            }
            return true;
        }
    }
}