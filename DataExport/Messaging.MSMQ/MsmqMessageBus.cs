using Fourth.StarChef.Invariables.Interfaces;
using Messaging.MSMQ.Interface;
using System.Threading.Tasks;

namespace Messaging.MSMQ
{
    public class MsmqMessageBus : IMessageBus
    {
        private readonly IMsmqSender sender;

        public MsmqMessageBus(IMsmqSender sender)
        {
            this.sender = sender;
        }
        public async Task<bool> SendAsync(IMessage message)
        {
            Task<bool> task = Task.Factory.StartNew(() => { return SendMessage(message); });
            return await task;
        }

        public bool SendMessage(IMessage message)
        {
            // Build and send the message
            sender.Send(message);

            return true;
        }
    }
}