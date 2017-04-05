using System;
using System.Configuration;
using System.Reflection;
using Fourth.Orchestration.Messaging;
using Fourth.Orchestration.Messaging.Azure;
using Fourth.Orchestration.Storage;
using Fourth.Orchestration.Storage.Azure;
using log4net;

using PriceBandUpdated = Fourth.Orchestration.Model.Recipes.Events.PriceBandUpdated;
using AccountCreated = Fourth.Orchestration.Model.People.Events.AccountCreated;
using AccountCreateFailed = Fourth.Orchestration.Model.People.Events.AccountCreateFailed;
using AccountUpdated = Fourth.Orchestration.Model.People.Events.AccountUpdated;
using AccountUpdateFailed = Fourth.Orchestration.Model.People.Events.AccountUpdateFailed;
using AccountStatusChanged = Fourth.Orchestration.Model.People.Events.AccountStatusChanged;
using AccountStatusChangeFailed = Fourth.Orchestration.Model.People.Events.AccountStatusChangeFailed;

namespace StarChef.Listener
{
    public class MessageBusController
    {
        private static readonly Lazy<IMessageStore> _messageStore = new Lazy<IMessageStore>(() => new AzureMessageStore());
        private static readonly Lazy<IMessagingFactory> _factory = new Lazy<IMessagingFactory>(() => new AzureMessagingFactory(_messageStore.Value));
        private static readonly Lazy<IMessagingHandlersFactory> _handlersFactory = new Lazy<IMessagingHandlersFactory>(() => new MessagingHandlersFactory());
        private static readonly Lazy<IMessageListener> _listener = new Lazy<IMessageListener>(() => _factory.Value.CreateMessageListener(ConfigurationManager.AppSettings["StarChefListenerName"]));
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Start()
        {
            try
            {
                _logger.Info("Starting Starchef listener");

                // Create an instance on a remote store for large messages.
                // Create a messaging factory.
                _listener.Value.RegisterHandler(_handlersFactory.Value.CreateHandler<PriceBandUpdated>());
                _listener.Value.RegisterHandler(_handlersFactory.Value.CreateHandler<AccountCreated>());
                _listener.Value.RegisterHandler(_handlersFactory.Value.CreateHandler<AccountCreateFailed>());
                _listener.Value.RegisterHandler(_handlersFactory.Value.CreateHandler<AccountUpdated>());
                _listener.Value.RegisterHandler(_handlersFactory.Value.CreateHandler<AccountUpdateFailed>());
                _listener.Value.RegisterHandler(_handlersFactory.Value.CreateHandler<AccountStatusChanged>());
                _listener.Value.RegisterHandler(_handlersFactory.Value.CreateHandler<AccountStatusChangeFailed>());

                _listener.Value.StartListener();
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to start listener due to unexpected error", ex);
                throw;
            }
        }

        public static void Stop()
        {
            _logger.Info("Stopping Starchef listener");
            if (_listener.IsValueCreated)
            {
                _listener.Value.StopListener();
                _listener.Value.Dispose();
            }
            if (_factory.IsValueCreated)
                _factory.Value.Dispose();
        }
    }
}