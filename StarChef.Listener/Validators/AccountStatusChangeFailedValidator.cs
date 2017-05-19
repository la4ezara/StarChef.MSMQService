using System;
using System.Reflection;
using log4net;
using StarChef.Listener.Commands;
using StarChef.Listener.Types;
using AccountStatusChangeFailed = Fourth.Orchestration.Model.People.Events.AccountStatusChangeFailed;

namespace StarChef.Listener.Validators
{
    class AccountStatusChangeFailedValidator : AccountEventValidator, IEventValidator
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public AccountStatusChangeFailedValidator(IDatabaseCommands databaseCommands) : base(databaseCommands)
        {
        }

        public override bool IsEnabled(object payload)
        {
            var e = payload as AccountStatusChangeFailed;
            if (e == null)
                throw new ArgumentException("The type of the payload is not supported");

            return GetFromDbConfiguration(e.ExternalId, typeof(AccountStatusChangeFailed).Name);
        }

        public bool IsValidPayload(object payload)
        {
            _logger.Info("Validating the payload");

            if (payload == null) return false;
            if (payload.GetType() != typeof(AccountStatusChangeFailed)) return false;
            var e = (AccountStatusChangeFailed)payload;

            if (!e.HasExternalId)
            {
                SetLastError("ExternalId is missing");
                return false;
            }

            _logger.Info("Payload is valid");
            return true;
        }
    }
}