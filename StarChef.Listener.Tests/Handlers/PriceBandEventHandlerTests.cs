using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Moq;
using StarChef.Listener.Commands;
using StarChef.Listener.Handlers;
using Xunit;
using PriceBandUpdated = Fourth.Orchestration.Model.Recipes.Events.PriceBandUpdated;
using PriceBand = Fourth.Orchestration.Model.Recipes.Events.PriceBandUpdated.Types.PriceBand;

namespace StarChef.Listener.Tests.Handlers
{
    public class PriceBandEventHandlerTests
    {
        [Fact]
        public void Should_process_block_by_chunks()
        {
            #region arrange

            var configuration = Mock.Of<IConfiguration>();
            // all data blocks will be divided to chunks with 3 items each
            Mock.Get(configuration).Setup(m => m.PriceBandBatchSize).Returns(3);

            var databaseCommands = Mock.Of<IDatabaseCommands>();
            var handler = new PriceBandEventHandler(databaseCommands, Mock.Of<IEventValidator>(), Mock.Of<IMessagingLogger>(), configuration);

            var priceBandUpdated = PriceBandUpdated.CreateBuilder();
            priceBandUpdated.SetCustomerId(Guid.NewGuid().ToString());

            // creates 10 items to process it with 4 chunks (10 => chunks 3 + 3 + 3 + 1)
            const int ITEMS = 10;
            for (var i = 0; i < ITEMS; i++)
            {
                var priceBand = PriceBand.CreateBuilder();
                priceBand.SetId(Guid.NewGuid().ToString()).SetMinimumPrice(0).SetMaximumPrice(1);
                priceBandUpdated.AddPriceBands(priceBand.Build());
            }

            #endregion

            // act
            handler.HandleAsync(priceBandUpdated.Build(), Guid.Empty.ToString()).Wait();

            #region assert

            // verify that save was executed 4 times exactly
            var mock = Mock.Get(databaseCommands);
            mock.Verify(m => m.SavePriceBandData(It.IsAny<Guid>(), It.IsAny<XmlDocument>()), Times.Exactly(4));

            #endregion
        }
    }
}
