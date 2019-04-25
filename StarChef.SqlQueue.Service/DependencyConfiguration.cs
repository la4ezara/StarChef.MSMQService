using Autofac;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Messaging.Azure;
using Fourth.Orchestration.Storage;
using Fourth.Orchestration.Storage.Azure;
using log4net;
using StarChef.Common;
using StarChef.Orchestrate;
using StarChef.Orchestrate.EventSetters.Impl;
using StarChef.SqlQueue.Service.Configuration;
using StarChef.SqlQueue.Service.Interface;
using Events = Fourth.Orchestration.Model.Menus.Events;
using DeactivateAccountBuilder = Fourth.Orchestration.Model.People.Commands.DeactivateAccount.Builder;

namespace StarChef.SqlQueue.Service
{
    /// <summary>
    /// Responsible for setting up and configuring the dependencies.
    /// </summary>
    public class DependencyConfiguration : Module
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
       
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Listener>().As<IListener>().InstancePerLifetimeScope();
            builder.RegisterType<AppConfiguration>().As<IAppConfiguration>().InstancePerLifetimeScope();

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
            builder.RegisterType<SetUpdatedSetter>().As<IEventSetter<Events.SetUpdated.Builder>>().InstancePerLifetimeScope();
            builder.RegisterType<RecipeNutritionUpdatedSetter>().As<IEventSetter<Events.RecipeNutritionUpdated.Builder>>().InstancePerLifetimeScope();
            #endregion

            _logger.Info("Dependencies are configured.");
        }
    }
}
