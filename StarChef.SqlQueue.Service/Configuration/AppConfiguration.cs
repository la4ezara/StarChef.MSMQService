namespace StarChef.SqlQueue.Service.Configuration
{
    using System.Configuration;
    using StarChef.SqlQueue.Service.Interface;

    public class AppConfiguration : IAppConfiguration
    {
        public string UserDSN { get; private set; }

        public int MessagesCount { get; private set; }
        
        public AppConfiguration()
        {
            this.UserDSN = ConfigurationManager.AppSettings["DSN"];
            this.MessagesCount = int.Parse(ConfigurationManager.AppSettings["messagesCount"]);
        }
    }
}