using System;
using System.Threading.Tasks;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Model.People;
using StarChef.Listener.Commands;

namespace StarChef.Listener.Handlers
{
    public class AccountUpdateFailedEventHandler : ListenerEventHandler, IMessageHandler<Events.AccountUpdateFailed>
    {
        public AccountUpdateFailedEventHandler()
        {
        }

        public AccountUpdateFailedEventHandler(IDatabaseCommands customerDb) : base(customerDb)
        {
        }

        public Task<MessageHandlerResult> HandleAsync(Events.AccountUpdateFailed payload, string trackingId)
        {
            throw new NotImplementedException();
        }
    }
}