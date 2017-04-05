using System;
using System.Reflection;
using log4net;
using StarChef.Listener.Commands;
using StarChef.Listener.Types;
using AccountCreateFailed = Fourth.Orchestration.Model.People.Events.AccountCreateFailed;

namespace StarChef.Listener.Validators
{
    class AccountCreateFailedValidator : AccountEventValidator, IEventValidator
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public AccountCreateFailedValidator(IDatabaseCommands databaseCommands) : base(databaseCommands)
        {
        }

        public override bool IsEnabled(object payload)
        {
            var e = payload as AccountCreateFailed;
            if (e == null)
                throw new ArgumentException("The type of the payload is not supported");

            var loginId = int.Parse(e.InternalId);
            return GetFromDbConfiguration(loginId, typeof(AccountCreateFailed).Name);
        }

        public bool IsValidPayload(object payload)
        {
            _logger.Info("Validating the payload");

            if (payload == null) return false;
            if (payload.GetType() != typeof(AccountCreateFailed)) return false;
            var e = (AccountCreateFailed)payload;

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
            if (!e.HasReason)
            {
                SetLastError("Reason is missing");
                return false;
            }

            _logger.Info("Payload is valid");
            return true;
        }
    }
}