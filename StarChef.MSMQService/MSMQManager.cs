using log4net;
using System;
using System.Configuration;
using System.Messaging;
using Fourth.StarChef.Invariables;

namespace StarChef.MSMQService
{
	/// <summary>
	/// Summary description for MSMQManager.
	/// </summary>
	public class MSMQManager
	{
        private static readonly ILog Logger =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string _queueName;

		private MessageQueue mq = null;

		public MSMQManager(string queueName)
		{
            this._queueName = queueName;
        }

		private MessageQueue mqConnect()
		{
			MessageQueue queue;
			
			if (MessageQueue.Exists(this._queueName))
			{
				queue = new MessageQueue(this._queueName);
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
				throw new Exception("StarChef Message Queue: " + this._queueName + " does not exist. Please check MSMQ Setup");
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

			Message msg = new Message(message);
		    
            msg.Priority = priority;

            //msg.Recoverable = true;	// this is now set as a default value
            if (mq == null)
            {
                throw new Exception("StarChef: No connection to Message queue. Cannot proceed.");
            }

			mq.Send(msg, message.ToString());
		}

        public void mqSendToPoisonQueue(string queueName, UpdateMessage message, MessagePriority priority)
        {
            try
            {
                if (MessageQueue.Exists(queueName))
                {
                    MessageQueue q = new MessageQueue(queueName);
                    q.DefaultPropertiesToSend.Recoverable = true;
                    MessagePropertyFilter mf = new MessagePropertyFilter();
                    mf.SetAll();
                    mf.AppSpecific = true;
                    q.MessageReadPropertyFilter = mf;

                    Message msg = new Message(message);
                    msg.Priority = priority;
                    
                    q.Send(msg, message.ToString());
                }
                else
                {
                    Logger.Error(new Exception("StarChef Message Queue: " + queueName + " does not exist. Please check MSMQ Setup"));
                }
            }
            catch (Exception ex)
            {
                Logger.Error("StarChef: MSMQ Errored while adding a message to the Poison Queue.", ex);
            }
        }

	    public Message mqReceive(string messageId)
		{
		    try
		    {
                Message msg;
                if (mq == null)
                {
                    mq = mqConnect();
                }

                msg = mq.ReceiveById(messageId, new TimeSpan(10));
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
                if (mq == null) mq = mqConnect();

                return mq.Peek(timeout);
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

