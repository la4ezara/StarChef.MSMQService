using System.Linq;
using Fourth.Orchestration.Messaging;
using log4net;
using StarChef.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Fourth.Orchestration.Model.Menus;
using Google.ProtocolBuffers;

#region Orchestration types

using IngredientUpdated = Fourth.Orchestration.Model.Menus.Events.IngredientUpdated;
using RecipeUpdated = Fourth.Orchestration.Model.Menus.Events.RecipeUpdated;
using GroupUpdated = Fourth.Orchestration.Model.Menus.Events.GroupUpdated;
using MenuUpdated = Fourth.Orchestration.Model.Menus.Events.MenuUpdated;
using MealPeriodUpdated = Fourth.Orchestration.Model.Menus.Events.MealPeriodUpdated;
using SupplierUpdated = Fourth.Orchestration.Model.Menus.Events.SupplierUpdated;
using UserUpdated = Fourth.Orchestration.Model.Menus.Events.UserUpdated;
using SetUpdated = Fourth.Orchestration.Model.Menus.Events.SetUpdated;
using RecepiNutritionUpdated = Fourth.Orchestration.Model.Menus.Events.RecipeNutritionUpdated;

using IngredientUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.IngredientUpdated.Builder;
using RecipeUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.RecipeUpdated.Builder;
using GroupUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.GroupUpdated.Builder;
using MenuUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.MenuUpdated.Builder;
using MealPeriodUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.MealPeriodUpdated.Builder;
using SupplierUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.SupplierUpdated.Builder;
using UserUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.UserUpdated.Builder;
using DeactivateAccount = Fourth.Orchestration.Model.People.Commands.DeactivateAccount;
using SetUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.SetUpdated.Builder;
using RecepiNutritionUpdateBuilder = Fourth.Orchestration.Model.Menus.Events.RecipeNutritionUpdated.Builder;

using CreateAccountBuilder = Fourth.Orchestration.Model.People.Commands.CreateAccount.Builder;
using UpdateAccountBuilder = Fourth.Orchestration.Model.People.Commands.UpdateAccount.Builder;
using ActivateAccountBuilder = Fourth.Orchestration.Model.People.Commands.ActivateAccount.Builder;
using DeactivateAccountBuilder = Fourth.Orchestration.Model.People.Commands.DeactivateAccount.Builder;
using Fourth.StarChef.Invariables;

