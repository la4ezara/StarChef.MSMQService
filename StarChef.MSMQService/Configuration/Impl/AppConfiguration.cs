using System;
using System.Configuration;

namespace StarChef.MSMQService.Configuration.Impl
{
    class AppConfiguration : IAppConfiguration
    {
        public string Alias => ConfigurationManager.AppSettings["Alias"];

        public string FromAddress => ConfigurationManager.AppSettings["FromAddress"];

        public long Interval => long.Parse(ConfigurationManager.AppSettings["interval"]);

        public int MsmqThreadCount => int.Parse(ConfigurationManager.AppSettings.Get("MSMQThreadCount"));

        public string QueuePath => ConfigurationManager.AppSettings.Get("StarChef.MSMQ.Queue");

        public string Subject => ConfigurationManager.AppSettings["Subject"];

        public string ToAddress => ConfigurationManager.AppSettings["ToAddress"];
    }
}