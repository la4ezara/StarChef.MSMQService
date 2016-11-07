using AccountCreated = Fourth.Orchestration.Model.People.Events.AccountCreated;

namespace StarChef.Listener.Types
{
    class AccountCreatedValidator : EventValidator, IEventValidator
    {
        public bool IsValid(object payload)
        {
            if (payload == null) return false;
            if (payload.GetType() != typeof(AccountCreated)) return false;
            var e = (AccountCreated) payload;

            if (!e.HasInternalId)
            {
                SetLastError("InternalId is missing");
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