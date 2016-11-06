
using AccountCreated = Fourth.Orchestration.Model.People.Events.AccountCreated;
using AccountCreateFailed = Fourth.Orchestration.Model.People.Events.AccountCreateFailed;
using AccountUpdated = Fourth.Orchestration.Model.People.Events.AccountUpdated;
using AccountUpdateFailed = Fourth.Orchestration.Model.People.Events.AccountUpdateFailed;
using SourceSystem = Fourth.Orchestration.Model.People.Events.SourceSystem;

namespace StarChef.Listener.Types
{
    abstract class EventValidator
    {
        private string _lastError = string.Empty;

        public virtual string GetErrors()
        {
            return _lastError;
        }
        public virtual bool IsStarChefEvent(object payload)
        {
            if (payload == null) return false;

            if (payload.GetType() != typeof (AccountCreated)) return false;
            if (payload.GetType() != typeof (AccountCreateFailed)) return false;
            if (payload.GetType() != typeof (AccountUpdated)) return false;
            if (payload.GetType() != typeof (AccountUpdateFailed)) return false;

            dynamic p = payload; // Let's assign to dynamic var because there is no common type for the events, but they have common properties
            return p.HasSource && p.Source == SourceSystem.STARCHEF;
        }

        protected void SetLastError(string error)
        {
            _lastError = error;
        }
    }
}
