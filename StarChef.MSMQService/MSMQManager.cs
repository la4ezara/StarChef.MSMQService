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
		private MessageQueueTransaction mqt = null;

		public MessageQueue Queue
		{
			get {return mq;}
			set {mq = value;}
		}

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

		public void mqTransactionBegin()
		{
			if (mq == null) mq = mqConnect();
			
			if (!mq.Transactional) 
				throw new Exception("StarChef Message Queue: " + this._queueName + " is not transactional. Please check MSMQ Setup");;

			if (mqt == null)
			{
				mqt = new MessageQueueTransaction();
				mqt.Begin();
			}
			else
			{
				mqt.Begin();
			}
		}

		public void mqTransactionAbort()
		{	
			if (mq == null) return;

			if (mq.Transactional)
			{
				mqt.Abort();			
			}
		}

		public void mqTransactionCommit()
		{
			if (mq == null) return;
			if (mq.Transactional)
			{
				mqt.Commit();
			}
		}

		public void mqSend(UpdateMessage message, MessagePriority priority)
		{
			if (mq == null) mq = mqConnect();

			Message msg = new Message(message);
		    
            msg.Priority = priority;
            
            //msg.Recoverable = true;	// this is now set as a default value
			if (mq == null) throw new Exception ("StarChef: No connection to Message queue. Cannot proceed.");

			if (mq.Transactional)
			{
				if (mqt != null) 
				{
					mq.Send(msg, message.ToString(), mqt);
				}
				else
				{
					mq.Send(msg, message.ToString());					
				}
			}
			else
			{
				mq.Send(msg, message.ToString());
			}
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
                if (mq == null) mq = mqConnect();

                if (mq.Transactional)
                {
                    if (mqt != null)
                    {
                        msg = mq.ReceiveById(messageId, new TimeSpan(10), mqt);
                    }
                    else
                    {
                        msg = mq.ReceiveById(messageId, new TimeSpan(10));
                    }
                }
                else
                {
                    msg = mq.ReceiveById(messageId, new TimeSpan(10));
                }
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

        public Cursor mqCreateCursor()
        {
            try
            {
                if (mq == null) mq = mqConnect();

                return mq.CreateCursor();

            }
            catch (MessageQueueException exception)
            {
                if (exception.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout &&
                    exception.MessageQueueErrorCode != MessageQueueErrorCode.MessageNotFound)
                    throw new Exception(exception.Message, exception);
                return null;
            }
        }

        public Message mqPeek(Cursor cursor, PeekAction action)
        {
            try
            {
                if (mq == null) mq = mqConnect();

                return mq.Peek(new TimeSpan(1), cursor, action);
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

	/// <summary>
	/// Added to handle web service query messages
	/// MTB - 2005-07-28
	/// </summary>
	[Serializable]
	public class WebServiceQueryMessage
	{
		private string _requestGuid = string.Empty;
		private string _sp_name_and_paramsXML = string.Empty;
		private string _dbDSN = string.Empty;

		public WebServiceQueryMessage()
		{
		}

		public WebServiceQueryMessage(string RequestGuid, string sp_name_and_paramsXML, string UserDSN)
		{
			_requestGuid = RequestGuid;
			_sp_name_and_paramsXML = sp_name_and_paramsXML;
			_dbDSN = UserDSN;
		}

		public string RequestGuid
		{
			get {return _requestGuid;}
			set {_requestGuid = value;}
		}

		public string sp_name_and_paramsXML
		{
			get {return _sp_name_and_paramsXML;}
			set {_sp_name_and_paramsXML = value;}
		}
	
		public string DSN
		{
			get {return _dbDSN;}
			set {_dbDSN = value;}
		}
	}

	/// <summary>
	/// Added to handle reporting messages
	/// SEM - 2005-10-31
	/// </summary>
	[Serializable]
	public class ReportingMessage
	{
		private string _reportGuid = string.Empty;
		private string _reportName = string.Empty;
		private string _parameterXML = string.Empty;
		private string _filterXML = string.Empty;
		private string _outputFilterXML = string.Empty;
		private string _dbDSN = string.Empty;
		private Constants.ReportOutputType _format = Constants.ReportOutputType.NotSet;
		private int _groupFilterId = 0;
		private Constants.GroupFilterType _groupFilterType = Constants.GroupFilterType.NotSet;
		private DateTime _startDate;
		private DateTime _endDate;
		private Constants.ScopeType _scopeFilterID = Constants.ScopeType.NotSet;
		private int _startDay = 0;
		private int _endDay = 0;

		public ReportingMessage()
		{
		}

		public ReportingMessage(string ReportGuid, string ReportName, string parameterXML, string filterXML, string outputFilterXML, string UserDSN, Constants.ReportOutputType Format, int GroupFilterID, Constants.GroupFilterType GroupFilterType, DateTime StartDate, DateTime EndDate, int StartDay, int EndDay, Constants.ScopeType ScopeFilterID)
		{
			_reportGuid = ReportGuid;
			_reportName = ReportName;
			_parameterXML = parameterXML;
			_filterXML = filterXML;
			_outputFilterXML = outputFilterXML;
			_dbDSN = UserDSN;
			_format = Format;
			_groupFilterId = GroupFilterID;
			_groupFilterType = GroupFilterType;
			_startDate = StartDate;
			_endDate = EndDate;
			_startDay = StartDay;
			_endDay = EndDay;
			_scopeFilterID = ScopeFilterID;
		}

		public string ReportName
		{
			get {return _reportName;}
			set {_reportName = value;}
		}

		public string ReportGuid
		{
			get {return _reportGuid;}
			set {_reportGuid = value;}
		}

		public string parameterXML
		{
			get {return _parameterXML;}
			set {_parameterXML = value;}
		}
	
		public string filterXML
		{
			get {return _filterXML;}
			set {_filterXML = value;}
		}

		public string OutputFilterXML
		{
			get{ return _outputFilterXML;}
			set{_outputFilterXML = value;}
		}
	
		public string DSN
		{
			get {return _dbDSN;}
			set {_dbDSN = value;}
		}

		public Constants.ReportOutputType Format
		{
			get {return _format;}
			set {_format = value;}
		}

		public int GroupFilterID
		{
			get {return _groupFilterId;}
			set {_groupFilterId = value;}
		}

		public Constants.GroupFilterType GroupFilterType
		{
			get {return _groupFilterType;}
			set {_groupFilterType = value;}
		}

		public DateTime StartDate
		{
			get {return _startDate;}
			set {_startDate = value;}
		}

		public DateTime EndDate
		{
			get {return _endDate;}
			set {_endDate = value;}
		}

		public Constants.ScopeType ScopeFilterID
		{
			get {return _scopeFilterID;}
			set {_scopeFilterID = value;}
		}

		public int StartDay
		{
			get {return _startDay;}
			set {_startDay = value;}
		}

		public int EndDay
		{
			get {return _endDay;}
			set {_endDay = value;}
		}
	}
}

