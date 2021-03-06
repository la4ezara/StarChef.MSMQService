﻿using System.Configuration;

namespace StarChef.MSMQService.Configuration.Impl
{
    public class AppConfiguration : IAppConfiguration
    {
        public string Alias { get; private set; }

        public string FromAddress { get; private set; }

        public string NormalQueueName { get; private set; }

        public string Subject { get; private set; }

        public string ToAddress { get; private set; }

        public int GlobalUpdateWaitTime { get; private set; }

        public string PoisonQueueName { get; private set; }

        public bool SendPoisonMessageNotification { get; private set; }

        public bool IsBackgroundTaskEnabled { get; private set; }

        public AppConfiguration()
        {
            this.GlobalUpdateWaitTime = int.Parse(ConfigurationManager.AppSettings["GlobalUpdateWaitTime"]);
            this.NormalQueueName = ConfigurationManager.AppSettings["StarChef.MSMQ.Queue"];
            this.PoisonQueueName = ConfigurationManager.AppSettings["StarChef.MSMQ.PoisonQueue"];
            this.Subject = ConfigurationManager.AppSettings["Subject"];
            this.ToAddress = ConfigurationManager.AppSettings["ToAddress"];
            this.Alias = ConfigurationManager.AppSettings["Alias"];
            this.FromAddress = ConfigurationManager.AppSettings["FromAddress"];
            this.SendPoisonMessageNotification = false;

            if (bool.TryParse(ConfigurationManager.AppSettings["enableBackgroundTask"], out var enableBackgroundTask))
            {
                this.IsBackgroundTaskEnabled = enableBackgroundTask;
            }
        }
    }
}