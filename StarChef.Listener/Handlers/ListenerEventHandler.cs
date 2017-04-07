using StarChef.Listener.Commands;

namespace StarChef.Listener.Handlers
{
    public abstract class ListenerEventHandler
    {
        public const string EXTERNAL_ID = "ExternalId";
        public const string INTERNAL_ID = "InternalId";
        public const string DATABASE_GUID = "DatabaseGuid";
        public IDatabaseCommands DbCommands { get; private set; }
        public IEventValidator Validator { get; private set; }
        public IMessagingLogger MessagingLogger { get; private set; }

        protected ListenerEventHandler(IDatabaseCommands dbCommands, IEventValidator validator, IMessagingLogger messagingLogger)
        {
            DbCommands = dbCommands;
            Validator = validator;
            MessagingLogger = messagingLogger;
        }
    }
}
