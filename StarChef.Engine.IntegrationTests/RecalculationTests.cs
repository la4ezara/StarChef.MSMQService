
using StarChef.Common.Engine;
using StarChef.Common.Repository;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace StarChef.Engine.IntegrationTests
{
    public class RecalculationTests
    {
        private readonly CustomerDbRepository _customerDbRepository;
        private readonly PricingRepository _pricingRepository;

        public RecalculationTests()
        {
            var cnStr = TestConfiguration.Instance.ConnectionString;
            _customerDbRepository = new CustomerDbRepository(cnStr, 72000);
            _pricingRepository = new PricingRepository(cnStr, 72000);
        }

        public RecalculationTests(string connectionString)
        {
            var cnStr = TestConfiguration.Instance.ConnectionString;
            if (!string.IsNullOrEmpty(connectionString))
            {
                cnStr = connectionString;
            }
            _customerDbRepository = new CustomerDbRepository(cnStr, 72000);
            _pricingRepository = new PricingRepository(cnStr, 72000);
        }

        public async virtual void GlobalPriceRecalculation(ITestOutputHelper output)
        {
            IPriceEngine engine = new PriceEngine(_pricingRepository);
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            
            //var result = _pricingRepository.GetPrices().Where(g => g.GroupId == 225);
            //var prices = engine.GlobalRecalculation().Where(g => g.GroupId == 225).ToList();
            
            _customerDbRepository.ClearPrices();

            //var result = await _pricingRepository.GetPrices();
            var result = _customerDbRepository.ExecutePriceRecalculation(0, 0, 0, 0, 0);
            sw.Stop();
            output.WriteLine($"Old price recalculation takes {sw.Elapsed.TotalSeconds} seconds.");
            //acknoledge
            sw.Restart();
            var dbPrices = result.ToList();

            //act
            var prices = await engine.GlobalRecalculation(false);
            sw.Stop();
            output.WriteLine($"New price recalculation takes {sw.Elapsed.TotalSeconds} seconds.");

            Assert.Equal(dbPrices.Count, prices.Count());

            var priceToUpdate = engine.ComparePrices(dbPrices, prices).ToList();
            Assert.Empty(priceToUpdate);
            var pricesToDelete = engine.ComparePrices(prices, dbPrices).ToList();
            Assert.Empty(pricesToDelete);

            output.WriteLine($"Total Prices {prices.Count() }");
        }

        public async virtual void GlobalPriceRecalculationStorage(ITestOutputHelper output)
        {
            IPriceEngine engine = new PriceEngine(_pricingRepository);
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            _customerDbRepository.ClearPrices();

            //act
            var prices = await engine.GlobalRecalculation(true);
            var dbCount = await _pricingRepository.GetPricesCount();

            sw.Stop();
            //assert
            Assert.Equal(dbCount, prices.Count());
            output.WriteLine($"Total Prices {prices.Count() }");
            output.WriteLine($"Total Seconds {sw.Elapsed.TotalSeconds }");
        }

        public async virtual void GlobalPriceRecalculationNoStorage(ITestOutputHelper output)
        {
            IPriceEngine engine = new PriceEngine(_pricingRepository);
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            _customerDbRepository.ClearPrices();

            //act
            var prices = await engine.GlobalRecalculation(false);

            sw.Stop();
            output.WriteLine($"Total Prices {prices.Count() }");
            output.WriteLine($"Total Seconds {sw.Elapsed.TotalSeconds }");
        }
    }
}
