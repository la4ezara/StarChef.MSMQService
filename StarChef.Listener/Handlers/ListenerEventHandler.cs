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
        protected readonly IDatabaseCommands DbCommands;
        protected IEventValidator Validator;
        protected IMessagingLogger MessagingLogger;

        protected ListenerEventHandler(IDatabaseCommands dbCommands, IEventValidator validator, IMessagingLogger messagingLogger)
        {
            DbCommands = dbCommands;
            Validator = validator;
            MessagingLogger = messagingLogger;
        }
    }
}
