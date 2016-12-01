using StarChef.Orchestrate.Models;
using System;
using Fourth.Orchestration.Model.Examples;
using Commands = Fourth.Orchestration.Model.People.Commands;

namespace StarChef.Orchestrate
{
    public class CommandFactory
    {
        /// <summary>
        /// New User Activation command
        /// </summary>
        /// <param name="dbConnectionString"></param>
        /// <param name="entityId"></param>
        /// <param name="databaseId"></param>
        /// <returns></returns>
        public static Commands.ActivateAccount ActivateAccountCommand(string dbConnectionString, int entityId, int databaseId)
        {
            Customer cust = new Customer(databaseId);
            User u = new User(entityId);

            var builder = u.BuildActivateAccount(cust, dbConnectionString);

            // Build the immutable data object
            var eventObj = builder.Build();

            return eventObj;
        }

        public static Commands.DeactivateAccount DeactivateAccountCommand(string dbConnectionString, int entityId, int databaseId)
        {
            Customer cust = new Customer(databaseId);
            User u = new User(entityId);

            var builder = u.BuildDeactivateAccount(cust, dbConnectionString);

            // Build the immutable data object
            var eventObj = builder.Build();

            return eventObj;
        }

        public static Commands.CreateAccount CreateAccountCommand(string dbConnectionString, int entityId, int databaseId)
        {
            Customer cust = new Customer(databaseId);
            User u = new User(entityId);

            var builder = u.BuildCreateAccount(cust, dbConnectionString);

            // Build the immutable data object
            var eventObj = builder.Build();

            return eventObj;
        }

        public static Commands.UpdateAccount UpdateAccountCommand(string dbConnectionString, int entityId, int databaseId)
        {
            Customer cust = new Customer(databaseId);
            User u = new User(entityId);

            var builder = u.BuildUpdateAccount(cust, dbConnectionString);

            // Build the immutable data object
            var eventObj = builder.Build();

            return eventObj;
        }
    }
}
