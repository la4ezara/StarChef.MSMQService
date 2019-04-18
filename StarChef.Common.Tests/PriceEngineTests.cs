using Moq;
using StarChef.Common.Engine;
using StarChef.Common.Model;
using StarChef.Common.Repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;


namespace StarChef.Common.Tests
{
    public class PriceEngineTests
    {
        [Fact]
        public void SkipPriceRecalculation() {
            Mock<IPricingRepository> repo = new Mock<IPricingRepository>();
            int logIdResult = 123;
            string actionExpected = "Dish Pricing Calculation Skipped";
            string actionResult = string.Empty;
            DateTime dtResult = DateTime.MinValue;
            int productIdResult;
            DateTime messageTime = DateTime.UtcNow;

            repo.Setup(x => x.GetLastMsmqStartTime(0)).Returns(() => { return Task.FromResult<MsmqLog>(new MsmqLog() { StartTime = DateTime.UtcNow.AddHours(1) }); });
            repo.Setup(x => x.CreateMsmqLog(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>())).Callback<string, int,int,int,int,int, DateTime>((string action, int productId, int groupid, int pbandId, int setid, int unitId, DateTime dt) => {
                actionResult = action;
                dtResult = dt;
                productIdResult = productId;
            }).Returns(() => { return Task.FromResult<int>(logIdResult); });

            IPriceEngine engine = new PriceEngine(repo.Object);
            var result = engine.GlobalRecalculation(true, messageTime).Result;

            Assert.Empty(result);
            Assert.Equal(actionExpected, actionResult);
            Assert.Equal(messageTime, dtResult);
            Assert.Equal(messageTime, dtResult);
        }

        [Fact]
        public void SkipPriceRecalculationEndTime()
        {
            Mock<IPricingRepository> repo = new Mock<IPricingRepository>();
            int logIdResult = 123;
            string actionExpected = "Dish Pricing Calculation Skipped";
            string actionResult = string.Empty;
            DateTime dtResult = DateTime.MinValue;
            int productIdResult;
            DateTime messageTime = DateTime.UtcNow;

            repo.Setup(x => x.GetLastMsmqStartTime(0)).Returns(() => { return Task.FromResult<MsmqLog>(new MsmqLog() { EndTime = DateTime.UtcNow.AddHours(1) }); });
            repo.Setup(x => x.CreateMsmqLog(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>())).Callback<string, int, int, int, int, int, DateTime>((string action, int productId, int groupid, int pbandId, int setid, int unitId, DateTime dt) => {
                actionResult = action;
                dtResult = dt;
                productIdResult = productId;
            }).Returns(() => { return Task.FromResult<int>(logIdResult); });

            IPriceEngine engine = new PriceEngine(repo.Object);
            var result = engine.GlobalRecalculation(true, messageTime).Result;

            Assert.Empty(result);
            Assert.Equal(actionExpected, actionResult);
            Assert.Equal(messageTime, dtResult);
            Assert.Equal(messageTime, dtResult);
        }

        [Fact]
        public void PriceRecalculationNoStore()
        {
            Mock<IPricingRepository> repo = new Mock<IPricingRepository>();
            DateTime messageTime = DateTime.UtcNow;

            repo.Setup(x => x.GetLastMsmqStartTime(0)).Returns(() => { return Task.FromResult<MsmqLog>( new MsmqLog() { StartTime = DateTime.UtcNow.AddHours(-1) }); });
            
            repo.Setup(x=> x.GetProducts()).Returns( ()=> { return Task.FromResult<IEnumerable<Product>>(new List<Product>()); });
            repo.Setup(x => x.GetProductParts()).Returns(() => { return Task.FromResult<IEnumerable<ProductPart>>(new List<ProductPart>()); });
            repo.Setup(x => x.GetGroupProductPricesByGroup(It.IsAny<int>())).Returns(() => { return Task.FromResult<IEnumerable<ProductGroupPrice>>(new List<ProductGroupPrice>()); });

            IPriceEngine engine = new PriceEngine(repo.Object);
            var result = engine.GlobalRecalculation(false, messageTime).Result;

            Assert.Empty(result);
        }

