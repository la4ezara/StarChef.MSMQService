using System;
using System.Threading.Tasks;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Model.People;
using StarChef.Listener.Commands;

namespace StarChef.Listener.Handlers
{
    public class AccountCreateFailedEventHandler : ListenerEventHandler, IMessageHandler<Events.AccountCreateFailed>
    {
        public AccountCreateFailedEventHandler()
        {
        }

        public AccountCreateFailedEventHandler(IDatabaseCommands customerDb) : base(customerDb)
        {
        }

        public Task<MessageHandlerResult> HandleAsync(Events.AccountCreateFailed payload, string trackingId)
        {
            throw new NotImplementedException();
        }
    }
}