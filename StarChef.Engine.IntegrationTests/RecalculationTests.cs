
using StarChef.Common.Engine;
using StarChef.Common.Repository;
using StarChef.Engine.IntegrationTests.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StarChef.Engine.IntegrationTests
{
    public class RecalculationTests
    {
        private readonly CustomerDbRepository _customerDbRepository;
        private readonly PricingRepository _pricingRepository;

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

        public virtual void GlobalPriceRecalculation()
        {
            var request = new PriceRecalculationRequest();
            SinglePriceRecalculation(request);
        }

        public virtual void RecipePriceRecalculations()
        {
            var recipes = _customerDbRepository.GetProducts().ToList();
            MultiPriceRecalculations(recipes);
        }

        public virtual void IngredientsPriceRecalculations()
        {
            var ingredients = _customerDbRepository.GetIngredients().ToList();
            MultiPriceRecalculations(ingredients);
        }

        public virtual void GroupPriceRecalculations()
        {
            var groups = _customerDbRepository.GetGroups().ToList();
            MultiPriceRecalculations(groups);
        }

        public virtual void SetPriceRecalculations()
        {
            var sets = _customerDbRepository.GetSets().ToList();
            MultiPriceRecalculations(sets);
        }


        public virtual void PriceBandPriceRecalculations()
        {
            var priceBands = _customerDbRepository.GetSets().ToList();
            MultiPriceRecalculations(priceBands);
        }

        public virtual void UnitPriceRecalculations()
        {
            var units = _customerDbRepository.GetSets().ToList();
            MultiPriceRecalculations(units);
        }

        private void MultiPriceRecalculations(List<PriceRecalculationRequest> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                System.Diagnostics.Trace.WriteLine(item.ToString());
                SinglePriceRecalculation(item);
                System.Diagnostics.Debug.WriteLine($"{i} - {item}");
            }
        }

        public void SinglePriceRecalculation(PriceRecalculationRequest priceRequest)
        {
            IPriceEngine engine = new PriceEngine(_pricingRepository);

            //act
            var result = _customerDbRepository.ExecutePriceRecalculation(priceRequest.ProductId, priceRequest.GroupId, priceRequest.UnitId, priceRequest.PsetId, priceRequest.PbandId);
            var prices = engine.CalculatePrices(priceRequest.GroupId, priceRequest.ProductId, priceRequest.PbandId, priceRequest.UnitId, priceRequest.PsetId).ToList();

            //acknoledge
            //var dbPrices = _pricingRepository.GetPrices().OrderBy(x => x.ProductId).ToList();
            var dbPrices = result.ToList();
            Assert.Equal(dbPrices.Count, prices.Count);

            var priceToUpdate = engine.ComparePrices(dbPrices, prices).ToList();
            Assert.Empty(priceToUpdate);
            var pricesToDelete = engine.ComparePrices(prices, dbPrices).ToList();
            Assert.Empty(pricesToDelete);
        }
    }
}
