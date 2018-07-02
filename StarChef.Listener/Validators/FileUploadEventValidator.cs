namespace StarChef.Listener.Validators
{
    using log4net;
    using StarChef.Listener.Commands;
    using StarChef.Listener.Types;
    using System.Linq;
    using System.Reflection;
    using FileUploadCompleted = Fourth.Orchestration.Model.StarChef.Events.FileUploadCompleted;

    public class FileUploadEventValidator : EventValidator, IEventValidator
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public FileUploadEventValidator(IDatabaseCommands databaseCommands) : base(databaseCommands)
        {
        }

        public override bool IsEnabled(object payload)
        {
            var e = payload as FileUploadCompleted;
            if (e == null)
                return false;

            return true;
        }

        public override bool IsAllowedEvent(object payload)
        {
            var supportedEvents = new[]
            {
                typeof (FileUploadCompleted)
            };
            if (payload == null) return false;

            if (!supportedEvents.Contains(payload.GetType())) return false;

            return true;
        }

        public bool IsValidPayload(object payload)
        {
            _logger.Info("Validating the payload");

            if (payload == null) return false;
            if (payload.GetType() != typeof(FileUploadCompleted)) return false;
            var e = (FileUploadCompleted)payload;

            if (!e.HasInternalCustomerId)
            {
                SetLastError("InternalCustomerId is missing");
                return false;
            }
            if (!e.HasFilePath)
            {
                SetLastError("FilePath is missing");
                return false;
            }
            return true;
        }
    }
}
