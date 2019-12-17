using StarChef.Orchestrate.Models;
using System;
using Google.ProtocolBuffers;
using Commands = Fourth.Orchestration.Model.People.Commands;

using SourceSystem = Fourth.Orchestration.Model.Common.SourceSystemId;
using DeactivateAccount = Fourth.Orchestration.Model.People.Commands.DeactivateAccount;
using DeactivateAccountBuilder = Fourth.Orchestration.Model.People.Commands.DeactivateAccount.Builder;
using Fourth.StarChef.Invariables;

namespace StarChef.Orchestrate
{
    public class CommandFactory : ICommandFactory
    {
        private readonly ICommandSetter<DeactivateAccountBuilder> _deactivateAccountSetter;
        public CommandFactory(ICommandSetter<DeactivateAccountBuilder> deactivateAccountSetter)
        {
            _deactivateAccountSetter = deactivateAccountSetter;
        }

        /// <summary>
        /// New User Activation command
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="databaseId"></param>
        /// <returns></returns>
        public static Commands.ActivateAccount ActivateAccountCommand(int entityId, int databaseId)
        {
            var builder = new User().BuildActivateAccount(entityId, databaseId);

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

        protected TBuilder CreateBuilder<TCommand, TBuilder>()
            where TCommand : GeneratedMessage<TCommand, TBuilder>
            where TBuilder : GeneratedBuilder<TCommand, TBuilder>, new()
        {
            dynamic result = null;

            if (typeof (TCommand) == typeof (DeactivateAccount))
                result = DeactivateAccount.CreateBuilder();

            if (result != null)
            {
                result.SetSource(SourceSystem.STARCHEF);
                // note: it's just a time marker, no business value for the platform team
                result.SetCommandId(Fourth.Orchestration.Model.UnixDateTime.FromDateTime(DateTime.UtcNow).ToString());
            }
            return (TBuilder) result;
        }

        public TCommand CreateCommand<TCommand, TBuilder>(UpdateMessage message)
            where TCommand : GeneratedMessage<TCommand, TBuilder>
            where TBuilder : GeneratedBuilder<TCommand, TBuilder>, new()
        {
            var builder = CreateBuilder<TCommand, TBuilder>();
            object builderObj = builder; // builder cannot be cast directly to event builder for specific events

            if (typeof (TCommand) == typeof (DeactivateAccount))
                _deactivateAccountSetter.Set((DeactivateAccountBuilder)builderObj, message);

            return builder.Build();
        }
    }
}
