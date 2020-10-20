using Autofac;
using Hangfire;
using Hangfire.Logging;
using Hangfire.SqlServer;
using log4net;
using log4net.Config;
using StarChef.BackgroundServices.Common;
using StarChef.BackgroundServices.Common.Jobs;
using StarChef.MSMQService.Configuration;
using StarChef.MSMQService.Interface;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Net.Configuration;
using System.Threading;
using IContainer = Autofac.IContainer;

namespace StarChef.MSMQService
{
    public class ServiceRunner : IServiceRunner
    {
        private IAppConfiguration _appConfiguration;
        private readonly IListener _listener;

        public IAppConfiguration Configuration { get { return _appConfiguration; } }

        private static readonly log4net.ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Container _components;

        private readonly Hashtable _globalUpdateTimeStamps;
        private readonly Hashtable _activeTaskDatabaseIDs;

        private BackgroundJobServer _server;
        private BackgroundJobServerOptions _options;

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


            // Recommended in: https://docs.hangfire.io/en/latest/configuration/using-sql-server.html
            var configuration = GlobalConfiguration.Configuration
                .UseAutofacActivator(container)
                .UseSqlServerStorage("SL_login", new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true
                })
                .UseLog4NetLogProvider();

            if (Environment.UserInteractive)
            {
                configuration.UseColouredConsoleLogProvider(LogLevel.Debug);
            }

            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
            {
                Attempts = 1,
                DelaysInSeconds = new[] { 60 },
                OnAttemptsExceeded = AttemptsExceededAction.Fail,
                LogEvents = true
            });

            _options = new BackgroundJobServerOptions
            {
                //WorkerCount = Environment.ProcessorCount * 5,
                WorkerCount = Environment.ProcessorCount,
                Queues =new string[]
                {
                    JobQueue.Default.ToString().ToLower(),
                    JobQueue.Critical.ToString().ToLower()
                }
            };
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
            if (_appConfiguration.IsBackgroundTaskEnabled)
            {
                _server = new BackgroundJobServer(_options);
            }
            else
            {
                ThreadPool.QueueUserWorkItem(StartProcessing);
            }
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
            _logger.Info("Service is stopping.");
            _listener.CanProcess = false;

            while (!_isCompleted)
            {
                Thread.Sleep(2000);
            }

            if (_appConfiguration.IsBackgroundTaskEnabled)
            {
                _server.Dispose();
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
            if (_appConfiguration.IsBackgroundTaskEnabled)
            {
                _server.Dispose();
            }
        }

        public void Continue()
        {
            if (!_listener.CanProcess)
            {
                _listener.CanProcess = true;
                ThreadPool.QueueUserWorkItem(StartProcessing);
            }

            if (_appConfiguration.IsBackgroundTaskEnabled)
            {
                _server = new BackgroundJobServer(_options);
            }

            _logger.Info("Service is continued.");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        // NOTE: Leave out the finalize altogether if this class doesn't   
        // own unmanaged resources itself, but leave the other methods  
        // exactly as they are.   
        ~ServiceRunner()
        {
            // Finalize calls Dispose(false)  
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
