using System;
using System.Configuration;

namespace StarChef.MSMQService.Configuration.Impl
{
    public class AppConfiguration : IAppConfiguration
    {
        public string Alias { get; private set; }

        public string FromAddress { get; private set; }

        public string QueueName { get; private set; }

        public string Subject { get; private set; }

        public string ToAddress { get; private set; }

        public int GlobalUpdateWaitTime { get; private set; }

        public string PoisonQueue { get; private set; }
        public AppConfiguration()
        {
            this.GlobalUpdateWaitTime = int.Parse(ConfigurationManager.AppSettings["GlobalUpdateWaitTime"]);
            this.QueueName = ConfigurationManager.AppSettings["StarChef.MSMQ.Queue"];
            this.PoisonQueue = ConfigurationManager.AppSettings["StarChef.MSMQ.PoisonQueue"];
            this.Subject = ConfigurationManager.AppSettings["Subject"];
            this.ToAddress = ConfigurationManager.AppSettings["ToAddress"];
            this.Alias = ConfigurationManager.AppSettings["Alias"];
            this.FromAddress = ConfigurationManager.AppSettings["FromAddress"];
        }
    }
}