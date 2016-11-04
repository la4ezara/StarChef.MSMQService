using System;
using System.Threading.Tasks;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Model.People;

namespace StarChef.Listener.Handlers
{
    public class AccountUpdatedEventHandler : IMessageHandler<Events.AccountUpdated>
    {
        public Task<MessageHandlerResult> HandleAsync(Events.AccountUpdated payload, string trackingId)
        {
            throw new NotImplementedException();
        }
    }
}