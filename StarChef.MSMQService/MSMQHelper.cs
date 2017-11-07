using System;
using System.Messaging;
using System.Reflection;
using log4net;
using StarChef.Common;

namespace StarChef.MSMQService
{
	/// <summary>
	/// Summary description for MSMQHelper.
	/// </summary>
	public class MSMQHelper
	{
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MSMQHelper()
		{			
		}

		public static void Send(UpdateMessage msg)
		{
		    var db = new DatabaseManager();
			var mqm = new MSMQManager(".\\private$\\starchef_update");
			
			if (msg == null) return;

			try
			{
			    var priority = db.GetSetting(msg.DSN, "CONFIG_MSMQ_MESSAGE_PRIORITY");
				mqm.mqSend(msg, (MessagePriority)Convert.ToInt32(priority));
				mqm.mqDisconnect();
			}
			catch (Exception e)
			{
                _logger.Error(e);

                mqm.mqDisconnect();
				// don't throw exception while testing - MSMQ not installed as standard
				throw e;
			}
			
		}
	}
}