        [Fact]
        public void PriceRecalculationStore()
        {
            Mock<IPricingRepository> repo = new Mock<IPricingRepository>();
            int logIdResult = 123;
            string actionExpected = "Dish Pricing Calculation";
            string actionResult = string.Empty;
            DateTime dtResult = DateTime.MinValue;
            int productIdResult;
            DateTime messageTime = DateTime.UtcNow;

            repo.Setup(x => x.GetLastMsmqStartTime(0)).Returns(() => { return Task.FromResult<MsmqLog>(new MsmqLog() { StartTime = DateTime.UtcNow.AddHours(-1) }); });
            repo.Setup(x => x.CreateMsmqLog(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>())).Callback<string, int, int, int, int, int, DateTime>((string action, int productId, int groupid, int pbandId, int setid, int unitId, DateTime dt) => {
                actionResult = action;
                dtResult = dt;
                productIdResult = productId;
            }).Returns(() => { return Task.FromResult<int>(logIdResult); });

            List<ProductGroupPrice> privateItems = new List<ProductGroupPrice>();
            privateItems.Add(new ProductGroupPrice() { GroupId = 0, Price = 123.321m, ProductId = 1122 });

            List<DbPrice> expectedPrices = new List<DbPrice>();
            expectedPrices.Add(new DbPrice() { ProductId = 1122, Price = 123.321m });

            List<Product> products = new List<Product>();
            products.Add(new Product() { ScopeId = 3, ProductId = 1122 });

            repo.Setup(x => x.GetProducts()).Returns(() => { return Task.FromResult<IEnumerable<Product>>(products); });
            repo.Setup(x => x.GetProductParts()).Returns(() => { return Task.FromResult<IEnumerable<ProductPart>>(new List<ProductPart>()); });
            repo.Setup(x => x.GetGroupProductPricesByGroup(It.IsAny<int>())).Returns(() => { return Task.FromResult<IEnumerable<ProductGroupPrice>>(privateItems); });

            repo.Setup(x => x.InsertPrices(It.IsAny<Dictionary<int, decimal>>(),  It.Is<int?>(g=> !g.HasValue), It.Is<int>(l=> l == 123), It.Is<DateTime>(d=> d == messageTime))).Returns(true);

            repo.Setup(x => x.UpdateMsmqLog(It.IsAny<DateTime>(), It.Is<int>(l => l == 123), It.Is<bool>(b => b))).Returns(()=> Task.FromResult<int>(1));

            IPriceEngine engine = new PriceEngine(repo.Object);
            var result = engine.GlobalRecalculation(true, messageTime).Result;

            Assert.Equal(actionExpected, actionResult);
            Assert.NotEmpty(result);
            Assert.Equal(expectedPrices, result);
        }

        [Fact]
        public void RecalculationStore()
        {
            Mock<IPricingRepository> repo = new Mock<IPricingRepository>();
            int logIdResult = 123;
            string actionExpected = "Dish Pricing Calculation";
            string actionExpectedSkipped = "Dish Pricing Calculation Skipped";
            string actionResult = string.Empty;
            DateTime dtResult = DateTime.MinValue;
            int productIdResult;
            DateTime messageTime = DateTime.UtcNow;
            int productId = 1122;
            bool isSuccessStatusUpdate = false;

            repo.Setup(x => x.GetLastMsmqStartTime(productId)).Returns(() => { return Task.FromResult<MsmqLog>(new MsmqLog() { StartTime = DateTime.UtcNow.AddHours(-1) }); });
            repo.Setup(x => x.CreateMsmqLog(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>())).Callback<string, int, int, int, int, int, DateTime>((string action, int product_Id, int groupid, int pbandId, int setid, int unitId, DateTime dt) => {
                actionResult = action;
                dtResult = dt;
                productIdResult = product_Id;
            }).Returns(() => { return Task.FromResult<int>(logIdResult); });

            List<ProductGroupPrice> privateItems = new List<ProductGroupPrice>();
            privateItems.Add(new ProductGroupPrice() { GroupId = 0, Price = 123.321m, ProductId = productId });

            List<DbPrice> expectedPrices = new List<DbPrice>();
            expectedPrices.Add(new DbPrice() { ProductId = productId, Price = 123.321m });

            List<Product> products = new List<Product>();
            products.Add(new Product() { ScopeId = 3, ProductId = productId });
             
            repo.Setup(x => x.GetProducts()).Returns(() => { return Task.FromResult(products.AsEnumerable()); });

            repo.Setup(x => x.GetProductParts()).Returns(() => { return Task.FromResult(new List<ProductPart>().AsEnumerable()); });

            var res = new Tuple<IEnumerable<Product>, IEnumerable<ProductPart>>(products, new List<ProductPart>());
            repo.Setup(x => x.GetProductsAndParts(It.IsAny<int>())).Returns(() => { return Task.FromResult(res); });


            repo.Setup(x => x.GetGroupProductPricesByProduct(It.IsAny<int>())).Returns(() => { return Task.FromResult<IEnumerable<ProductGroupPrice>>(privateItems); });

            repo.Setup(x => x.UpdatePrices(It.IsAny<Dictionary<int, decimal>>(), It.Is<int?>(g => !g.HasValue), It.Is<int>(l => l == 123), It.Is<DateTime>(d => d == messageTime))).Returns(true);

            repo.Setup(x => x.UpdateMsmqLog(It.IsAny<DateTime>(), It.Is<int>(l => l == 123), It.IsAny<bool>())).Callback<DateTime, int, bool>((DateTime dt,int product_Id, bool isSuccess) => {
                isSuccessStatusUpdate = isSuccess;
                dtResult = dt;
                productIdResult = product_Id;
            }).Returns(() => Task.FromResult<int>(1));

            IPriceEngine engine = new PriceEngine(repo.Object);
            var result = engine.Recalculation(productId, true, messageTime).Result;

            Assert.Equal(actionExpected, actionResult);
            Assert.NotEmpty(result);
            Assert.Equal(expectedPrices, result);
            Assert.True(isSuccessStatusUpdate);

            repo.Setup(x => x.GetLastMsmqStartTime(productId)).Returns(() => { return Task.FromResult<MsmqLog>(new MsmqLog() { StartTime = DateTime.UtcNow.AddHours(1) }); });

            engine = new PriceEngine(repo.Object);
            result = engine.Recalculation(productId, true, messageTime).Result;

            Assert.Equal(actionExpectedSkipped, actionResult);
            Assert.Empty(result);

            repo.Setup(x => x.GetLastMsmqStartTime(productId)).Returns(() => { return Task.FromResult<MsmqLog>(new MsmqLog() { StartTime = DateTime.UtcNow.AddHours(-1) }); });
            repo.Setup(x => x.UpdatePrices(It.IsAny<Dictionary<int, decimal>>(), It.Is<int?>(g => !g.HasValue), It.Is<int>(l => l == 123), It.Is<DateTime>(d => d == messageTime))).Returns(false);

            engine = new PriceEngine(repo.Object);
            result = engine.Recalculation(productId, true, messageTime).Result;

            Assert.Equal(actionExpected, actionResult);
            Assert.NotEmpty(result);
            Assert.Equal(expectedPrices, result);
            Assert.False(isSuccessStatusUpdate);
        }

