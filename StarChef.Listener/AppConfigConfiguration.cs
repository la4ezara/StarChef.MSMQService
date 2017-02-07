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
    }
}
