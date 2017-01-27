using StarChef.Orchestrate.Models;
using Events = Fourth.Orchestration.Model.Menus.Events;
using System.Collections.Generic;
using Google.ProtocolBuffers;

namespace StarChef.Orchestrate
{
    public class EventFactory : IEventFactory
    {
        private readonly IEventSetter<Events.IngredientUpdated.Builder> _ingredientUpdatedSetter;
        private readonly IEventSetter<Events.RecipeUpdated.Builder> _recipeUpdatedSetter;
        private readonly IEventSetter<Events.MenuUpdated.Builder> _menuUpdatedSetter;
        private readonly IEventSetter<Events.GroupUpdated.Builder> _groupUpdatedSetter;
        private readonly IEventSetter<Events.MealPeriodUpdated.Builder> _mealPeriodUpdatedSetter;
        private readonly IEventSetter<Events.SupplierUpdated.Builder> _supplierUpdatedSetter;
        private readonly IEventSetter<Events.UserUpdated.Builder> _userUpdatedSetter;

        public EventFactory(
            IEventSetter<Events.IngredientUpdated.Builder> ingredientUpdatedSetter,
            IEventSetter<Events.RecipeUpdated.Builder> recipeUpdatedSetter, 
            IEventSetter<Events.GroupUpdated.Builder> groupUpdatedSetter,
            IEventSetter<Events.MenuUpdated.Builder> menuUpdatedSetter,
            IEventSetter<Events.MealPeriodUpdated.Builder> mealPeriodUpdatedSetter,
            IEventSetter<Events.SupplierUpdated.Builder> supplierUpdatedSetter,
            IEventSetter<Events.UserUpdated.Builder> userUpdatedSetter)
        {
            _ingredientUpdatedSetter = ingredientUpdatedSetter;
            _recipeUpdatedSetter = recipeUpdatedSetter;
            _groupUpdatedSetter = groupUpdatedSetter;
            _menuUpdatedSetter = menuUpdatedSetter;
            _mealPeriodUpdatedSetter = mealPeriodUpdatedSetter;
            _supplierUpdatedSetter = supplierUpdatedSetter;
            _userUpdatedSetter = userUpdatedSetter;
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
            else if (typeof(TBuilder) == typeof(Events.UserUpdated.Builder))
                ((Events.UserUpdated.Builder)builderObj).SetBuilderForDelete(entityExternalId, databaseId);

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
            else if (typeof(TBuilder) == typeof(Events.UserUpdated.Builder))
                _userUpdatedSetter.SetBuilder((Events.UserUpdated.Builder)builderObj, connectionString, entityId, databaseId);

            // the builder object is initialized since it was passed to initializes as referenced object
            return builder.Build();
        }

        #endregion
    }
}