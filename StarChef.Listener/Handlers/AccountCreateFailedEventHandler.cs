using System;
using System.Threading.Tasks;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Model.People;

namespace StarChef.Listener.Handlers
{
    public class AccountCreateFailedEventHandler : IMessageHandler<Events.AccountCreateFailed>
    {
        public Task<MessageHandlerResult> HandleAsync(Events.AccountCreateFailed payload, string trackingId)
        {
            throw new NotImplementedException();
        }
    }
}