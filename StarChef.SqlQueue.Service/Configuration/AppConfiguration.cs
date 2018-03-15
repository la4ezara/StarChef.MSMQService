namespace StarChef.SqlQueue.Service.Configuration
{
    using System.Configuration;
    using StarChef.SqlQueue.Service.Interface;

    public class AppConfiguration : IAppConfiguration
    {
        public string UserDSN { get; private set; }

        public int MessagesCount { get; private set; }

        public int RetryCount { get; private set; }
        public int SleepMinutes { get; private set; }
        public int NewThreadMessages { get; private set; }
        public int MaxThreadCount { get; private set; }
        public AppConfiguration()
        {
            this.UserDSN = ConfigurationManager.AppSettings["DSN"];
            this.MessagesCount = int.Parse(ConfigurationManager.AppSettings["messagesCount"]);
            this.RetryCount = int.Parse(ConfigurationManager.AppSettings["retryCount"]);
            this.SleepMinutes = int.Parse(ConfigurationManager.AppSettings["sleepMinutes"]);
            this.NewThreadMessages = int.Parse(ConfigurationManager.AppSettings["newThreadMessages"]);
            this.MaxThreadCount = int.Parse(ConfigurationManager.AppSettings["maxThreadCount"]);
        }
    }
}