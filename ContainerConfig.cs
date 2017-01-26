using Autofac;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Messaging.Azure;
using Fourth.Orchestration.Storage;
using Fourth.Orchestration.Storage.Azure;
using log4net;
using StarChef.Common;
using StarChef.Orchestrate;

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
            builder.RegisterType<EventFactory>().As<IEventFactory>().InstancePerLifetimeScope();

            return builder.Build();
        }
    }
}
