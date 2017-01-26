﻿using StarChef.Orchestrate.Models;
using Events = Fourth.Orchestration.Model.Menus.Events;
using System.Collections.Generic;
using Google.ProtocolBuffers;
using Fourth.Orchestration.Model.Menus;

namespace StarChef.Orchestrate
{
    public class EventFactory : IEventFactory
    {
        private readonly IIngredientUpdatedSetter _ingredientUpdatedSetter;
        private IRecipeUpdatedSetter _recipeUpdatedSetter;
        private IMenuUpdatedSetter _menuUpdatedSetter;
        private IGroupUpdatedSetter _groupUpdatedSetter;
        private IMealPeriodUpdatedSetter _mealPeriodUpdatedSetter;
        private ISupplierUpdatedSetter _supplierUpdatedSetter;

        public EventFactory(ISupplierUpdatedSetter supplierUpdatedSetter, IMealPeriodUpdatedSetter mealPeriodUpdatedSetter, IGroupUpdatedSetter groupUpdatedSetter, IMenuUpdatedSetter menuUpdatedSetter, IRecipeUpdatedSetter recipeUpdatedSetter, IIngredientUpdatedSetter ingredientUpdatedSetter)
        {
            _supplierUpdatedSetter = supplierUpdatedSetter;
            _mealPeriodUpdatedSetter = mealPeriodUpdatedSetter;
            _groupUpdatedSetter = groupUpdatedSetter;
            _menuUpdatedSetter = menuUpdatedSetter;
            _recipeUpdatedSetter = recipeUpdatedSetter;
            _ingredientUpdatedSetter = ingredientUpdatedSetter;
        }

        protected TBuilder CreateBuilder<TMessage, TBuilder>(Events.ChangeType? changeType = null)
            where TMessage : GeneratedMessage<TMessage, TBuilder>
            where TBuilder : GeneratedBuilder<TMessage, TBuilder>, new()
        {
            dynamic result = null;

            if (typeof(TMessage) == typeof(Events.IngredientUpdated))
                result = Events.IngredientUpdated.CreateBuilder();
            else if (typeof(TMessage) == typeof(Events.RecipeUpdated))
                result = Events.RecipeUpdated.CreateBuilder();
            else if (typeof(TMessage) == typeof(Events.MenuUpdated))
                result = Events.MenuUpdated.CreateBuilder();
            else if (typeof(TMessage) == typeof(Events.GroupUpdated))
                result = Events.GroupUpdated.CreateBuilder();
            else if (typeof(TMessage) == typeof(Events.MealPeriodUpdated))
                result = Events.MealPeriodUpdated.CreateBuilder();
            else if (typeof(TMessage) == typeof(Events.SupplierUpdated))
                result = Events.SupplierUpdated.CreateBuilder();
            else if (typeof(TMessage) == typeof(Events.UserUpdated))
                result = Events.UserUpdated.CreateBuilder();

            if (result != null)
            {
                result.SetSource(Events.SourceSystem.STARCHEF);
                result.SetSequenceNumber(Fourth.Orchestration.Model.SequenceNumbers.GetNext());
                if (changeType.HasValue)
                    result.SetChangeType(changeType.Value);
            }
            return (TBuilder)result;
        }

        #region Updated events

        public static Events.UserUpdated CreateUserEvent(string dbConnectionString, int entityId, int databaseId)
        {
            Customer cust = new Customer(databaseId);
            User u = new User(entityId);
            
            var builder = u.Build(cust, dbConnectionString);

            // Build the immutable data object
            var eventObj = builder.Build();

            return eventObj;
        }

        public static IEnumerable<Events.UserUpdated> CreateUserGroupEvent(string dbConnectionString, int entityId, int databaseId)
        {
            Customer cust = new Customer(databaseId);

            var userGroup = new UserGroup(entityId);

            foreach(var user in userGroup.GetUsersInGroup(dbConnectionString))
            {
                var builder = user.Build(cust, dbConnectionString);

                // Build the immutable data object
                var eventObj = builder.Build();

                yield return eventObj;
            }
        }

