using Messaging.MSMQ.Interface;
using System;
using System.Threading.Tasks;

namespace Messaging.MSMQ
{
    public class MsmqMessageBus : IMessageBus
    {
        private readonly IMsmqSender sender;

        private bool isDisposed;

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
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            // Build and send the message
            sender.Send(message);

            return true;
        }
        ~MsmqMessageBus()
        {
            this.Dispose(false);
        }
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.isDisposed = true;

                if (!(this.sender == null))
                {
                    this.sender.Dispose();
                }
            }
        }
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
