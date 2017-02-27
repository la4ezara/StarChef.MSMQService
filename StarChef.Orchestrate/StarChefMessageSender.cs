﻿using Fourth.Orchestration.Messaging;
using log4net;
using StarChef.Common;
using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using Google.ProtocolBuffers;
using StarChef.Data;
using UpdateMessage = StarChef.MSMQService.UpdateMessage;
using StarChef.MSMQService;

#region Orchestration types

using SourceSystem = Fourth.Orchestration.Model.Menus.Events.SourceSystem;
using ChangeType = Fourth.Orchestration.Model.Menus.Events.ChangeType;

using IngredientUpdated = Fourth.Orchestration.Model.Menus.Events.IngredientUpdated;
using RecipeUpdated = Fourth.Orchestration.Model.Menus.Events.RecipeUpdated;
using GroupUpdated = Fourth.Orchestration.Model.Menus.Events.GroupUpdated;
using MenuUpdated = Fourth.Orchestration.Model.Menus.Events.MenuUpdated;
using MealPeriodUpdated = Fourth.Orchestration.Model.Menus.Events.MealPeriodUpdated;
using SupplierUpdated = Fourth.Orchestration.Model.Menus.Events.SupplierUpdated;
using UserUpdated = Fourth.Orchestration.Model.Menus.Events.UserUpdated;

using IngredientUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.IngredientUpdated.Builder;
using RecipeUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.RecipeUpdated.Builder;
using GroupUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.GroupUpdated.Builder;
using MenuUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.MenuUpdated.Builder;
using MealPeriodUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.MealPeriodUpdated.Builder;
using SupplierUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.SupplierUpdated.Builder;
using UserUpdatedBuilder = Fourth.Orchestration.Model.Menus.Events.UserUpdated.Builder;

using CreateAccount = Fourth.Orchestration.Model.People.Commands.CreateAccount;
using UpdateAccount = Fourth.Orchestration.Model.People.Commands.UpdateAccount;
using ActivateAccount = Fourth.Orchestration.Model.People.Commands.ActivateAccount;
using DeactivateAccount = Fourth.Orchestration.Model.People.Commands.DeactivateAccount;

using CreateAccountBuilder = Fourth.Orchestration.Model.People.Commands.CreateAccount.Builder;
using UpdateAccountBuilder = Fourth.Orchestration.Model.People.Commands.UpdateAccount.Builder;
using ActivateAccountBuilder = Fourth.Orchestration.Model.People.Commands.ActivateAccount.Builder;
using DeactivateAccountBuilder = Fourth.Orchestration.Model.People.Commands.DeactivateAccount.Builder;

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
                                var payload = _eventFactory.CreateUpdateEvent<RecipeUpdated, RecipeUpdatedBuilder>(dbConnectionString, entityId, databaseId);
                                result = Publish(bus, payload);
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
                            var userCommandCreateAccount = CommandFactory.CreateAccountCommand(dbConnectionString, entityId, databaseId);
                            result = Send(bus, userCommandCreateAccount);
                            break;
                        case EnumHelper.EntityTypeWrapper.UserActivated:
                            var userCommandAccountActivated = CommandFactory.ActivateAccountCommand(entityId, databaseId);
                            result = Send(bus, userCommandAccountActivated);
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
                                    var payload = _eventFactory.CreateUpdateEvent<UserUpdated, UserUpdatedBuilder>(dbConnectionString, userId, databaseId);
                                    result = Publish(bus, payload);
                                    LogDatabase(dbConnectionString, entityTypeId, entityId, messageArrivedTime, result);
                                    logged = true;
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
                                result = Publish(bus, payload);
                            }
                            break;
                        case EnumHelper.EntityTypeWrapper.SendSupplierUpdatedEvent:
                            {
                                var payload = _eventFactory.CreateUpdateEvent<SupplierUpdated, SupplierUpdatedBuilder>(dbConnectionString, entityId, databaseId);
                                result = Publish(bus, payload);
                            }
                            break;
                        default:
                            throw new NotSupportedException();
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

        public bool PublishUpdateEvent(UpdateMessage message)
        {
            var result = false;
            var connectionString = message.DSN;
            var entityTypeId = message.EntityTypeId;

            try
            {
                if (_databaseManager.IsPublishEnabled(connectionString, entityTypeId))
                {
                    using (var bus = _messagingFactory.CreateMessageBus())
                    {
                        var payload = CreateUpdateEvent(message);
                        if (payload != null)
                        {
                            result = Publish(bus, payload);
                            LogDatabase(connectionString, entityTypeId, message.ProductID, message.ArrivedTime, result);
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
                    payload = _eventFactory.CreateDeleteEvent<GroupUpdated, GroupUpdatedBuilder>(dbConnectionString, entityExternalId, databaseId);
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
                        payload = _eventFactory.CreateDeleteEvent<SupplierUpdated, SupplierUpdatedBuilder>(dbConnectionString, entityExternalId, databaseId);
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
                        payload = _eventFactory.CreateDeleteEvent<UserUpdated, UserUpdatedBuilder>(dbConnectionString, entityExternalId, databaseId);
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

        public bool PublishCommand(UpdateMessage message)
        {
            var result = false;
            var connectionString = message.DSN;
            var entityTypeId = message.EntityTypeId;

            try
            {
                if (_databaseManager.IsPublishEnabled(connectionString, entityTypeId))
                {
                    using (var bus = _messagingFactory.CreateMessageBus())
                    {
                        var payload = CreateCommandPayload(message);
                        if (payload != null)
                        {
                            result = Send(bus, payload);
                            LogDatabase(connectionString, entityTypeId, message.ProductID, message.ArrivedTime, result);
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

        public IMessage CreateUpdateEvent(UpdateMessage message)
        {
            var messageActionType = (Constants.MessageActionType)message.Action;
            switch (messageActionType)
            {
                case Constants.MessageActionType.EntityUpdated:
                   
                    return _eventFactory.CreateUpdateEvent<SupplierUpdated, SupplierUpdatedBuilder>(message.DSN, message.ProductID, message.DatabaseID);
                default:
                    throw new NotSupportedException(string.Format("Action type {0} is not supported by commands.", messageActionType));
            }
        }
    }
}