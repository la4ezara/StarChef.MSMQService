using StarChef.Common.Model;
using Xunit;

namespace StarChef.Common.Tests
{
    public class ModelTests
    {
        [Fact]
        public void DbPriceTest() {
            
            DbPrice price = new DbPrice() { GroupId = 1, ProductId = 1, Price = 1m };
            var result = price.Equals(null);
            Assert.False(result);

            DbPrice differentPrice = new DbPrice() { GroupId = 1, ProductId = 2, Price = 1m };

            result = price.Equals(differentPrice);
            Assert.False(result);

            differentPrice.ProductId = price.ProductId;
            differentPrice.Price = price.Price - (price.Delta * 2);

            result = price.Equals(differentPrice);
            Assert.False(result);

            differentPrice.Price = price.Price - (price.Delta / 2);

            result = price.Equals(differentPrice);
            Assert.True(result);

            var priceHash = price.GetHashCode();
            var diffPriceHash = differentPrice.GetHashCode();
            Assert.NotEqual<int>(priceHash, diffPriceHash);
        }
    }
}
