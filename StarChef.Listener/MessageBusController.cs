using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Messaging.Azure;
using Fourth.Orchestration.Storage;
using Fourth.Orchestration.Storage.Azure;
using log4net;
using StarChef.Listener.Configuration;

namespace StarChef.Listener
{
    public class MessageBusController
    {
        private static IMessagingFactory _factory;
        private static IMessageStore _messageStore;
        private static readonly List<IMessageListener> _listeners = new List<IMessageListener>();
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Start()
        {
            try
            {
                Logger.Info("Starting Starchef listener");

                // Create an instance on a remote store for large messages.
                _messageStore = new AzureMessageStore();

                // Create a messaging factory.
                _factory = new AzureMessagingFactory(_messageStore);

                var listenersConfig = AzureListenersSection.GetConfiguration();
                foreach (var pair in listenersConfig)
                {
                    var subscription = pair.Key;
                    var handlerTypes = pair.Value;

                    // Create a listener instance for listening to messages
                    var listener = _factory.CreateMessageListener(subscription);
                    _listeners.Add(listener);

                    // Register command handler listeners for all events and commands
                    var messageHandlers = handlerTypes.Select(Activator.CreateInstance).OfType<IMessageHandler>();
                    foreach (var messageHandler in messageHandlers)
                        listener.RegisterHandler(messageHandler);
                }

                //start listening
                foreach (var messageListener in _listeners)
                    messageListener.StartListener();
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

            if (_listeners.Any())
            {
                foreach (var messageListener in _listeners)
                {
                    messageListener.StopListener();
                    messageListener.Dispose();
                }
            }

            _factory?.Dispose();
        }
    }
}