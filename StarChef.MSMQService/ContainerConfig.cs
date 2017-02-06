using Autofac;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Messaging.Azure;
using Fourth.Orchestration.Storage;
using Fourth.Orchestration.Storage.Azure;
using log4net;
using StarChef.Common;
using StarChef.Orchestrate;
using StarChef.Orchestrate.EventSetters.Impl;
using Events = Fourth.Orchestration.Model.Menus.Events;

using DeactivateAccount = Fourth.Orchestration.Model.People.Commands.DeactivateAccount;
using DeactivateAccountBuilder = Fourth.Orchestration.Model.People.Commands.DeactivateAccount.Builder;


namespace StarChef.MSMQService
{
    /// <summary>
    /// Responsible for setting up and configuring the dependencies.
    /// </summary>
    public class ContainerConfig
    {
        /// <summary> The log4net Logger instance. </summary>
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Sets up the IoC container with the correct dependencies.
        /// </summary>
        /// <param name="config">The configuration section that describes the dependencies to set up. </param>
        /// <returns>A container with the correct dependencies configured.</returns>
        public static IContainer Configure()
        {
            Logger.DebugFormat("Resolving dependencies.");

            var builder = new ContainerBuilder();

            // Set the messaging implementation
            builder.RegisterType<AzureMessageStore>().As<IMessageStore>().InstancePerLifetimeScope();
            builder.RegisterType<AzureMessagingFactory>().As<IMessagingFactory>().InstancePerLifetimeScope();
            builder.RegisterType<StarChefMessageSender>().As<IStarChefMessageSender>().InstancePerLifetimeScope();
            builder.RegisterType<DatabaseManager>().As<IDatabaseManager>().InstancePerLifetimeScope();

            #region command factory and setters
            builder.RegisterType<CommandFactory>().As<ICommandFactory>().InstancePerLifetimeScope();
            builder.RegisterType<DeactivateAccountSetter>().As<ICommandSetter<DeactivateAccountBuilder>>().InstancePerLifetimeScope();
            #endregion

            #region event factory and setters
            builder.RegisterType<EventFactory>().As<IEventFactory>().InstancePerLifetimeScope();
            builder.RegisterType<IngredientUpdateSetter>().As<IEventSetter<Events.IngredientUpdated.Builder>>().InstancePerLifetimeScope();
            builder.RegisterType<RecipeUpdatedSetter>().As<IEventSetter<Events.RecipeUpdated.Builder>>().InstancePerLifetimeScope();
            builder.RegisterType<MenuUpdatedSetter>().As<IEventSetter<Events.MenuUpdated.Builder>>().InstancePerLifetimeScope();
            builder.RegisterType<GroupUpdatedSetter>().As<IEventSetter<Events.GroupUpdated.Builder>>().InstancePerLifetimeScope();
            builder.RegisterType<MealPeriodUpdatedSetter>().As<IEventSetter<Events.MealPeriodUpdated.Builder>>().InstancePerLifetimeScope();
            builder.RegisterType<SupplierUpdatedSetter>().As<IEventSetter<Events.SupplierUpdated.Builder>>().InstancePerLifetimeScope();
            builder.RegisterType<UserUpdatedSetter>().As<IEventSetter<Events.UserUpdated.Builder>>().InstancePerLifetimeScope(); 
            #endregion

            return builder.Build();
        }
    }
}
