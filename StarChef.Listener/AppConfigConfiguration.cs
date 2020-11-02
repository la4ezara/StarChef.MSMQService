using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;

namespace StarChef.Listener
{
    class AppConfigConfiguration : IConfiguration
    {
        public AppConfigConfiguration() {

            var priceBandBatchSize = 500;
            if (!int.TryParse(ConfigurationManager.AppSettings["PriceBandBatchSize"], out priceBandBatchSize))
            {
                priceBandBatchSize = 500;
            }

            this.PriceBandBatchSize = priceBandBatchSize;

            var appSettings = ConfigurationManager.AppSettings["UserDefaults"];
            if (appSettings != null)
            {
                Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(appSettings);
                UserDefaults = values;
            }
        }
        
        public int PriceBandBatchSize { get; private set; }
        public Dictionary<string, string> UserDefaults { get; private set; }
    }
}