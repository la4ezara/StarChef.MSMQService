using Fourth.Orchestration.Messaging;
using log4net;
using StarChef.Common;
using System;
using System.Data.SqlClient;
using Google.ProtocolBuffers;
using StarChef.Data;
using UpdateMessage = StarChef.MSMQService.UpdateMessage;
using Events = Fourth.Orchestration.Model.Menus.Events;

namespace StarChef.Orchestrate
{
    /// <summary>
    /// The main class that wires up and sends the message to orchestration.
    /// </summary>
    public class StarChefMessageSender : IStarChefMessageSender
    {
        /// <summary> The log4net Logger instance. </summary>
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary> The messaging factory to use when creating bus and listener instances. </summary>
        private readonly IMessagingFactory _messagingFactory;
        private readonly IDatabaseManager _databaseManager;
        private readonly IEventFactory _eventFactory;

        public StarChefMessageSender(
            IMessagingFactory messagingFactory, 
            IDatabaseManager databaseManager,
            IEventFactory eventFactory
            )
        {
            _eventFactory = eventFactory;
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
                    switch (entityTypeWrapper)
                    {
                        case EnumHelper.EntityTypeWrapper.Recipe:
                            {
                                var payload = _eventFactory.CreateUpdateEvent<Events.RecipeUpdated, Events.RecipeUpdated.Builder>(dbConnectionString, entityId, databaseId);
                                result = Publish(bus, payload);
                            }
                            break;
                        case EnumHelper.EntityTypeWrapper.MealPeriod:
                            {
                                var payload = _eventFactory.CreateUpdateEvent<Events.MealPeriodUpdated, Events.MealPeriodUpdated.Builder>(dbConnectionString, entityId, databaseId);
                                result = Publish(bus, payload);
                            }
                            break;
                        case EnumHelper.EntityTypeWrapper.Group:
                            {
                                var payload = _eventFactory.CreateUpdateEvent<Events.GroupUpdated, Events.GroupUpdated.Builder>(dbConnectionString, entityId, databaseId);
                                result = Publish(bus, payload);
                            }
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
                                var payload = _eventFactory.CreateUpdateEvent<Events.UserUpdated, Events.UserUpdated.Builder>(dbConnectionString, entityId, databaseId);
                                result = Publish(bus, payload);
                            }
                            break;
                        case EnumHelper.EntityTypeWrapper.SendUserUpdatedEventAndCommand:
                            {
                                var userCreatedEventPayload = _eventFactory.CreateUpdateEvent<Events.UserUpdated, Events.UserUpdated.Builder>(dbConnectionString, entityId, databaseId);
                                var userCreatedCommandPayload = CommandFactory.UpdateAccountCommand(dbConnectionString, entityId, databaseId);
                                result = Publish(bus, userCreatedEventPayload)
                                         && Send(bus, userCreatedCommandPayload);
                                break;
                            }
                        case EnumHelper.EntityTypeWrapper.UserGroup:
                            {
                                var userIds = _databaseManager.GetUsersInGroup(dbConnectionString, entityId);
                                foreach (var userId in userIds)
                                {
                                    var payload = _eventFactory.CreateUpdateEvent<Events.UserUpdated, Events.UserUpdated.Builder>(dbConnectionString, userId, databaseId);
                                    result = Publish(bus, payload);
                                    LogDatabase(dbConnectionString, entityTypeId, entityId, messageArrivedTime, result);
                                    logged = true;
                                }
                            }
                            break;
                        case EnumHelper.EntityTypeWrapper.Menu:
                            {
                                var payload = _eventFactory.CreateUpdateEvent<Events.MenuUpdated, Events.MenuUpdated.Builder>(dbConnectionString, entityId, databaseId);
                                result = Publish(bus, payload);
                            }
                            break;
                        case EnumHelper.EntityTypeWrapper.Ingredient:
                            {
                                var payload = _eventFactory.CreateUpdateEvent<Events.IngredientUpdated, Events.IngredientUpdated.Builder>(dbConnectionString, entityId, databaseId);
                                result = Publish(bus, payload);
                            }
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
                _logger.Fatal("StarChef MSMQService Orchestrate failed to send to Orchestration in Send.", ex);
            }

            return result;
        }

        private bool Send(IMessageBus bus, IMessage messagePayload)
        {
            var result = bus.Send(messagePayload);
            _logger.InfoFormat("Command '{0}' sent: {1}", messagePayload.GetType().Name, messagePayload.ToJson());
            return result;
        }

