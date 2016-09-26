using System;
using System.Reflection;
using log4net.Config;
using Autofac;
using Autofac.Configuration;
using Fourth.Orchestration.Messaging;
using log4net;

namespace SampleEventListener
{
    public class Program
    {
        /// <summary> The log4net Logger instance. </summary>
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Main(string[] args)
        {
            // Initialise log4net
            XmlConfigurator.Configure();

            // Initialize Autofac
            var builder = new ContainerBuilder();
            builder.RegisterModule(new ConfigurationSettingsReader("autofac"));
            var container = builder.Build();

            // Build the dependencies and run the application
            using (var scope = container.BeginLifetimeScope())
            {
                var factory = scope.Resolve<IMessagingFactory>();
                Run(factory);
            }
        }

        /// <summary>
        /// A loop that receives event messages sent on the service bus.
        /// </summary>
        /// <param name="factory">The messaging factory that creates connections to the service bus infrastructure.</param>
        public static void Run(IMessagingFactory factory)
        {
            // Create a listener that receives messages for an end-point called "recipeupdated".
            using (IMessageListener listener = factory.CreateMessageListener("recipeupdated"))
            {
                // Create an event handler
                var handler = new RecipeUpdatedEventHandler();

                // Register the handler with the listener
                listener.RegisterHandler(handler);

                // Start the listener
                listener.StartListener();

                Console.WriteLine("Now listening for events...");

                while (true)
                {
                }
            }
        }
    }
}