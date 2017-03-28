using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarChef.Listener
{
    class AppConfigConfiguration : IConfiguration
    {
        public int PriceBandBatchSize
        {
            get
            {
                int priceBandBatchSize;
                if (int.TryParse(ConfigurationManager.AppSettings["PriceBandBatchSize"], out priceBandBatchSize))
                    return priceBandBatchSize;
                return 500;
            }
        }

        public Dictionary<string, string> UserDefaults
        {
            get
            {
                var appSettings = ConfigurationManager.AppSettings["UserDefaults"];
                Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(appSettings);
                return values;
            }
        }
    }
}
