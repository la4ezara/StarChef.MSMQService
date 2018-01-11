using log4net;
using System;
using System.Messaging;

namespace StarChef.MSMQService
{
    public interface IMessageManager
    {
        void mqDisconnect();
        void mqSend(UpdateMessage message, MessagePriority priority);
        void mqSendToPoisonQueue(UpdateMessage message, MessagePriority priority);
        Message mqReceive(string messageId, TimeSpan timeout);
        Message mqPeek(TimeSpan timeout);
    }

	/// <summary>
	/// Summary description for MSMQManager.
	/// </summary>
	public class MsmqManager : IMessageManager
    {
        private static readonly ILog Logger =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _normalQueueName;
        private readonly string _poisonQueueName;

        private MessageQueue mq = null;

		public MsmqManager(string queueName, string poisonQueueName)
		{
            this._normalQueueName = queueName;
            this._poisonQueueName = poisonQueueName;
        }

		private MessageQueue mqConnect()
		{
			MessageQueue queue;
			
			if (MessageQueue.Exists(this._normalQueueName))
			{
				queue = new MessageQueue(this._normalQueueName);
				queue.DefaultPropertiesToSend.Recoverable = true;
				
				// Added to make sure we can read the AppSpecific property of messages on this queue
				// MTB - 2005-07-28
				MessagePropertyFilter mf = new MessagePropertyFilter();
				mf.SetAll();
				mf.AppSpecific = true;
				queue.MessageReadPropertyFilter = mf;
				// MTB - 2005-07-28

				return queue;
			}
			else
			{
				throw new Exception("StarChef Message Queue: " + this._normalQueueName + " does not exist. Please check MSMQ Setup");
			}
		}

		public void mqDisconnect()
		{
            if (mq != null)
            {
                mq.Close();
            }
			mq = null;
		}

		public void mqSend(UpdateMessage message, MessagePriority priority)
		{
            if (mq == null)
            {
                mq = mqConnect();
            }

			var msg = new Message(message) { Priority = priority };

            //msg.Recoverable = true;	// this is now set as a default value
            if (mq == null)
            {
                throw new Exception("StarChef: No connection to Message queue. Cannot proceed.");
            }

			mq.Send(msg, message.ToString());
		}

        public void mqSendToPoisonQueue(UpdateMessage message, MessagePriority priority)
        {
            try
            {
                if (MessageQueue.Exists(_poisonQueueName))
                {
                    using (MessageQueue q = new MessageQueue(_poisonQueueName))
                    {
                        q.DefaultPropertiesToSend.Recoverable = true;
                        MessagePropertyFilter mf = new MessagePropertyFilter();
                        mf.SetAll();
                        mf.AppSpecific = true;
                        q.MessageReadPropertyFilter = mf;

                        var msg = new Message(message) { Priority = priority };

                        q.Send(msg, message.ToString());
                    }
                }
                else
                {
                    Logger.Error(new Exception("StarChef Message Queue: " + _poisonQueueName + " does not exist. Please check MSMQ Setup"));
                }
            }
            catch (Exception ex)
            {
                Logger.Error("StarChef: MSMQ Errored while adding a message to the Poison Queue.", ex);
            }
        }

	    public Message mqReceive(string messageId, TimeSpan timeout)
		{
		    try
		    {
                if (mq == null)
                {
                    mq = mqConnect();
                }

                var msg = mq.ReceiveById(messageId, timeout);
                return msg;
		    }
            catch (MessageQueueException exception)
            {
                if (exception.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout &&
                    exception.MessageQueueErrorCode != MessageQueueErrorCode.MessageNotFound)
                    throw new Exception(exception.Message, exception);
                return null;
            }
		}

        public Message mqPeek(TimeSpan timeout)
        {
            try
            {
                if (mq == null)
                {
                    mq = mqConnect();
                }

                var msg = mq.Peek(timeout);
                return msg;
            }
            catch (MessageQueueException exception)
            {
                if (exception.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout &&
                    exception.MessageQueueErrorCode != MessageQueueErrorCode.MessageNotFound)
                    throw new Exception(exception.Message, exception);
                return null;
            }
        }
    }
}