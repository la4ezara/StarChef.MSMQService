using AccountUpdated = Fourth.Orchestration.Model.People.Events.AccountUpdated;

namespace StarChef.Listener.Types
{
    class AccountUpdatedValidator : EventValidator, IEventValidator
    {
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
            if (!e.HasExternalId)
            {
                SetLastError("ExternalId is missing");
                return false;
            }
            return true;
        }
    }
}