        [Fact]
        public void PriceRecalculationStoreSaveFailed()
        {
            Mock<IPricingRepository> repo = new Mock<IPricingRepository>();
            int logIdResult = 123;
            string actionExpected = "Dish Pricing Calculation";
            string actionResult = string.Empty;
            DateTime dtResult = DateTime.MinValue;
            int productIdResult;
            DateTime messageTime = DateTime.UtcNow;

            repo.Setup(x => x.GetLastMsmqStartTime(0)).Returns(() => { return Task.FromResult<MsmqLog>(new MsmqLog() { StartTime = DateTime.UtcNow.AddHours(-1) }); });
            repo.Setup(x => x.CreateMsmqLog(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>())).Callback<string, int, int, int, int, int, DateTime>((string action, int productId, int groupid, int pbandId, int setid, int unitId, DateTime dt) => {
                actionResult = action;
                dtResult = dt;
                productIdResult = productId;
            }).Returns(() => { return Task.FromResult<int>(logIdResult); });

            List<ProductGroupPrice> privateItems = new List<ProductGroupPrice>();
            privateItems.Add(new ProductGroupPrice() { GroupId = 0, Price = 123.321m, ProductId = 1122 });

            List<DbPrice> expectedPrices = new List<DbPrice>();
            expectedPrices.Add(new DbPrice() { ProductId = 1122, Price = 123.321m });

            List<Product> products = new List<Product>();
            products.Add(new Product() { ScopeId = 3, ProductId = 1122 });

            repo.Setup(x => x.GetProducts()).Returns(() => { return Task.FromResult<IEnumerable<Product>>(products); });
            repo.Setup(x => x.GetProductParts()).Returns(() => { return Task.FromResult<IEnumerable<ProductPart>>(new List<ProductPart>()); });
            repo.Setup(x => x.GetGroupProductPricesByGroup(It.IsAny<int>())).Returns(() => { return Task.FromResult<IEnumerable<ProductGroupPrice>>(privateItems); });

            repo.Setup(x => x.InsertPrices(It.IsAny<Dictionary<int, decimal>>(), It.Is<int?>(g => !g.HasValue), It.Is<int>(l => l == 123), It.Is<DateTime>(d => d == messageTime))).Returns(false);
            bool expectedSave = false;
            bool currentSave = true;
            repo.Setup(x => x.UpdateMsmqLog(It.IsAny<DateTime>(), It.Is<int>(l => l == 123), It.Is<bool>(b => !b))).Returns(() => Task.FromResult<int>(1)).Callback((DateTime dt, int logid, bool success)=> { currentSave = success; });

            IPriceEngine engine = new PriceEngine(repo.Object);
            var result = engine.GlobalRecalculation(true, messageTime).Result;

            Assert.Equal(expectedSave, currentSave);
            Assert.Equal(actionExpected, actionResult);
            Assert.NotEmpty(result);
            Assert.Equal(expectedPrices, result);
        }

