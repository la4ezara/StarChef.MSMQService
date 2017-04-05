using System;
using System.Reflection;
using log4net;
using StarChef.Listener.Commands;
using StarChef.Listener.Types;
using AccountUpdated = Fourth.Orchestration.Model.People.Events.AccountUpdated;

namespace StarChef.Listener.Validators
{
    class AccountUpdatedValidator : AccountEventValidator, IEventValidator
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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

        public bool IsValidPayload(object payload)
        {
            _logger.Info("Validating the payload");

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

            _logger.Info("Payload is valid");
            return true;
        }
    }
}