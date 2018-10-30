
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
            _customerDbRepository = new CustomerDbRepository(cnStr, 7200);
            _pricingRepository = new PricingRepository(cnStr, 7200);
        }

        public RecalculationTests(string connectionString)
        {
            var cnStr = TestConfiguration.Instance.ConnectionString;
            if (!string.IsNullOrEmpty(connectionString))
            {
                cnStr = connectionString;
            }
            _customerDbRepository = new CustomerDbRepository(cnStr, 7200);
            _pricingRepository = new PricingRepository(cnStr, 7200);
        }

        public virtual void GlobalPriceRecalculation(ITestOutputHelper output)
        {
            IPriceEngine engine = new PriceEngine(_pricingRepository);
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            //act
            _customerDbRepository.ClearPrices();

            //var result = _pricingRepository.GetPrices();
            var result = _customerDbRepository.ExecutePriceRecalculation(0, 0, 0, 0, 0);
            sw.Stop();
            output.WriteLine($"Old price recalculation takes {sw.Elapsed.TotalSeconds} seconds.");

            //var result = _pricingRepository.GetPrices().Where(g => g.GroupId == 225);
            //var prices = engine.GlobalRecalculation().Where(g => g.GroupId == 225).ToList();
            sw.Restart();
            var prices = engine.GlobalRecalculation().ToList();
            output.WriteLine($"New price recalculation takes {sw.Elapsed.TotalSeconds} seconds.");
            //acknoledge

            var dbPrices = result.ToList();
            //Assert.Equal(dbPrices.Count, prices.Count);

            var priceToUpdate = engine.ComparePrices(dbPrices, prices).ToList();
            Assert.Empty(priceToUpdate);
            var pricesToDelete = engine.ComparePrices(prices, dbPrices).ToList();
            Assert.Empty(pricesToDelete);
        }
    }
}
