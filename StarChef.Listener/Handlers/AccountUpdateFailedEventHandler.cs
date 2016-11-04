using System;
using System.Threading.Tasks;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Model.People;

namespace StarChef.Listener.Handlers
{
    public class AccountUpdateFailedEventHandler : IMessageHandler<Events.AccountUpdateFailed>
    {
        public Task<MessageHandlerResult> HandleAsync(Events.AccountUpdateFailed payload, string trackingId)
        {
            throw new NotImplementedException();
        }
    }
}