        private static bool Publish(IMessageBus bus, IMessage messagePayload)
        {
            var result = bus.Publish(messagePayload);
            _logger.InfoFormat("Event '{0}' published: {1}", messagePayload.GetType().Name, messagePayload.ToJson());
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
            _logger.Info("Logging publish status to database");

            _databaseManager.Execute(connectionString,
                                    "sc_insert_orchestration_event_log",
                                    new SqlParameter("@entity_type_id", entityTypeId),
                                    new SqlParameter("@entity_id", entityId),
                                    new SqlParameter("@publish_status", publishStatus ? 1 : 0),
                                    new SqlParameter("@date_message_captured", msgDateTime));
        }

        public bool PublishDeleteEvent(UpdateMessage message)
        {
            var result = false;
            try
            {
                using (IMessageBus bus = _messagingFactory.CreateMessageBus())
                {
                    var payload = CreateDeleteEventPayload(message.ExternalId, message.EntityTypeId, message.DatabaseID, message.DSN, _databaseManager);
                    if (payload != null)
                    {
                        result = Publish(bus, payload);
                        LogDatabase(message.DSN, message.EntityTypeId, message.ProductID, message.ArrivedTime, result);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Fatal("Failed to publish delete event.", ex);
            }
            return result;
        }

        internal IMessage CreateDeleteEventPayload(string entityExternalId, int entityTypeId, int databaseId, string dbConnectionString, IDatabaseManager databaseManager)
        {
            IMessage payload = null;
            switch ((Constants.EntityType)entityTypeId)
            {
                case Constants.EntityType.Ingredient:
                    {
                        payload = _eventFactory.CreateDeleteEvent<Events.IngredientUpdated, Events.IngredientUpdated.Builder>(dbConnectionString, entityExternalId, databaseId);
                    }
                    break;
                case Constants.EntityType.Dish:
                    {
                        payload = _eventFactory.CreateDeleteEvent<Events.RecipeUpdated, Events.RecipeUpdated.Builder>(dbConnectionString, entityExternalId, databaseId);
                    }
                    break;
                case Constants.EntityType.Menu:
                    {
                        payload = _eventFactory.CreateDeleteEvent<Events.MenuUpdated, Events.MenuUpdated.Builder>(dbConnectionString, entityExternalId, databaseId);
                    }
                    break;
                case Constants.EntityType.MenuCycle:
                    {
                        _logger.InfoFormat("MenuCycle deletion message is not sent because it's not supported in Fourth.Orchestration.Model.Menus.Events.");
                    }
                    break;
                case Constants.EntityType.Category:
                    {
                        /*
                         * Categories are updated with Update message for ingredient
                         */
                        _logger.InfoFormat("Category deletion does not send delete message.");
                    }
                    break;
                case Constants.EntityType.Group:
                    payload = _eventFactory.CreateDeleteEvent<Events.GroupUpdated, Events.GroupUpdated.Builder>(dbConnectionString, entityExternalId, databaseId);
                    break;
                case Constants.EntityType.PriceBand:
                    {
                        _logger.InfoFormat("PriceBand deletion message is not sent because it's not supported in Fourth.Orchestration.Model.Menus.Events.");
                    }
                    break;
                case Constants.EntityType.ProductSet:
                    {
                        _logger.InfoFormat("ProductSet deletion message is not sent because it's not supported in Fourth.Orchestration.Model.Menus.Events.");
                    }
                    break;
                case Constants.EntityType.Supplier:
                    {
                        payload = _eventFactory.CreateDeleteEvent<Events.SupplierUpdated, Events.SupplierUpdated.Builder>(dbConnectionString, entityExternalId, databaseId);
                    }
                    break;
                case Constants.EntityType.UserGroup:
                    {
                        /*
                         * User Groups are updated with Update message for users
                         */
                        _logger.InfoFormat("UserGroup deletion does not send delete message.");
                    }
                    break;
                case Constants.EntityType.User:
                    {
                        payload = _eventFactory.CreateDeleteEvent<Events.UserUpdated, Events.UserUpdated.Builder>(dbConnectionString, entityExternalId, databaseId);
                    }
                    break;
                case Constants.EntityType.UserUnit:
                    {
                        _logger.InfoFormat("UserUnit deletion message is not sent because it's not supported in Fourth.Orchestration.Model.Menus.Events.");
                    }
                    break;
            }
            return payload;
        }
    }
}