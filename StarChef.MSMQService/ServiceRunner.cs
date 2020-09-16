using Autofac;
using log4net;
using log4net.Config;
using StarChef.MSMQService.Configuration;
using StarChef.MSMQService.Interface;
using System;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using IContainer = Autofac.IContainer;

namespace StarChef.MSMQService
{
    public class ServiceRunner : IServiceRunner
    {
        private IAppConfiguration _appConfiguration;
        private readonly IListener _listener;

        public IAppConfiguration Configuration { get { return _appConfiguration; } }

        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Container _components;

        private readonly Hashtable _globalUpdateTimeStamps;
        private readonly Hashtable _activeTaskDatabaseIDs;

        public ServiceRunner()
        {

            _components = new Container();
            // Start log4net up
            XmlConfigurator.Configure();
            GlobalContext.Properties["component"] = "Fourth.StarChef.MSMQ";
            _globalUpdateTimeStamps = new Hashtable();
            _activeTaskDatabaseIDs = new Hashtable();

            var builder = new ContainerBuilder();
            builder.RegisterModule<DependencyConfiguration>();
            IContainer container = builder.Build();

            _appConfiguration = container.Resolve<IAppConfiguration>();
            _listener = container.Resolve<IListener>();
            _listener.MessageNotProcessing += _listener_MessageNotProcessing;
        }

        private void _listener_MessageNotProcessing(object sender, MessageProcessEventArgs e)
        {
            if (e.Message != null)
            {
                _logger.Warn($"MessageNotProcessing status:{e.Status} , message: {e.Message}");
            }
            else
            {
                _logger.Warn($"MessageNotProcessing status:{e.Status} , message: null");
            }
        }

        public void Start()
        {
            ThreadPool.QueueUserWorkItem(StartProcessing);
        }

        private bool _isCompleted;

        private async void StartProcessing(Object stateInfo)
        {
            _logger.Info("Service is stared.");
            _isCompleted = await _listener.ExecuteAsync(this._activeTaskDatabaseIDs, this._globalUpdateTimeStamps);
            _logger.Info("Service finish.");
        }

        public void Stop()
        {
            _logger.Info("Service is stoping.");
            _listener.CanProcess = false;

            while (!_isCompleted)
            {
                Thread.Sleep(2000);
            }
            _logger.Info("Service is stopped.");
        }

        public void ShutDown()
        {
            _logger.Info("Service is ShutDown.");
            _listener.CanProcess = false;

            while (!_isCompleted)
            {
                Thread.Sleep(2000);
            }
            _logger.Info("Service is ShutDown.");
        }

        public void Pause()
        {
            _listener.CanProcess = false;
            _logger.Info("Service is paused.");
        }

        public void Continue()
        {
            if (!_listener.CanProcess)
            {
                _listener.CanProcess = true;
                ThreadPool.QueueUserWorkItem(StartProcessing);
            }

            _logger.Info("Service is continued.");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        // NOTE: Leave out the finalizer altogether if this class doesn't   
        // own unmanaged resources itself, but leave the other methods  
        // exactly as they are.   
        ~ServiceRunner()
        {
            // Finalizer calls Dispose(false)  
            Dispose(false);
        }
        // The bulk of the clean-up code is implemented in Dispose(bool)  
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Stop();
            }
        }
    }
}
