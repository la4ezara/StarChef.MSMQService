using Fourth.Orchestration.Model.Menus;
using StarChef.Orchestrate.Models;
using System;

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

            var rand = new Random();


            var builder = g.Build(cust, dbConnectionString);
            
            // Build the immutable data object
            var eventObj = builder.Build();

            return eventObj;

        }
    }
}