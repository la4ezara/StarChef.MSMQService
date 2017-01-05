using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarChef.Listener.Commands;
using StarChef.Listener.Commands.Impl;

namespace StarChef.Listener.Handlers
{
    public abstract class ListenerEventHandler
    {
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
