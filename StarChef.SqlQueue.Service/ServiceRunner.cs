using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Autofac;
using log4net;
using StarChef.Common;
using StarChef.SqlQueue.Service.Interface;
using System.Linq;

namespace StarChef.SqlQueue.Service
{
    public class ServiceRunner : IServiceRunner
    {
        private IAppConfiguration _appConfiguration;
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IListener _listener;

        public ServiceRunner()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<DependencyConfiguration>();
            IContainer container = builder.Build();

            _appConfiguration = container.Resolve<IAppConfiguration>();
            _listener = container.Resolve<IListener>();
        }

        public void Start()
        {
            Console.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            _logger.Info("SQL Queue Service is stared.");
            _listener.CanProcess = true;
            ThreadPool.QueueUserWorkItem(StartProcessing);
        }

        private bool _isCompleted;

        private async void StartProcessing(object state)
        {
            _logger.Info("SQL Queue Service is stared.");
            _isCompleted = await _listener.ExecuteAsync();
            _logger.Info("SQL Queue Service finish.");
        }

        public void Stop()
        {
            _logger.Info("SQL Queue Service is stopping.");
            _listener.CanProcess = false;

            while (!_isCompleted)
            {
                Thread.Sleep(2000);
            }

            while (this._listener.ActiveThreads.Any(c => c.IsAlive))
            {
                Thread.Sleep(2000);
            }

            _logger.Info("SQL Queue Service is stopped.");
        }

        public void Pause()
        {
            _listener.CanProcess = false;
            _logger.Info("SQL Queue Service is paused.");
        }
        public void Continue()
        {
            _listener.CanProcess = true;
            _logger.Info("SQL Queue Service is paused.");
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Stop();
            }
        }
    }
}