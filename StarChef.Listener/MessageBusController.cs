using System;
using System.Configuration;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Messaging.Azure;
using Fourth.Orchestration.Storage;
using Fourth.Orchestration.Storage.Azure;
using log4net;

namespace StarChef.Listener
{
    public class MessageBusController
    {
        private static IMessagingFactory _factory;
        private static IMessageStore _messageStore;
        private static IMessageListener _priceBandListener;
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static string _priceBandEventSubscription;

        public static void Start()
        {
            try
            {
                Logger.Info("Starting starchef listener");

                // Create an instance on a remote store for large messages.
                _messageStore = new AzureMessageStore();

                // Create a messaging factory.
                _factory = new AzureMessagingFactory(_messageStore);

                _priceBandEventSubscription = ConfigurationManager.AppSettings["PriceBandEventSubscription"];

                // Create a listener instance for listening to messages
                _priceBandListener = _factory.CreateMessageListener(_priceBandEventSubscription);

                // Register command handler listeners for all events and commands
                _priceBandListener.RegisterHandler(new PriceBandEventHandler());

                //start listening
                _priceBandListener.StartListener();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to start listener due to unexpected error", ex);
                throw;
            }
        }

        public static void Stop()
        {
            Logger.Info("Stopping Starchef listener");

            if (_priceBandListener != null)
            {
                _priceBandListener.StopListener();
                _priceBandListener.Dispose();
            }

            _factory?.Dispose();
        }
    }    
}
