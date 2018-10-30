using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarChef.Engine.IntegrationTests
{
    public sealed class TestConfiguration
    {
        private static TestConfiguration instance = null;
        private static readonly object padlock = new object();
        public readonly string ConnectionString;
        public readonly string SlLoginConnectionString;
        public readonly int TimeOut = 7200;
        public readonly int MaxTestsAmount;

        TestConfiguration()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["PriceRecalculateTests"].ConnectionString;
            SlLoginConnectionString = ConfigurationManager.ConnectionStrings["PriceRecalculateConnectionTests"].ConnectionString;
            int.TryParse(ConfigurationManager.AppSettings["MaxTestsAmount"], out MaxTestsAmount);
        }

        public static TestConfiguration Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new TestConfiguration();
                    }
                    return instance;
                }
            }
        }
    }
}
