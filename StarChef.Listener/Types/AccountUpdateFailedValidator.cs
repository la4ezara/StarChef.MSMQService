using AccountUpdateFailed = Fourth.Orchestration.Model.People.Events.AccountUpdateFailed;

namespace StarChef.Listener.Types
{
    class AccountUpdateFailedValidator : EventValidator, IEventValidator
    {
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