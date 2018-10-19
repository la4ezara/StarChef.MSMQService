//using StarChef.Common.Engine;
//using StarChef.Common.Repository;
//using StarChef.Engine.Runner.Model;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Xunit;

//namespace StarChef.Engine.Runner
//{
//    public class CustomerDbIntegrationTests
//    {
//        public static string _cntStr = "Initial Catalog=SCNET_trg;Data Source=.\\sqlexpress;User ID=sl_web_user; Password=reddevil;";
//        private readonly CustomerDbRepository _customerDbRepository;
//        private readonly PricingRepository _pricingRepository;

//        public CustomerDbIntegrationTests() {
//            var cnStr = "Initial Catalog=SCNET_trg;Data Source=.\\sqlexpress;User ID=sl_web_user; Password=reddevil;";
//            _customerDbRepository = new CustomerDbRepository(cnStr);
//            _pricingRepository = new PricingRepository(cnStr, 0);
//        }

//        [Fact]
//        public void GlobalPriceRecalculation()
//        {
//            var request = new PriceRecalculationRequest();
//            IPriceEngine engine = new PriceEngine(_pricingRepository);

//            //act
//            var prices = engine.CalculatePrices(request.GroupId, request.ProductId, request.PbandId, request.UnitId, request.PsetId).ToList();

//            //acknoledge
//            var dbPrices = _pricingRepository.GetPrices().OrderBy(x => x.ProductId).ToList();
//            Assert.Equal(dbPrices.Count, prices.Count);

//            var priceToUpdate = engine.ComparePrices(dbPrices, prices).ToList();
//            Assert.Empty(priceToUpdate);
//            var pricesToDelete = engine.ComparePrices(prices, dbPrices).ToList();
//            Assert.Empty(pricesToDelete);
//        }

//        [Theory]
//        [ProductData("Initial Catalog=SCNET_trg;Data Source=.\\sqlexpress;User ID=sl_web_user; Password=reddevil;")]
//        public void Test(PriceRecalculationRequest priceRequest) {
//            IPriceEngine engine = new PriceEngine(_pricingRepository);

//            //act
//            var result = _customerDbRepository.ExecutePriceRecalculation(priceRequest.ProductId, priceRequest.GroupId, priceRequest.UnitId, priceRequest.PsetId, priceRequest.PbandId);
//            var prices = engine.CalculatePrices(priceRequest.GroupId, priceRequest.ProductId, priceRequest.PbandId, priceRequest.UnitId, priceRequest.PsetId).ToList();

//            //acknoledge
//            var dbPrices = _pricingRepository.GetPrices().OrderBy(x => x.ProductId).ToList();
//            Assert.Equal(dbPrices.Count, prices.Count);
            
//            var priceToUpdate = engine.ComparePrices(dbPrices, prices).ToList();
//            Assert.Empty(priceToUpdate);
//            var pricesToDelete = engine.ComparePrices(prices, dbPrices).ToList();
//            Assert.Empty(pricesToDelete);
//        }
//    }
//}