#endregion

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
        private readonly ICommandFactory _commandFactory;

        public StarChefMessageSender(
            IMessagingFactory messagingFactory,
            IDatabaseManager databaseManager,
            IEventFactory eventFactory,
            ICommandFactory commandFactory)
        {
            _eventFactory = eventFactory;
            _messagingFactory = messagingFactory;
            _databaseManager = databaseManager;
            _commandFactory = commandFactory;
        }

        private bool Send(
            EnumHelper.EntityTypeWrapper entityTypeWrapper,
            string dbConnectionString,
            int entityTypeId,
            int entityId,
            int databaseId,
            DateTime messageArrivedTime
            )
        {
            return Send(entityTypeWrapper, dbConnectionString, entityTypeId, entityTypeId, string.Empty, databaseId, messageArrivedTime);
        }

        public IList<KeyValuePair<Tuple<int, int>, bool>> Send(
            EnumHelper.EntityTypeWrapper entityTypeWrapper,
            string dbConnectionString,
            int entityTypeId,
            List<int> entityIds,
            string entityExternalId,
            int databaseId,
            DateTime messageArrivedTime
        )
        {
            var result = new List<KeyValuePair<Tuple<int, int>, bool>>();

            Parallel.ForEach(entityIds, entityId =>
            {
                var keyValuePaid = new KeyValuePair<Tuple<int,int>, bool>(Tuple.Create(entityId, entityTypeId), Send(entityTypeWrapper, dbConnectionString, entityTypeId, entityId, entityExternalId, databaseId, messageArrivedTime));
                result.Add(keyValuePaid);
            });

            return result;
        }
                
        public bool Send(
            EnumHelper.EntityTypeWrapper entityTypeWrapper,
            string dbConnectionString,
            int entityTypeId,
            int entityId,
            string entityExternalId,
            int databaseId,
            DateTime messageArrivedTime
            )
        {
            var result = false;

            var logged = false;

            messageArrivedTime = TimeZoneInfo.ConvertTimeToUtc(messageArrivedTime);

            try
            {
                var isSsoEnabled = _databaseManager.IsSsoEnabled(dbConnectionString);

                if (_databaseManager.IsPublishEnabled(dbConnectionString, entityTypeId))
                {
                    using (IMessageBus bus = _messagingFactory.CreateMessageBus())
                    {
                        // Create an event payload
                        switch (entityTypeWrapper)
                        {
                            case EnumHelper.EntityTypeWrapper.Recipe:
                                {
                                    _logger.Debug("enter createEventUpdate");
                                    var payload = _eventFactory.CreateUpdateEvent<RecipeUpdated, RecipeUpdatedBuilder>(dbConnectionString, entityId, databaseId);
                                    _logger.Debug("exit createEventUpdate");

                                    var isSetOrchestrationSentDate = _databaseManager.IsSetOrchestrationSentDate(dbConnectionString, entityId);

                                    if (isSetOrchestrationSentDate || payload.ChangeType == Events.ChangeType.UPDATE)
                                    {
                                        result = Publish(bus, payload);

                                        if (result)
                                        {
                                            _databaseManager.UpdateOrchestrationSentDate(dbConnectionString, entityId);
                                        }
                                    }
                                    else
                                    {
                                        result = true;
                                    }

                                    _logger.Debug("exit publish recipe");
                                }
                                break;
                            case EnumHelper.EntityTypeWrapper.MealPeriod:
                                {
                                    var payload = _eventFactory.CreateUpdateEvent<MealPeriodUpdated, MealPeriodUpdatedBuilder>(dbConnectionString, entityId, databaseId);
                                    result = Publish(bus, payload);
                                }
                                break;
                            case EnumHelper.EntityTypeWrapper.Group:
                                {
                                    var payload = _eventFactory.CreateUpdateEvent<GroupUpdated, GroupUpdatedBuilder>(dbConnectionString, entityId, databaseId);
                                    result = Publish(bus, payload);
                                }
                                break;
                            case EnumHelper.EntityTypeWrapper.User:
                                {
                                    if (isSsoEnabled)
                                    {
                                        IMessage messagePayload;

                                        if (string.IsNullOrEmpty(entityExternalId))
                                        {
                                            messagePayload = CommandFactory.CreateAccountCommand(dbConnectionString, entityId, databaseId);
                                        }
                                        else
                                        {
                                            messagePayload = CommandFactory.UpdateAccountCommand(dbConnectionString, entityId, databaseId);
                                        }

                                        result = Send(bus, messagePayload);
                                    }
                                    else {
                                        _logger.Warn("SSO not enabled");
                                        result = true;
                                    }
                                }
                                break;
                            case EnumHelper.EntityTypeWrapper.UserActivated:
                                {
                                    if (isSsoEnabled)
                                    {
                                        var userCommandAccountActivated = CommandFactory.ActivateAccountCommand(entityId, databaseId);
                                        result = Send(bus, userCommandAccountActivated);
                                    }
                                    else
                                    {
                                        _logger.Warn("SSO not enabled");
                                        result = true;
                                    }
                                }
                                break;
                            case EnumHelper.EntityTypeWrapper.SendUserUpdatedEvent:
                                {
                                    var payload = _eventFactory.CreateUpdateEvent<UserUpdated, UserUpdatedBuilder>(dbConnectionString, entityId, databaseId);
                                    result = Publish(bus, payload);
                                }
                                break;
                            case EnumHelper.EntityTypeWrapper.SendUserUpdatedEventAndCommand:
                                {
                                    var userCreatedEventPayload = _eventFactory.CreateUpdateEvent<UserUpdated, UserUpdatedBuilder>(dbConnectionString, entityId, databaseId);

                                    result = Publish(bus, userCreatedEventPayload);
                                    if (isSsoEnabled)
                                    {
                                        IMessage messagePayload;

                                        if (string.IsNullOrEmpty(entityExternalId))
                                            messagePayload = CommandFactory.CreateAccountCommand(dbConnectionString, entityId, databaseId);
                                        else
                                            messagePayload = CommandFactory.UpdateAccountCommand(dbConnectionString, entityId, databaseId);

                                        result = result && Send(bus, messagePayload);
                                    }
                                    break;
                                }
                            case EnumHelper.EntityTypeWrapper.UserGroup:
                                {
                                    var userIds = _databaseManager.GetUsersInGroup(dbConnectionString, entityId);
                                    if (userIds != null && userIds.Any())
                                    {
                                        foreach (var userId in userIds)
                                        {
                                            var payload = _eventFactory.CreateUpdateEvent<UserUpdated, UserUpdatedBuilder>(dbConnectionString, userId, databaseId);
                                            result = Publish(bus, payload);
                                            LogDatabase(dbConnectionString, entityTypeId, entityId, messageArrivedTime, result);
                                            logged = true;
                                        }
                                    }
                                    else {
                                        result = true;
                                        _logger.Warn($"No users for usergroupId {entityId}");
                                    }
                                }
                                break;
                            case EnumHelper.EntityTypeWrapper.Menu:
                                {
                                    var payload = _eventFactory.CreateUpdateEvent<MenuUpdated, MenuUpdatedBuilder>(dbConnectionString, entityId, databaseId);
                                    result = Publish(bus, payload);
                                }
                                break;
                            case EnumHelper.EntityTypeWrapper.Ingredient:
                                {
                                    var payload = _eventFactory.CreateUpdateEvent<IngredientUpdated, IngredientUpdatedBuilder>(dbConnectionString, entityId, databaseId);

                                    var isSetOrchestrationSentDate = _databaseManager.IsSetOrchestrationSentDate(dbConnectionString, entityId);

                                    if (isSetOrchestrationSentDate || payload.ChangeType == Events.ChangeType.UPDATE)
                                    {
                                        result = Publish(bus, payload);

                                        if (result)
                                        {
                                            _databaseManager.UpdateOrchestrationSentDate(dbConnectionString, entityId);
                                        }
                                    }
                                    else
                                    {
                                        result = true;
                                    }
                                }
                                break;
                            case EnumHelper.EntityTypeWrapper.SendSupplierUpdatedEvent:
                                {
                                    var payload = _eventFactory.CreateUpdateEvent<SupplierUpdated, SupplierUpdatedBuilder>(dbConnectionString, entityId, databaseId);
                                    result = Publish(bus, payload);
                                }
                                break;
                            case EnumHelper.EntityTypeWrapper.ProductSet:
                                {
                                    var payload = _eventFactory.CreateUpdateEvent<SetUpdated, SetUpdatedBuilder>(dbConnectionString, entityId, databaseId);
                                    result = Publish(bus, payload);
                                }
                                break;
                            case EnumHelper.EntityTypeWrapper.ProductNutrition:
                                {
                                    var payload = _eventFactory.CreateUpdateEvent<RecepiNutritionUpdated, RecepiNutritionUpdateBuilder>(dbConnectionString, entityId, databaseId);
                                    result = Publish(bus, payload);
                                }
                                break;
                            default:
                                throw new NotSupportedException();
                        }
                    }
                }
                
                if (!logged)
                {
                    _logger.Debug("enter LogDatabase");
                    LogDatabase(dbConnectionString,
                                        entityTypeId,
                                        entityId,
                                        messageArrivedTime,
                                        result);
                    _logger.Debug("exit LogDatabase");
                }
            }
            catch (Exception ex)
            {
                _logger.Fatal("StarChef MSMQService Orchestrate failed to send to Orchestration in Send.", ex);
            }
            _logger.Debug("Finish Send");
            return result;
        }

        private bool Send(IMessageBus bus, IMessage messagePayload)
        {
            var result = bus.Send(messagePayload);
            _logger.DebugFormat("Command '{0}' sent: {1}", messagePayload.GetType().Name, messagePayload.ToJson());
            return result;
        }

        private static bool Publish(IMessageBus bus, IMessage messagePayload)
        {
            var result = bus.Publish(messagePayload);
            _logger.DebugFormat("Event '{0}' published: {1}", messagePayload.GetType().Name, messagePayload.ToJson());
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
                using (var bus = _messagingFactory.CreateMessageBus())
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
                        payload = _eventFactory.CreateDeleteEvent<IngredientUpdated, IngredientUpdatedBuilder>(dbConnectionString, entityExternalId, databaseId);
                    }
                    break;
                case Constants.EntityType.Dish:
                    {
                        payload = _eventFactory.CreateDeleteEvent<RecipeUpdated, RecipeUpdatedBuilder>(dbConnectionString, entityExternalId, databaseId);
                    }
                    break;
                case Constants.EntityType.Menu:
                    {
                        payload = _eventFactory.CreateDeleteEvent<MenuUpdated, MenuUpdatedBuilder>(dbConnectionString, entityExternalId, databaseId);
                    }
                    break;
                case Constants.EntityType.MenuCycle:
                    {
                        _logger.DebugFormat("MenuCycle deletion message is not sent because it's not supported in Fourth.Orchestration.Model.Menus.Events.");
                    }
                    break;
                case Constants.EntityType.Category:
                    {
                        /*
                         * Categories are updated with Update message for ingredient
                         */
                        _logger.DebugFormat("Category deletion does not send delete message.");
                    }
                    break;
                case Constants.EntityType.Group:
                    payload = _eventFactory.CreateDeleteEvent<GroupUpdated, GroupUpdatedBuilder>(dbConnectionString, entityExternalId, databaseId);
                    break;
                case Constants.EntityType.PriceBand:
                    {
                        _logger.DebugFormat("PriceBand deletion message is not sent because it's not supported in Fourth.Orchestration.Model.Menus.Events.");
                    }
                    break;
                case Constants.EntityType.ProductSet:
                    {
                        payload = _eventFactory.CreateDeleteEvent<SetUpdated, SetUpdatedBuilder>(dbConnectionString, entityExternalId, databaseId);
                    }
                    break;
                case Constants.EntityType.Supplier:
                    {
                        payload = _eventFactory.CreateDeleteEvent<SupplierUpdated, SupplierUpdatedBuilder>(dbConnectionString, entityExternalId, databaseId);
                    }
                    break;
                case Constants.EntityType.UserGroup:
                    {
                        /*
                         * User Groups are updated with Update message for users
                         */
                        _logger.DebugFormat("UserGroup deletion does not send delete message.");
                    }
                    break;
                case Constants.EntityType.User:
                    {
                        payload = _eventFactory.CreateDeleteEvent<UserUpdated, UserUpdatedBuilder>(dbConnectionString, entityExternalId, databaseId);
                    }
                    break;
                case Constants.EntityType.UserUnit:
                    {
                        _logger.DebugFormat("UserUnit deletion message is not sent because it's not supported in Fourth.Orchestration.Model.Menus.Events.");
                    }
                    break;
            }
            return payload;
        }

        public bool PublishCommand(UpdateMessage message)
        {
            var result = false;
            try
            {
                if (IsShouldBeSend(message))
                {
                    using (var bus = _messagingFactory.CreateMessageBus())
                    {
                        var payload = CreateCommandPayload(message);
                        if (payload != null)
                        {
                            result = Send(bus, payload);
                            LogDatabase(message.DSN, message.EntityTypeId, message.ProductID, message.ArrivedTime, result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Fatal("Failed to publish delete event.", ex);
            }
            return result;
        }

        internal virtual bool IsShouldBeSend(UpdateMessage message)
        {
            var messageActionType = (Constants.MessageActionType)message.Action;
            return _databaseManager.IsPublishEnabled(message.DSN, message.EntityTypeId)
                    && (!IsSsoRelatedAction(messageActionType) || _databaseManager.IsSsoEnabled(message.DSN));
        }

        internal virtual bool IsSsoRelatedAction(Constants.MessageActionType messageActionType)
        {
            Constants.MessageActionType[] ssoRelatedActionTypes = new[]
            {
                Constants.MessageActionType.UserCreated,
                Constants.MessageActionType.UserUpdated,
                Constants.MessageActionType.UserActivated,
                Constants.MessageActionType.UserDeActivated
            };
            return ssoRelatedActionTypes.Contains(messageActionType);
        }

        private IMessage CreateCommandPayload(UpdateMessage message)
        {
            var messageActionType = (Constants.MessageActionType)message.Action;
            switch (messageActionType)
            {
                case Constants.MessageActionType.UserDeActivated:
                    return _commandFactory.CreateCommand<DeactivateAccount, DeactivateAccountBuilder>(message);
                default:
                    throw new NotSupportedException(string.Format("Action type {0} is not supported by commands.", messageActionType));
            }
        }
    }
}