using System;
using System.Messaging;
using StarChef.Data;


namespace StarChef.MSMQService
{
	/// <summary>
	/// Summary description for MSMQHelper.
	/// </summary>
	public class MSMQHelper
	{
		public MSMQHelper()
		{			
		}

		public static void Send(UpdateMessage msg)
		{
			MSMQManager mqm = new MSMQManager();
			
			if (msg == null) return;

			try
			{
				mqm.mqSend(msg, (MessagePriority)Convert.ToInt32(DbManager.GetSetting("CONFIG_MSMQ_MESSAGE_PRIORITY")));
				mqm.mqDisconnect();
			}
			catch (Exception e)
			{
				mqm.mqDisconnect();
				// don't throw exception while testing - MSMQ not installed as standard
				throw e;
			}
			
		}

		public static void Send(string productId, string groupId, int action, string dbDSN, int databaseId)
		{
			Send(Convert.ToInt32(productId), Convert.ToInt32(groupId), action, dbDSN, databaseId);
		}

		public static void Send(string productId, int action, string dbDSN, int databaseId)
		{			
			Send(Convert.ToInt32(productId), action, dbDSN, databaseId);
		}

        public static void Send(int productId, int groupId, int action, string dbDSN, int databaseId)
		{
			UpdateMessage msg = new UpdateMessage(productId, groupId, dbDSN, action, databaseId);
			Send(msg);
		}

		public static void Send(int productId, int action, string dbDSN, int databaseId)
		{
			UpdateMessage msg = new UpdateMessage(productId, dbDSN, action, databaseId);
			Send(msg);
		}
	}
}
