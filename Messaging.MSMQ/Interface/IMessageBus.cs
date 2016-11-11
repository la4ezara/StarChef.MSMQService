using System;
using System.Threading.Tasks;

namespace Messaging.MSMQ.Interface
{
    public interface IMessageBus : IDisposable
    {
        Task<bool> SendAsync(IMessage message);

        bool SendMessage(IMessage message);
    }
}