        [Fact]
        public void PriceRecalculationDisabled()
        {
            Mock<IPricingRepository> repo = new Mock<IPricingRepository>();

            repo.Setup(x => x.GetDbSetting(It.IsAny<string>())).Returns(() => { return Task.FromResult<string>(string.Empty); });

            IPriceEngine engine = new PriceEngine(repo.Object);
            var result = engine.IsEngineEnabled().Result;

            Assert.False(result);
        }

        [Fact]
        public void PriceRecalculationEnabled()
        {
            Mock<IPricingRepository> repo = new Mock<IPricingRepository>();

            repo.Setup(x => x.GetDbSetting(It.IsAny<string>())).Returns(() => { return Task.FromResult<string>("1"); });

            IPriceEngine engine = new PriceEngine(repo.Object);
            var result = engine.IsEngineEnabled().Result;

            Assert.True(result);
        }

        [Fact]
        public void PriceRecalculationPriceComparison()
        {
            Mock<IPricingRepository> repo = new Mock<IPricingRepository>();
            IPriceEngine engine = new PriceEngine(repo.Object);

            List<DbPrice> existingPrices = new List<DbPrice>();
            existingPrices.Add(new DbPrice() { GroupId = 1, ProductId = 1, Price =1m });
            existingPrices.Add(new DbPrice() { GroupId = 2, ProductId = 1, Price = 1m });

            List<DbPrice> newPrices = new List<DbPrice>();
            newPrices.Add(new DbPrice() { GroupId = 1, ProductId = 1, Price = 1m });
            newPrices.Add(new DbPrice() { GroupId = 2, ProductId = 1, Price = 1m });
            
            var result = engine.ComparePrices(existingPrices, newPrices);

            Assert.Empty(result);

            newPrices.Add(new DbPrice() { GroupId = 3, ProductId = 1, Price = 1m });

            result = engine.ComparePrices(existingPrices, newPrices);

            Assert.Single(result);

            existingPrices.Add(new DbPrice() { GroupId = 3, ProductId = 1, Price = 1.1m });

            result = engine.ComparePrices(existingPrices, newPrices);

            Assert.Single(result);

            existingPrices.Add(new DbPrice() { GroupId = 4, ProductId = 1, Price = 1m });
            result = engine.ComparePrices(existingPrices, newPrices);
            Assert.Single(result);
            result = engine.ComparePrices(newPrices, existingPrices);
            Assert.Equal(2, result.Count());
        }


        [Fact]
        public void PriceRecalculationPriceComparisonA()
        {
            Mock<IPricingRepository> repo = new Mock<IPricingRepository>();
            IPriceEngine engine = new PriceEngine(repo.Object);

            List<DbPrice> existingPrices = new List<DbPrice>();
            existingPrices.Add(new DbPrice() { GroupId = 1, ProductId = 1, Price = 1m });
            existingPrices.Add(new DbPrice() { GroupId = 1, ProductId = 2, Price = 2m });

            List<DbPrice> newPrices = new List<DbPrice>();
            newPrices.Add(new DbPrice() { GroupId = 1, ProductId = 1, Price = 1m });
            newPrices.Add(new DbPrice() { GroupId = 1, ProductId = 2, Price = 2m });

            var existingPrices_dict = existingPrices.GroupBy(g => g.GroupId).ToDictionary(k => k.Key, v => v.ToList());
            ConcurrentBag<DbPrice> bag = new ConcurrentBag<DbPrice>();
            engine.GetPriceDifferences(existingPrices_dict, bag, 1, newPrices);

            Assert.Empty(bag);

            newPrices.Add(new DbPrice() { GroupId = 1, ProductId = 3, Price = 1.1m });
            bag = new ConcurrentBag<DbPrice>();

            engine.GetPriceDifferences(existingPrices_dict, bag, 1, newPrices);
            Assert.Single(bag);

            newPrices = new List<DbPrice>();
            newPrices.Add(new DbPrice() { GroupId = 1, ProductId = 1, Price = 1.1m });
            bag = new ConcurrentBag<DbPrice>();
            engine.GetPriceDifferences(existingPrices_dict, bag, 1, newPrices);
            Assert.Single(bag);
        }
    }
}
