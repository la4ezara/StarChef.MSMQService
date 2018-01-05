﻿using System;
using System.ComponentModel;
using System.Collections;
using log4net;
using StarChef.MSMQService.Configuration;
using Autofac;
using log4net.Config;
using StarChef.Common;
using IContainer = Autofac.IContainer;

namespace StarChef.MSMQService
{
    public class ServiceRunner : IDisposable
    {
        private IAppConfiguration _appConfiguration;
        private readonly IListener _listener;

        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private Container _components;

        private readonly Hashtable _globalUpdateTimeStamps;
        private readonly Hashtable _activeTaskDatabaseIDs;

        public ServiceRunner() {

            _components = new Container();
            // Start log4net up
            XmlConfigurator.Configure();
            log4netHelper.ConfigureAdoAppenderCommandText(Constant.CONFIG_LOG4NET_ADO_APPENDER_COMMAND);

            _globalUpdateTimeStamps = new Hashtable();
            _activeTaskDatabaseIDs = new Hashtable();

            var builder = new ContainerBuilder();
            builder.RegisterModule<DependencyConfiguration>();
            IContainer container = builder.Build();

            _appConfiguration = container.Resolve<IAppConfiguration>();
            _listener = container.Resolve<IListener>();
        }

        public void Start() {
            _logger.Info("Service is starting.");
            _listener.ExecuteAsync(this._activeTaskDatabaseIDs, this._globalUpdateTimeStamps).Wait();
            _logger.Info("Service is started.");
        }

        public void Stop() {
            _logger.Info("Service is stoping.");
            _listener.CanProcess = false;
            while (_listener.IsProcessing)
            {
                
            }

            _logger.Info("Service is stopped.");
        }

        public void Pause() {
            _listener.CanProcess = false;
            _logger.Info("Service is paused.");
        }

        public void Continue() {
            if (!_listener.CanProcess) {
                _listener.CanProcess = true;
                _listener.ExecuteAsync(this._activeTaskDatabaseIDs, this._globalUpdateTimeStamps).Wait();
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
