using System;
using System.Messaging;
using System.Reflection;
using Fourth.StarChef.Invariables;
using Fourth.StarChef.Invariables.Interfaces;
using log4net;
using Newtonsoft.Json;

namespace Fourth.Import.Common.Messaging
{
    /// <summary>
    ///     Summary description for MSMQHelper
    /// </summary>
    public class MsmqHelper
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static MsmqHelper()
        {
            MsmqSenderFactory = () => new MSMQManager();
            MessagingSettingsFactory = connectionString => new MessagingSettings(connectionString);
        }

        internal static Func<IMSMQManager> MsmqSenderFactory { get; set; }
        internal static Func<string, IMessagingSettings> MessagingSettingsFactory { get; set; }

        private static void Send(UpdateMessage msg, MessagePriority messagePriority, string queueName)
        {
            if (msg == null) return;
            var mqm = MsmqSenderFactory();

            try
            {
                mqm.MQName = queueName;

                _logger.DebugFormat("Message sent: {0}", JsonConvert.SerializeObject(msg));
                
                mqm.mqSend(msg, messagePriority);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
            finally { mqm.mqDisconnect(); }
        }

        public static void Send(int entityId, int entityTypeId, int databaseId, string dbDsn, Fourth.StarChef.Invariables.Constants.MessageActionType messageAction, Fourth.StarChef.Invariables.Constants.MessageSubActionType? messageSubAction = null, string extendedProperties = null)
        {
            var messagingSettings = MessagingSettingsFactory(dbDsn);

            var msg = new UpdateMessage(entityId, entityTypeId: entityTypeId, action: (int)messageAction, dbDSN: dbDsn, databaseId: databaseId);
            if (messageSubAction.HasValue)
                msg.SubAction = (int)messageSubAction.Value;
            if (!string.IsNullOrEmpty(extendedProperties))
                msg.ExtendedProperties = extendedProperties;

            var queueName = messagingSettings.GetQueueName();
            Send(msg, messagingSettings.GetMessagePriority(), queueName);

            _logger.DebugFormat("MSMQ message sent: {0}", JsonConvert.SerializeObject(msg));
        }
    }
}