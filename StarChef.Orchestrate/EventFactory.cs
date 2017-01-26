using StarChef.Orchestrate.Models;
using Events = Fourth.Orchestration.Model.Menus.Events;
using System.Collections.Generic;
using Fourth.Orchestration.Model.Menus.Events;

namespace StarChef.Orchestrate
{
    public class EventFactory
    {
        public static Events.RecipeUpdated CreateRecipeUpdatedEvent(string dbConnectionString, int entityId, int databaseId)
        {
            var builder = CreateRecipeEventBuilder(dbConnectionString, entityId, databaseId);

            // Build the immutable data object
            var eventObj = builder.Build();

            return eventObj;
        }

        public static Events.RecipeUpdated CreateRecipeDeletedEvent(string dbConnectionString, int entityId, int databaseId)
        {
            var builder = CreateRecipeEventBuilder(dbConnectionString, entityId, databaseId);
            builder.SetChangeType(Events.ChangeType.DELETE);

            // Build the immutable data object
            var eventObj = builder.Build();

            return eventObj;
        }

        private static Events.RecipeUpdated.Builder CreateRecipeEventBuilder(string dbConnectionString, int entityId, int databaseId)
        {
            var cust = new Customer(databaseId);
            var recipe = new Recipe(entityId);
            var builder = recipe.Build(cust, dbConnectionString);
            return builder;
        }

        public static Events.MealPeriodUpdated CreateMealPeriodEvent(string dbConnectionString, int entityId, int databaseId)
        {
            Customer cust = new Customer(databaseId);
            MealPeriod mp = new MealPeriod(entityId);
            
            var builder = mp.Build(cust, dbConnectionString);

            // Build the immutable data object
            var eventObj = builder.Build();

            return eventObj;
        }

        public static Events.GroupUpdated CreateGroupUpdatedEvent(string dbConnectionString, int entityId, int databaseId)
        {
            var builder = CreateGroupUpdated(dbConnectionString, entityId, databaseId);

            // Build the immutable data object
            var eventObj = builder.Build();

            return eventObj;
        }

        public static Events.GroupUpdated CreateGroupDeletedEvent(string dbConnectionString, int entityId, int databaseId)
        {
            var builder = CreateGroupUpdated(dbConnectionString, entityId, databaseId);
            builder.SetChangeType(Events.ChangeType.DELETE);

            // Build the immutable data object
            var eventObj = builder.Build();

            return eventObj;
        }

        private static Events.GroupUpdated.Builder CreateGroupUpdated(string dbConnectionString, int entityId, int databaseId)
        {
            Customer cust = new Customer(databaseId);
            Group g = new Group(entityId);

            var builder = g.Build(cust, dbConnectionString);
            return builder;
        }

        public static Events.UserUpdated CreateUserEvent(string dbConnectionString, int entityId, int databaseId)
        {
            Customer cust = new Customer(databaseId);
            User u = new User(entityId);
            
            var builder = u.Build(cust, dbConnectionString);

            // Build the immutable data object
            var eventObj = builder.Build();

            return eventObj;
        }

        public static IEnumerable<Events.UserUpdated> CreateUserGroupEvent(
            string dbConnectionString, 
            int entityId, 
            int databaseId
            )
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

        public static Events.MenuUpdated CreateMenuUpdatedEvent(string dbConnectionString, int entityId, int databaseId)
        {
            var builder = CreateMenuUpdatedBuilder(dbConnectionString, entityId, databaseId);

            // Build the immutable data object
            var eventObj = builder.Build();

            return eventObj;
        }

        public static Events.MenuUpdated CreateMenuDeletedEvent(string dbConnectionString, int entityId, int databaseId)
        {
            var builder = CreateMenuUpdatedBuilder(dbConnectionString, entityId, databaseId);
            builder.SetChangeType(Events.ChangeType.DELETE);

            // Build the immutable data object
            var eventObj = builder.Build();

            return eventObj;
        }

        private static Events.MenuUpdated.Builder CreateMenuUpdatedBuilder(string dbConnectionString, int entityId, int databaseId)
        {
            Customer cust = new Customer(databaseId);
            Menu menu = new Menu(entityId);
            var builder = menu.Build(cust, dbConnectionString);
            return builder;
        }

        public static Events.IngredientUpdated CreateIngredientUpdatedEvent(string dbConnectionString, int entityId, int databaseId)
        {
            var builder = CreateIngredientEventBuilder(dbConnectionString, entityId, databaseId);

            // Build the immutable data object
            var eventObj = builder.Build();

            return eventObj;
        }

        public static Events.IngredientUpdated CreateIngredientDeletedEvent(string dbConnectionString, int entityId, int databaseId)
        {
            var builder = CreateIngredientEventBuilder(dbConnectionString, entityId, databaseId);
            builder.SetChangeType(Events.ChangeType.DELETE);

            // Build the immutable data object
            var eventObj = builder.Build();

            return eventObj;
        }

        private static Events.IngredientUpdated.Builder CreateIngredientEventBuilder(string dbConnectionString, int entityId, int databaseId)
        {
            Customer cust = new Customer(databaseId);
            Ingredient ingredient = new Ingredient(entityId);
            var builder = ingredient.Build(cust, dbConnectionString);
            return builder;
        }
    }
}