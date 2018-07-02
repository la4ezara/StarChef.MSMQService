using System;
using System.Linq;
using System.Management.Instrumentation;
using StarChef.Listener.Commands;
using AccountCreated = Fourth.Orchestration.Model.People.Events.AccountCreated;
using AccountCreateFailed = Fourth.Orchestration.Model.People.Events.AccountCreateFailed;
using AccountUpdated = Fourth.Orchestration.Model.People.Events.AccountUpdated;
using AccountUpdateFailed = Fourth.Orchestration.Model.People.Events.AccountUpdateFailed;
using AccountStatusChanged = Fourth.Orchestration.Model.People.Events.AccountStatusChanged;
using AccountStatusChangeFailed = Fourth.Orchestration.Model.People.Events.AccountStatusChangeFailed;
using SourceSystem = Fourth.Orchestration.Model.People.Events.SourceSystem;

namespace StarChef.Listener.Types
{
    public abstract class EventValidator
    {
        private string _lastError = string.Empty;
        protected readonly IDatabaseCommands _databaseCommands;

        protected EventValidator(IDatabaseCommands databaseCommands)
        {
            _databaseCommands = databaseCommands;
        }

        public virtual bool IsEnabled(object payload)
        {
            return true;
        }

        public virtual string GetErrors()
        {
            return _lastError;
        }

        /// <summary>
        /// If derived this method validates the event type and issuer are supported by the handler
        /// </summary>
        /// <param name="payload">Event payload</param>
        /// <returns></returns>
        public virtual bool IsAllowedEvent(object payload)
        {
            return true;
        }

        protected void SetLastError(string error)
        {
            _lastError = error;
        }

        protected bool GetFromDbConfiguration(Guid organizationGuid, string eventTypeShortName)
        {
            var result = _databaseCommands.IsEventEnabledForOrganization(eventTypeShortName, organizationGuid).Result;
            return result;
        }

        protected bool GetFromDbConfiguration(int loginId, string eventTypeShortName)
        {
            var result = _databaseCommands.IsEventEnabledForOrganization(eventTypeShortName, loginId).Result;
            return result;
        }

        protected bool GetFromDbConfiguration(string externalId, string eventTypeShortName)
        {
            var result = _databaseCommands.IsEventEnabledForOrganization(eventTypeShortName, externalId).Result;
            return result;
        }
    }
}
