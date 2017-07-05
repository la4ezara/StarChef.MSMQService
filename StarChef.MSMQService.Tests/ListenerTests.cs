using System;
using System.Collections.Generic;
using Moq;
using StarChef.Common;
using StarChef.MSMQService.Configuration;
using StarChef.Orchestrate;
using Xunit;

namespace StarChef.MSMQService.Tests
{
    public class ListenerTests
    {
        [Fact(DisplayName = "Listener / ProcessProductCostUpdate / Should call price recalculation for all products")]
        public void ProcessProductCostUpdate_should_call_price_recalculation_for_all_products()
        {
            var productIds = new List<int>(new[] {1, 2, 3});
            var databaseManager = new Mock<IDatabaseManager>();
            databaseManager.Setup(m => m.GetProductsForPriceUpdate(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(productIds);
            var listener = new Listener(Mock.Of<IAppConfiguration>(), Mock.Of<IStarChefMessageSender>(), databaseManager.Object);
            var msg = new UpdateMessage
            {
                DSN = "any_text",
                ProductID = 123,
                ArrivedTime = DateTime.Now
            };

            listener.ProcessProductCostUpdate(msg).Wait();

            databaseManager.Verify(
                m => m.RecalculatePriceForProduct(It.Is<string>(p => msg.DSN == p), It.Is<int>(p => productIds.Contains(p)), It.IsAny<DateTime>()),
                Times.Exactly(3));
        }
    }
}
