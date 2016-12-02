using StarChef.Listener.Types;
using AccountCreateFailed = Fourth.Orchestration.Model.People.Events.AccountCreateFailed;

namespace StarChef.Listener.Validators
{
    class AccountCreateFailedValidator : EventValidator, IEventValidator
    {
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