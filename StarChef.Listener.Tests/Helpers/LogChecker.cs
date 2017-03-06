using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;

namespace StarChef.Listener.Tests.Helpers
{
    internal class LogChecker : IDisposable
    {
        readonly ILog _logger;
        readonly MemoryAppender _appender;
        readonly ILoggerRepository _repository;
        readonly string repositoryName;

        public LogChecker(Type classType, Level levelToCheck)
        {
            repositoryName = Guid.NewGuid().ToString();
            _repository = LogManager.CreateRepository(repositoryName);
            _logger = LogManager.GetLogger(repositoryName, classType);
            _appender = new MemoryAppender();
            BasicConfigurator.Configure(_repository, _appender);
            ((Logger)_logger.Logger).Level = levelToCheck;
            ((Logger)_logger.Logger).AddAppender(_appender);
        }

        public List<LoggingEvent> LoggingEvents
        {
            get
            {
                return new List<LoggingEvent>(_appender.GetEvents());
            }
        }

        public ILog GetLogger()
        {
            return _logger;
        }

        public void Dispose()
        {
            ((Logger)_logger.Logger).RemoveAllAppenders();
            _appender.Close();
            LogManager.ShutdownRepository(repositoryName);
        }
    };

}
