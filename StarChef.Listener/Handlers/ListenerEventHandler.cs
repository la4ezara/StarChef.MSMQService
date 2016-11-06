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

        protected ListenerEventHandler()
        {
            DbCommands = new PriceBandCommands(new ConnectionStringProvider());
        }

        protected ListenerEventHandler(IDatabaseCommands dbCommands)
        {
            DbCommands = dbCommands;
        }
    }
}
