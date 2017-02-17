using System;
using System.Reflection;
using System.Xml;
using Moq;
using StarChef.Listener.Commands;
using StarChef.Listener.Handlers;
using Xunit;
using PriceBandUpdated = Fourth.Orchestration.Model.Recipes.Events.PriceBandUpdated;
using PriceBand = Fourth.Orchestration.Model.Recipes.Events.PriceBandUpdated.Types.PriceBand;
using Fourth.Orchestration.Messaging;
using StarChef.Listener.Tests.Helpers;

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
            var eventValidator = new Mock<IEventValidator>();
            eventValidator.Setup(m => m.IsEnabled(It.IsAny<PriceBandUpdated>())).Returns(true);
            eventValidator.Setup(m => m.IsValid(It.IsAny<PriceBandUpdated>())).Returns(true);
            var handler = new PriceBandEventHandler(databaseCommands, eventValidator.Object, Mock.Of<IMessagingLogger>(), configuration);

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

        [Fact]
        public void Should_not_process_invalid_payloads()
        {
            var eventValidator = new Mock<IEventValidator>();
            eventValidator.Setup(m => m.IsEnabled(It.IsAny<PriceBandUpdated>())).Returns(true);
            var handler = new PriceBandEventHandler(Mock.Of<IDatabaseCommands>(), eventValidator.Object, Mock.Of<IMessagingLogger>(), Mock.Of<IConfiguration>());
            var priceBandUpdated = PayloadHelpers.Construct<PriceBandUpdated>(new Type[0]);

            var result = handler.HandleAsync(priceBandUpdated, Guid.Empty.ToString()).Result;

            Assert.Equal(MessageHandlerResult.Fatal, result);
            eventValidator.Verify(m => m.GetErrors(), Times.Once);
        }

        [Fact]
        public void Should_not_process_payload_if_it_is_disabled()
        {
            var eventValidator = new Mock<IEventValidator>();
            eventValidator.Setup(m => m.IsEnabled(It.IsAny<PriceBandUpdated>())).Returns(false);
            var handler = new PriceBandEventHandler(Mock.Of<IDatabaseCommands>(), eventValidator.Object, Mock.Of<IMessagingLogger>(), Mock.Of<IConfiguration>());
            var priceBandUpdated = PayloadHelpers.Construct<PriceBandUpdated>(new Type[0]);

            var result = handler.HandleAsync(priceBandUpdated, Guid.Empty.ToString()).Result;

            Assert.Equal(MessageHandlerResult.Success, result);
            eventValidator.Verify(m => m.IsValid(It.IsAny<PriceBandUpdated>()), Times.Never);
        }
    }
}
