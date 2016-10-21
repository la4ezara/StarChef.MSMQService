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
            Group g = new Group(entityId, dbConnectionString);

            var rand = new Random();


            var builder = Events.GroupUpdated.CreateBuilder();

            builder.SetCustomerId(cust.ExternalId)
                .SetCustomerName(cust.Name)
                .SetExternalId(g.ExternalId)
                .SetGroupName(g.Name)
                .SetGroupCode(g.Code)
                .SetDescription(g.Description)
                .SetCurrencyIso4217Code(g.CurrencyCode)
                .SetLanguageIso6391Code(g.LanguageCode)
                .SetSource(Events.SourceSystem.STARCHEF)
                .SetSequenceNumber(rand.Next(1, int.MaxValue));

            //builder.SetSuppliers()

            // Build the immutable data object
            var eventObj = builder.Build();

            return eventObj;

        }
    }
}