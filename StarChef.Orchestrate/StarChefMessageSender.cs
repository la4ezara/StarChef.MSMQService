using Fourth.Orchestration.Messaging;
using log4net;
using StarChef.Common;
using System;
using System.Data.SqlClient;

namespace StarChef.Orchestrate
{
    /// <summary>
    /// The main class that wires up and sends the message to orchestration.
    /// </summary>
    public class StarChefMessageSender : IStarChefMessageSender
    {
        /// <summary> The log4net Logger instance. </summary>
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary> The messaging factory to use when creating bus and listener instances. </summary>
        private readonly IMessagingFactory _messagingFactory;
        private readonly IDatabaseManager _databaseManager;
        public StarChefMessageSender(
            IMessagingFactory messagingFactory, 
            IDatabaseManager databaseManager
            )
        {
            _messagingFactory = messagingFactory;
            _databaseManager = databaseManager;
        }

        public bool Send(
            EnumHelper.EntityTypeWrapper entityTypeWrapper,
            string dbConnectionString,
            int entityTypeId,
            int entityId,
            int databaseId,
            DateTime messageArrivedTime,
            int sqlCommandTimeout
            )
        {
            var result = false;

            try
            {
                using (IMessageBus bus = _messagingFactory.CreateMessageBus())
                {
                    // Create an event payload
                    switch(entityTypeWrapper)
                    {
                        case EnumHelper.EntityTypeWrapper.Recipe:
                            var eventPayload = EventFactory.CreateRecipeEvent();

                            result = bus.Publish(eventPayload);

                            LogDatabase(dbConnectionString,
                                        entityTypeId,
                                        entityId,
                                        sqlCommandTimeout,
                                        messageArrivedTime,
                                        result);
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Fatal("StarChef MSMQService Orchestrate failed to send to Orchestration in Send.", ex);
            }

            return result;
        }
        private void LogDatabase(
            string connectionString,
            int entityTypeId,
            int entityId,
            int timeout,
            DateTime msgDateTime,
            bool publishStatus
        )
        {
            Logger.Info("Logging publish status to database");

            _databaseManager.Execute(connectionString,
                                    "sc_insert_orchestration_event_log",
                                    timeout,
                                    new SqlParameter("@entity_type_id", entityTypeId),
                                    new SqlParameter("@entity_id", entityId),
                                    new SqlParameter("@publish_status", publishStatus ? 1 : 0),
                                    new SqlParameter("@date_message_captured", msgDateTime));
        }
    }
}