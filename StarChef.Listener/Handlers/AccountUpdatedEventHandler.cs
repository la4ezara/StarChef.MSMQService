using System;
using System.Threading.Tasks;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Model.People;
using StarChef.Listener.Commands;

namespace StarChef.Listener.Handlers
{
    public class AccountUpdatedEventHandler : ListenerEventHandler, IMessageHandler<Events.AccountUpdated>
    {
        public AccountUpdatedEventHandler()
        {
        }

        public AccountUpdatedEventHandler(IDatabaseCommands dbCommands) : base(dbCommands)
        {
        }

        public Task<MessageHandlerResult> HandleAsync(Events.AccountUpdated payload, string trackingId)
        {
            throw new NotImplementedException();
        }
    }
}