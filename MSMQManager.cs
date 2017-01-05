using System;
using System.Configuration;
using System.Messaging;
using Microsoft.ApplicationBlocks.ExceptionManagement;
using StarChef.Data;

namespace StarChef.MSMQService
{
	/// <summary>
	/// Summary description for MSMQManager.
	/// </summary>
	public class MSMQManager
	{
		//private string _mqName = @"\\\\10.10.10.195\\StarChef_Calc";
		private const string _configKeyMQ = "StarChef.MSMQ.Queue";		
		
		private string _mqName = string.Empty;
		private MessageQueue mq = null;
		private MessageQueueTransaction mqt = null;

		public MessageQueue Queue
		{
			get {return mq;}
			set {mq = value;}
		}

		public string MQName
		{
			get {return _mqName;}
			set {_mqName = value;}
		}

		public MSMQManager()
		{			
			//
			// TODO: Add constructor logic here
			//
		}

		private MessageQueue mqConnect()
		{
			MessageQueue q = null;

			if (_mqName == string.Empty)
			{
				try
				{					
					_mqName = ConfigurationSettings.AppSettings[_configKeyMQ].ToString();
				}
				catch(Exception)
				{
					throw new Exception("StarChef: Cannot find MSMQ configuration. Key " + _configKeyMQ + " is missing from web.config file.");
				}
			}
			
			if (MessageQueue.Exists(_mqName))
			{
				q = new MessageQueue(_mqName);
				q.DefaultPropertiesToSend.Recoverable = true;
				
				// Added to make sure we can read the AppSpecific property of messages on this queue
				// MTB - 2005-07-28
				MessagePropertyFilter mf = new MessagePropertyFilter();
				mf.SetAll();
				mf.AppSpecific = true;
				q.MessageReadPropertyFilter = mf;
				// MTB - 2005-07-28

				return q;
			}
			else
			{
				throw new Exception("StarChef Message Queue: " + _mqName + " does not exist. Please check MSMQ Setup");
			}
		}

		public void mqDisconnect()
		{
			if (mq != null) mq.Close();
			mq = null;
		}

		public void mqTransactionBegin()
		{
			if (mq == null) mq = mqConnect();
			
			if (!mq.Transactional) 
				throw new Exception("StarChef Message Queue: " + _mqName + " is not transactional. Please check MSMQ Setup");;

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

        public void mqSendToPoisonQueue(UpdateMessage message, MessagePriority priority)
        {
            try
            {
                string _configkeyPoisonMQ = ConfigurationSettings.AppSettings["StarChef.MSMQ.PoisonQueue"].ToString();

                if (MessageQueue.Exists(_configkeyPoisonMQ))
                {
                    MessageQueue q = new MessageQueue(_configkeyPoisonMQ);
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
                    ExceptionManager.Publish(new Exception("StarChef Message Queue: " + _configkeyPoisonMQ + " does not exist. Please check MSMQ Setup"));
                }
            }
            catch (Exception)
            {
                 ExceptionManager.Publish(new Exception("StarChef: MSMQ Errored while adding a message to the Poison Queue."));
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

	[Serializable]
	public class UpdateMessage
	{
		private int _product_id = 0;
		private int _message_type = 0;
		private int _sub_message_type = 0;
		private int _group_id = 0;
		private string _dbDSN = string.Empty;
        private DateTime _arrived_time = DateTime.MinValue;
        private int _database_id = 0;
        private int _entityTypeId = 0;

		public UpdateMessage()
		{
		}

        public UpdateMessage(int productId, string dbDSN, int action, int databaseId)
		{
			_product_id = productId;
			_group_id = 0;
			_dbDSN = dbDSN;
			_message_type = action;
            _database_id = databaseId;
		}

        public UpdateMessage(int productId, string dbDSN, int action, int databaseId, int entityTypeId = 0)
        {
            _product_id = productId;
            _group_id = 0;
            _dbDSN = dbDSN;
            _message_type = action;
            _database_id = databaseId;
            _entityTypeId = entityTypeId;
        }

        public UpdateMessage(int productId, int groupId, string dbDSN, int action, int databaseId)
		{
			_product_id = productId;
			_group_id = groupId;			
			_dbDSN = dbDSN;
			_message_type = action;
            _database_id = databaseId;
		}
        public UpdateMessage(int productId, int groupId, string dbDSN, int action, int databaseId, int entityTypeId) : this(productId,groupId,dbDSN,action,databaseId)
        {
            _entityTypeId = entityTypeId;
        }

        public int SubAction
		{
			get {return _sub_message_type;}
			set {_sub_message_type = value;}
		}

		public int Action
		{
			get {return _message_type;}
			set {_message_type = value;}
		}

		public int ProductID
		{
			get {return _product_id;}
			set {_product_id = value;}
		}

		public int GroupID
		{
			get {return _group_id;}
			set {_group_id = value;}
		}

		public string DSN
		{
			get {return _dbDSN;}
			set {_dbDSN = value;}
		}

        public DateTime ArrivedTime
        {
            get { return _arrived_time; }
            set { _arrived_time = value; }
        }

        public int DatabaseID
        {
            get { return _database_id; }
            set { _database_id = value; }
        }

        public int EntityTypeId
        {
            get { return _entityTypeId; }
            set { _entityTypeId = value; }
        }

        public override string ToString()
		{
            return "database_id: " + _database_id.ToString() + ", product_id: " + _product_id.ToString() + ", group_id: " + _group_id.ToString() + ", action: " + ((Constants.MessageActionType)_message_type).ToString() + ", sub action: " + _sub_message_type.ToString() + ", entityTypeId: " + _entityTypeId.ToString(); 
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
		private Data.Constants.ReportOutputType _format = Data.Constants.ReportOutputType.NotSet;
		private int _groupFilterId = 0;
		private Data.Constants.GroupFilterType _groupFilterType = Data.Constants.GroupFilterType.NotSet;
		private DateTime _startDate;
		private DateTime _endDate;
		private Data.Constants.ScopeType _scopeFilterID = Data.Constants.ScopeType.NotSet;
		private int _startDay = 0;
		private int _endDay = 0;

		public ReportingMessage()
		{
		}

		public ReportingMessage(string ReportGuid, string ReportName, string parameterXML, string filterXML, string outputFilterXML, string UserDSN, Data.Constants.ReportOutputType Format, int GroupFilterID, Data.Constants.GroupFilterType GroupFilterType, DateTime StartDate, DateTime EndDate, int StartDay, int EndDay, Data.Constants.ScopeType ScopeFilterID)
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

		public Data.Constants.ReportOutputType Format
		{
			get {return _format;}
			set {_format = value;}
		}

		public int GroupFilterID
		{
			get {return _groupFilterId;}
			set {_groupFilterId = value;}
		}

		public Data.Constants.GroupFilterType GroupFilterType
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

		public Data.Constants.ScopeType ScopeFilterID
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

