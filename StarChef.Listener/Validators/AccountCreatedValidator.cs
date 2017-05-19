using System;
using System.Reflection;
using log4net;
using StarChef.Listener.Commands;
using StarChef.Listener.Types;
using AccountCreated = Fourth.Orchestration.Model.People.Events.AccountCreated;

namespace StarChef.Listener.Validators
{
    class AccountCreatedValidator : AccountEventValidator, IEventValidator
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public AccountCreatedValidator(IDatabaseCommands databaseCommands) : base(databaseCommands)
        {
        }

        public override bool IsEnabled(object payload)
        {
            var e = payload as AccountCreated;
            if (e == null)
                throw new ArgumentException("The type of the payload is not supported");

            var loginId = int.Parse(e.InternalId);
            return GetFromDbConfiguration(loginId, typeof(AccountCreated).Name);
        }

        public bool IsValidPayload(object payload)
        {
            _logger.Info("Validating the payload");

            if (payload == null) return false;
            if (payload.GetType() != typeof(AccountCreated)) return false;
            var e = (AccountCreated) payload;

            if (!e.HasInternalId)
            {
                SetLastError("InternalId is missing");
                return false;
            }
            int internalId;
            if (!int.TryParse(e.InternalId, out internalId))
            {
                SetLastError("InternalId is not Int32: " + e.InternalId);
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
            if (!e.HasUsername)
            {
                SetLastError("Username is missing");
                return false;
            }
            if (e.Username.Length > 50)
            {
                SetLastError("Username exceeds the maximum length of 50 characters.");
                return false;
            }

            _logger.Info("Payload is valid");
            return true;
        }
    }
}