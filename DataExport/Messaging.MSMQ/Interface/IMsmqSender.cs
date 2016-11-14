using System;
using System.Threading.Tasks;

namespace Messaging.MSMQ.Interface
{
    public interface IMsmqSender : IDisposable
    {
        void Send(IMessage message);

        string QueueName { get; }
    }
}