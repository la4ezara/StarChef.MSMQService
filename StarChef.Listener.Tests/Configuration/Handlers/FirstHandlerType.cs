using System;
using System.Threading.Tasks;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Model.Recipes;

namespace StarChef.Listener.Tests.Configuration.Handlers
{
    public class FirstHandlerType : IMessageHandler<Events.PriceBandUpdated>
    {
        public Task<MessageHandlerResult> HandleAsync(Events.PriceBandUpdated payload, string trackingId)
        {
            return null;
        }
    }
}
