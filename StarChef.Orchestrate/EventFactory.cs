using Events = Fourth.Orchestration.Model.Menus.Events;
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
        private readonly IEventSetter<Events.SetUpdated.Builder> _setUpdatedSetter;

        public EventFactory(
            IEventSetter<Events.IngredientUpdated.Builder> ingredientUpdatedSetter,
            IEventSetter<Events.RecipeUpdated.Builder> recipeUpdatedSetter, 
            IEventSetter<Events.GroupUpdated.Builder> groupUpdatedSetter,
            IEventSetter<Events.MenuUpdated.Builder> menuUpdatedSetter,
            IEventSetter<Events.MealPeriodUpdated.Builder> mealPeriodUpdatedSetter,
            IEventSetter<Events.SupplierUpdated.Builder> supplierUpdatedSetter,
            IEventSetter<Events.UserUpdated.Builder> userUpdatedSetter,
            IEventSetter<Events.SetUpdated.Builder> setUpdatedSetter)
        {
            _ingredientUpdatedSetter = ingredientUpdatedSetter;
            _recipeUpdatedSetter = recipeUpdatedSetter;
            _groupUpdatedSetter = groupUpdatedSetter;
            _menuUpdatedSetter = menuUpdatedSetter;
            _mealPeriodUpdatedSetter = mealPeriodUpdatedSetter;
            _supplierUpdatedSetter = supplierUpdatedSetter;
            _userUpdatedSetter = userUpdatedSetter;
            _setUpdatedSetter = setUpdatedSetter;
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
            else if (typeof(TMessage) == typeof(Events.SetUpdated))
                result = Events.SetUpdated.CreateBuilder();

            if (result != null)
            {
                if (result.GetType().GetMethod("SetSource") != null)
                {
                    result.SetSource(Events.SourceSystem.STARCHEF);
                }
                result.SetSequenceNumber(Fourth.Orchestration.Model.SequenceNumbers.GetNext());
                if (changeType.HasValue && result.GetType().GetMethod("SetChangeType") != null)
                { 
                    result.SetChangeType(changeType.Value);
                }
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
                _ingredientUpdatedSetter.SetForDelete((Events.IngredientUpdated.Builder)builderObj, entityExternalId, databaseId);
            else if (typeof(TBuilder) == typeof(Events.RecipeUpdated.Builder))
                _recipeUpdatedSetter.SetForDelete((Events.RecipeUpdated.Builder)builderObj, entityExternalId, databaseId);
            else if (typeof(TBuilder) == typeof(Events.MenuUpdated.Builder))
                _menuUpdatedSetter.SetForDelete((Events.MenuUpdated.Builder)builderObj, entityExternalId, databaseId);
            else if (typeof(TBuilder) == typeof(Events.GroupUpdated.Builder))
                _groupUpdatedSetter.SetForDelete((Events.GroupUpdated.Builder)builderObj, entityExternalId, databaseId);
            else if (typeof(TBuilder) == typeof(Events.MealPeriodUpdated.Builder))
                _mealPeriodUpdatedSetter.SetForDelete((Events.MealPeriodUpdated.Builder)builderObj, entityExternalId, databaseId);
            else if (typeof(TBuilder) == typeof(Events.SupplierUpdated.Builder))
                _supplierUpdatedSetter.SetForDelete((Events.SupplierUpdated.Builder)builderObj, entityExternalId, databaseId);
            else if (typeof(TBuilder) == typeof(Events.UserUpdated.Builder))
                _userUpdatedSetter.SetForDelete((Events.UserUpdated.Builder)builderObj, entityExternalId, databaseId);
            else if (typeof(TBuilder) == typeof(Events.SetUpdated.Builder))
                _setUpdatedSetter.SetForDelete((Events.SetUpdated.Builder)builderObj, entityExternalId, databaseId);

            return builder.Build();
        }

        public TMessage CreateUpdateEvent<TMessage, TBuilder>(string connectionString, int entityId, int databaseId)
            where TMessage : GeneratedMessage<TMessage, TBuilder>
            where TBuilder : GeneratedBuilder<TMessage, TBuilder>, new()
        {
            var builder = CreateBuilder<TMessage, TBuilder>(Events.ChangeType.UPDATE);
            object builderObj = builder; // builder cannot be cast directly to event builder for specific events

            if (typeof (TBuilder) == typeof (Events.IngredientUpdated.Builder))
                _ingredientUpdatedSetter.SetForUpdate((Events.IngredientUpdated.Builder) builderObj, connectionString, entityId, databaseId);
            else if (typeof (TBuilder) == typeof (Events.RecipeUpdated.Builder))
                _recipeUpdatedSetter.SetForUpdate((Events.RecipeUpdated.Builder) builderObj, connectionString, entityId, databaseId);
            else if (typeof(TBuilder) == typeof(Events.MenuUpdated.Builder))
                _menuUpdatedSetter.SetForUpdate((Events.MenuUpdated.Builder)builderObj, connectionString, entityId, databaseId);
            else if (typeof(TBuilder) == typeof(Events.GroupUpdated.Builder))
                _groupUpdatedSetter.SetForUpdate((Events.GroupUpdated.Builder)builderObj, connectionString, entityId, databaseId);
            else if (typeof(TBuilder) == typeof(Events.MealPeriodUpdated.Builder))
                _mealPeriodUpdatedSetter.SetForUpdate((Events.MealPeriodUpdated.Builder)builderObj, connectionString, entityId, databaseId);
            else if (typeof(TBuilder) == typeof(Events.SupplierUpdated.Builder))
                _supplierUpdatedSetter.SetForUpdate((Events.SupplierUpdated.Builder)builderObj, connectionString, entityId, databaseId);
            else if (typeof(TBuilder) == typeof(Events.UserUpdated.Builder))
                _userUpdatedSetter.SetForUpdate((Events.UserUpdated.Builder)builderObj, connectionString, entityId, databaseId);
            else if (typeof(TBuilder) == typeof(Events.SetUpdated.Builder))
                _setUpdatedSetter.SetForUpdate((Events.SetUpdated.Builder)builderObj, connectionString, entityId, databaseId);

            // the builder object is initialized since it was passed to initializes as referenced object
            return builder.Build();
        }

        #endregion
    }
}