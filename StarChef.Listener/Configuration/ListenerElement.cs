using System.Configuration;

namespace StarChef.Listener.Configuration
{
    public class ListenerElement : ConfigurationElement
    {
        [ConfigurationProperty("handler", IsRequired = true, IsKey = true)]
        public string Handler
        {
            get { return (string) this["handler"]; }
            set { this["handler"] = value; }
        }

        [ConfigurationProperty("subscription", IsRequired = true)]
        public string Subscription
        {
            get { return (string)this["subscription"]; }
            set { this["subscription"] = value; }
        }
    }
}