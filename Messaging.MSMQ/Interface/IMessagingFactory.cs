using System;

namespace Messaging.MSMQ.Interface
{
    public interface IMessagingFactory : IDisposable
    {
        IMessageBus CreateMessageBus();
        void Close();
    }
}