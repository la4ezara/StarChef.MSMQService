using System;

namespace Messaging.MSMQ.Interface
{
    public interface IMessagingFactory
    {
        IMessageBus CreateMessageBus();
    }
}