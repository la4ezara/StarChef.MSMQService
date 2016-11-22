using StarChef.Orchestrate.Models;
using Events = Fourth.Orchestration.Model.Menus.Events;
using System.Collections.Generic;
using System;

namespace StarChef.Orchestrate
{
    public class EventFactory
    {
        public static Events.RecipeUpdated CreateRecipeEvent(
            string dbConnectionString, 
            int entityId, 
            int databaseId
            )
        {
            Customer cust = new Customer(databaseId);
            Recipe recipe = new Recipe(entityId);

            try
            {
                var builder = recipe.Build(cust, dbConnectionString);

                // Build the immutable data object
                var eventObj = builder.Build();

                return eventObj;
            }
            catch(Exception ex)
            { }
            return null;
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