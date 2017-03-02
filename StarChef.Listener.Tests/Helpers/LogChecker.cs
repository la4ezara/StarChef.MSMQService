using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StarChef.Listener.Tests.Helpers
{
    internal class LogChecker : IDisposable
    {
        readonly Logger _logger;
        readonly MemoryAppender _appender;

        public LogChecker(Type classType, Level levelToCheck)
        {
            _logger = (Logger)LogManager.GetLogger(classType).Logger;
            _appender = new MemoryAppender();
            BasicConfigurator.Configure(_appender);
            _logger.Level = levelToCheck;
            _logger.AddAppender(_appender);
        }

        public List<LoggingEvent> LoggingEvents
        {
            get
            {
                return new List<LoggingEvent>(_appender.GetEvents());
            }
        }

        public void Dispose()
        {
            _logger.RemoveAllAppenders();
            _appender.Close();
        }
    };

}
