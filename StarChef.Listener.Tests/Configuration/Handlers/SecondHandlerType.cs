using Fourth.Orchestration.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fourth.Orchestration.Model.Recipes;

namespace StarChef.Listener.Tests.Configuration.Handlers
{
    public class SecondHandlerType : IMessageHandler<Events.PriceBandUpdated>
    {
        public Task<MessageHandlerResult> HandleAsync(Events.PriceBandUpdated payload, string trackingId)
        {
            return null;
        }
    }
}
