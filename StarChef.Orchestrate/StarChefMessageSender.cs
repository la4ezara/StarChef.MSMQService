using Fourth.Orchestration.Messaging;
using log4net;
using StarChef.Common;
using System;
using System.Data.SqlClient;
using Fourth.Orchestration.Model.Customer;
using Fourth.Orchestration.Model.Menus.Events;
using Google.ProtocolBuffers;
using StarChef.Data;

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
            DateTime messageArrivedTime
            )
        {
            var result = false;

            var logged = false;

            try
            {
                using (IMessageBus bus = _messagingFactory.CreateMessageBus())
                {
                    // Create an event payload
                    switch(entityTypeWrapper)
                    {
                        case EnumHelper.EntityTypeWrapper.Recipe:
                            var recipeEventPayload = EventFactory.CreateRecipeEvent(dbConnectionString, entityId, databaseId);
                            result = Publish(bus, recipeEventPayload);
                            break;
                        case EnumHelper.EntityTypeWrapper.MealPeriod:
                            var mealPeriodEventPayload = EventFactory.CreateMealPeriodEvent(dbConnectionString, entityId, databaseId);
                            result = Publish(bus, mealPeriodEventPayload);
                            break;
                        case EnumHelper.EntityTypeWrapper.Group:
                            var groupEventPayload = EventFactory.CreateGroupEvent(dbConnectionString, entityId, databaseId);
                            result = Publish(bus, groupEventPayload);
                            break;
                        case EnumHelper.EntityTypeWrapper.User:
                            var userCommandCreateAccount = CommandFactory.CreateAccountCommand(dbConnectionString, entityId, databaseId);
                            result = Send(bus, userCommandCreateAccount);
                            break;
                        case EnumHelper.EntityTypeWrapper.UserActivated:  
                            var userCommandAccountActivated = CommandFactory.ActivateAccountCommand(entityId, databaseId);
                            result = Send(bus, userCommandAccountActivated);
                            break;
                        case EnumHelper.EntityTypeWrapper.UserDeactivated:  
                            var userCommandAccountDeactivated = CommandFactory.DeactivateAccountCommand(entityId, databaseId);
                            result = Send(bus, userCommandAccountDeactivated);
                            break;
                        case EnumHelper.EntityTypeWrapper.SendUserUpdatedEvent:
                        {
                            var userCreatedEventPayload = EventFactory.CreateUserEvent(dbConnectionString, entityId, databaseId);
                            result = Publish(bus, userCreatedEventPayload);
                            break;
                        }
                        case EnumHelper.EntityTypeWrapper.SendUserUpdatedEventAndCommand:
                        {
                            var userCreatedEventPayload = EventFactory.CreateUserEvent(dbConnectionString, entityId, databaseId);
                            var userCreatedCommandPayload = CommandFactory.UpdateAccountCommand(dbConnectionString, entityId, databaseId);
                            result = Publish(bus, userCreatedEventPayload)
                                     && Send(bus, userCreatedCommandPayload);
                            break;
                        }
                        case EnumHelper.EntityTypeWrapper.UserGroup:
                            var userGroupEventPayload = EventFactory.CreateUserGroupEvent(dbConnectionString, entityId, databaseId);
                            foreach(var user in userGroupEventPayload)
                            {
                                result = Publish(bus, user);
                                LogDatabase(dbConnectionString,
                                                        entityTypeId,
                                                        entityId,
                                                        messageArrivedTime,
                                                        result);
                                logged = true;
                            }
                            break;
                        case EnumHelper.EntityTypeWrapper.Menu:
                            var meuEventPayload = EventFactory.UpdateMenuEvent(dbConnectionString, entityId, databaseId);
                            result = Publish(bus, meuEventPayload);
                            break;
                        case EnumHelper.EntityTypeWrapper.Ingredient:
                            var ingredientEventPayload = EventFactory.UpdateIngredientEvent(dbConnectionString, entityId, databaseId);
                            result = Publish(bus, ingredientEventPayload);
                            break;
                    }
                }

                if(!logged)
                {
                    LogDatabase(dbConnectionString,
                                        entityTypeId,
                                        entityId,
                                        messageArrivedTime,
                                        result);
                }
            }
            catch(Exception ex)
            {
                Logger.Fatal("StarChef MSMQService Orchestrate failed to send to Orchestration in Send.", ex);
            }

            return result;
        }

        private bool Send(IMessageBus bus, IMessage messagePayload)
        {
            var result = bus.Send(messagePayload);
            Logger.InfoFormat("Command '{0}' sent: {1}", messagePayload.GetType().Name, messagePayload.ToJson());
            return result;
        }

        private static bool Publish(IMessageBus bus, IMessage messagePayload)
        {
            var result = bus.Publish(messagePayload);
            Logger.InfoFormat("Event '{0}' published: {1}", messagePayload.GetType().Name, messagePayload.ToJson());
            return result;
        }

        private void LogDatabase(
            string connectionString,
            int entityTypeId,
            int entityId,
            DateTime msgDateTime,
            bool publishStatus
        )
        {
            Logger.Info("Logging publish status to database");

            _databaseManager.Execute(connectionString,
                                    "sc_insert_orchestration_event_log",
                                    new SqlParameter("@entity_type_id", entityTypeId),
                                    new SqlParameter("@entity_id", entityId),
                                    new SqlParameter("@publish_status", publishStatus ? 1 : 0),
                                    new SqlParameter("@date_message_captured", msgDateTime));
        }

        public bool PublishDeleteEvent(int entityId, int entityTypeId, int databaseId, DateTime messageArrivedTime, string dbConnectionString)
        {
            var result = false;
            try
            {
                using (IMessageBus bus = _messagingFactory.CreateMessageBus())
                {
                    var payload = CreateDeleteEventPayload(entityId, entityTypeId, databaseId, dbConnectionString, _databaseManager);
                    result = Publish(bus, payload);

                    LogDatabase(dbConnectionString, entityTypeId, entityId, messageArrivedTime, result);
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal("Failed to publish delete event.", ex);
            }
            return result;
        }

        internal static IMessage CreateDeleteEventPayload(int entityId, int entityTypeId, int databaseId, string dbConnectionString, IDatabaseManager databaseManager)
        {
            IMessage payload = null;
            switch ((Constants.EntityType)entityTypeId)
            {
                case Constants.EntityType.Category:
                {
                    //Fourth.Orchestration.Model.MenuService.Events.
                    //Fourth.Orchestration.Model.Menus.Events.
                }
                    break;
                case Constants.EntityType.Group:
                    payload = EventFactory.CreateGroupDeletedEvent(dbConnectionString, entityId, databaseId);
                    break;
                case Constants.EntityType.PriceBand:
                {
                }
                    break;
                case Constants.EntityType.ProductSet:
                    //dif
                {
                }
                    break;
                case Constants.EntityType.Supplier:
                {
                }
                    break;
                case Constants.EntityType.UserGroup:
                {
                }
                    break;
                case Constants.EntityType.User:
                {
                }
                    break;
                case Constants.EntityType.UserUnit:
                {
                }
                    break;
            }
            return payload;
        }
    }
}