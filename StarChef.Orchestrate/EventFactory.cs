using Fourth.Orchestration.Model.Menus;
using StarChef.Orchestrate.Models;

namespace StarChef.Orchestrate
{
    public class EventFactory
    {
        public static Events.RecipeUpdated CreateRecipeEvent()
        {
            // Create a builder for the event
            var builder = Events.RecipeUpdated.CreateBuilder();

            // Build the immutable data object
            var eventObj = builder.Build();

            return eventObj;
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

        public static Events.GroupUpdated CreateGroupEvent(string dbConnectionString, int entityId, int databaseId)
        {
            Customer cust = new Customer(databaseId);
            Group g = new Group(entityId);

            var builder = g.Build(cust, dbConnectionString);
            
            // Build the immutable data object
            var eventObj = builder.Build();

            return eventObj;

        }

        public static Events.MenuUpdated UpdateMenuEvent(string dbConnectionString, int entityId, int databaseId)
        {
            Customer cust = new Customer(databaseId);
            Menu menu = new Menu(entityId);
            var builder = menu.Build(cust, dbConnectionString);

            // Build the immutable data object
            var eventObj = builder.Build();

            return eventObj;
        }
    }
}