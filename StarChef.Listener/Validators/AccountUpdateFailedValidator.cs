using StarChef.Listener.Commands;
using StarChef.Listener.Types;
using AccountUpdateFailed = Fourth.Orchestration.Model.People.Events.AccountUpdateFailed;

namespace StarChef.Listener.Validators
{
    class AccountUpdateFailedValidator : EventValidator, IEventValidator
    {
        public AccountUpdateFailedValidator(IDatabaseCommands databaseCommands) : base(databaseCommands)
        {
        }

        public bool IsValid(object payload)
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