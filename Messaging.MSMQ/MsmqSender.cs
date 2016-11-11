using Messaging.MSMQ.Interface;
using System;
using System.Messaging;

namespace Messaging.MSMQ
{
    public class MsmqSender : IMsmqSender
    {
        private readonly MessageQueue queue;

        public MsmqSender(string queueName)
        {
            if (MessageQueue.Exists(queueName))
            {
                queue = new MessageQueue(queueName);

                queue.DefaultPropertiesToSend.Recoverable = true;

                // Added to make sure we can read the AppSpecific property of messages on this queue
                // MTB - 2005-07-28
                var mf = new MessagePropertyFilter();
                mf.SetAll();
                mf.AppSpecific = true;
                queue.MessageReadPropertyFilter = mf;
            }
            else
            {
                throw new Exception("StarChef Message Queue: " + queueName + " does not exist. Please check MSMQ Setup");
            }
        }
        public string QueueName
        {
            get
            {
                if (this.queue != null)
                {
                    return queue.QueueName;
                }
                else
                {
                    return "[null]";
                }
            }
        }

        public void Send(IMessage message)
        {
            Message msg = new Message(message);

            if (this.queue == null) throw new Exception("StarChef: No connection to Message queue. Cannot proceed.");

            if(queue.Transactional)
            {
                using (var transaction = new MessageQueueTransaction())
                {
                    transaction.Begin();
                    this.queue.Send(msg, message.ToString(), transaction);
                    this.queue.Peek();
                    transaction.Commit();
                }
            }
            else
            {
                queue.Send(msg, message.ToString());
            }
        }

        ~MsmqSender()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.queue != null)
                {
                    this.queue.Dispose();
                }
            }
        }
    }
}
