using System;
using Fourth.Orchestration.Model.Menus;
using EventModel = Fourth.Orchestration.Model;
using CategoryModel = Fourth.Orchestration.Model.Recipes.Events.RecipeUpdated.Types.Category;
using StarChef.Common.Models;

namespace StarChef.Orchestrate
{
    public class EventFactory
    {
        public static Events.RecipeUpdated CreateRecipeEvent()
        {
            var rand = new Random();

            // Create a builder for the event
            var builder = Events.RecipeUpdated.CreateBuilder();

            // Build the immutable data object
            var eventObj = builder.Build();

            return eventObj;
        }

        public static Events.MealPeriodUpdated CreateMealPeriodEvent(string dbConnectionString,
            int entityId,
            int databaseId)
        {

            Customer cust = new Customer(databaseId);
            MealPeriod mp = new MealPeriod(entityId, dbConnectionString);

            var rand = new Random();

            
            var builder = Events.MealPeriodUpdated.CreateBuilder();

            builder.SetCustomerId(cust.ExternalId)
                .SetCustomerName(cust.Name)
                .SetExternalId(mp.ExternalId)
                .SetMealPeriodName(mp.Name)
                .SetSource(Events.SourceSystem.STARCHEF)
                .SetSequenceNumber(rand.Next(1, int.MaxValue));
            
            // Build the immutable data object
            var eventObj = builder.Build();

            return eventObj;

        }
    }
}