        #endregion

        #region Create event builders

        public TMessage CreateDeleteEvent<TMessage, TBuilder>(string connectionString, string entityExternalId, int databaseId)
            where TMessage : GeneratedMessage<TMessage, TBuilder>
            where TBuilder : GeneratedBuilder<TMessage, TBuilder>, new()
        {
            var builder = CreateBuilder<TMessage, TBuilder>(Events.ChangeType.DELETE);
            object builderObj = builder; // builder cannot be cast directly to event builder for specific events

            if (typeof(TBuilder) == typeof(Events.IngredientUpdated.Builder))
                ((Events.IngredientUpdated.Builder)builderObj).SetBuilderForDelete(entityExternalId, databaseId);
            else if (typeof(TBuilder) == typeof(Events.RecipeUpdated.Builder))
                ((Events.RecipeUpdated.Builder)builderObj).SetBuilderForDelete(entityExternalId, databaseId);
            else if (typeof(TBuilder) == typeof(Events.MenuUpdated.Builder))
                ((Events.MenuUpdated.Builder)builderObj).SetBuilderForDelete(entityExternalId, databaseId);
            else if (typeof(TBuilder) == typeof(Events.GroupUpdated.Builder))
                ((Events.GroupUpdated.Builder)builderObj).SetBuilderForDelete(entityExternalId, databaseId);
            else if (typeof(TBuilder) == typeof(Events.MealPeriodUpdated.Builder))
                ((Events.MealPeriodUpdated.Builder)builderObj).SetBuilderForDelete(entityExternalId, databaseId);
            else if (typeof(TBuilder) == typeof(Events.SupplierUpdated.Builder))
                ((Events.RecipeUpdated.Builder)builderObj).SetBuilderForDelete(entityExternalId, databaseId);

            return builder.Build();
        }

        public TMessage CreateUpdateEvent<TMessage, TBuilder>(string connectionString, int entityId, int databaseId)
            where TMessage : GeneratedMessage<TMessage, TBuilder>
            where TBuilder : GeneratedBuilder<TMessage, TBuilder>, new()
        {
            var builder = CreateBuilder<TMessage, TBuilder>(Events.ChangeType.UPDATE);
            object builderObj = builder; // builder cannot be cast directly to event builder for specific events

            if (typeof (TBuilder) == typeof (Events.IngredientUpdated.Builder))
                _ingredientUpdatedSetter.SetBuilder((Events.IngredientUpdated.Builder) builderObj, connectionString, entityId, databaseId);
            else if (typeof (TBuilder) == typeof (Events.RecipeUpdated.Builder))
                _recipeUpdatedSetter.SetBuilder((Events.RecipeUpdated.Builder) builderObj, connectionString, entityId, databaseId);
            else if (typeof(TBuilder) == typeof(Events.MenuUpdated.Builder))
                _menuUpdatedSetter.SetBuilder((Events.MenuUpdated.Builder)builderObj, connectionString, entityId, databaseId);
            else if (typeof(TBuilder) == typeof(Events.GroupUpdated.Builder))
                _groupUpdatedSetter.SetBuilder((Events.GroupUpdated.Builder)builderObj, connectionString, entityId, databaseId);
            else if (typeof(TBuilder) == typeof(Events.MealPeriodUpdated.Builder))
                _mealPeriodUpdatedSetter.SetBuilder((Events.MealPeriodUpdated.Builder)builderObj, connectionString, entityId, databaseId);
            else if (typeof(TBuilder) == typeof(Events.SupplierUpdated.Builder))
                _supplierUpdatedSetter.SetBuilder((Events.SupplierUpdated.Builder)builderObj, connectionString, entityId, databaseId);

            // the builder object is initialized since it was passed to initializes as referenced object
            return builder.Build();
        }

        #endregion
    }
}