using Fourth.StarChef.Invariables;
using log4net;
using Moq;
using StarChef.Common;
using StarChef.Common.Engine;
using StarChef.Common.Types;
using StarChef.MSMQService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StarChef.MsmqService.Tests
{
    public class BackgroundTaskProcessorTests
    {
        [Fact]
        [Trait("Background Tasks", "AAA")]
        public void AAA() {

            var settings = new Dictionary<string, ImportTypeSettings>();
            settings.Add("IngredientPriceBand", new ImportTypeSettings() { AutoCalculateCost = true, AutoCalculateIntolerance = true, Id = 1, Name = "IngredientPriceBand" });

            var dbManagerMock = new Mock<IDatabaseManager>();
            var priceEngineMock = new Mock<IPriceEngine>();
            priceEngineMock.Setup(x => x.IsEngineEnabled()).ReturnsAsync(true);
            //priceEngineMock.Setup(x=> x.)
            var logMock = new Mock<ILog>();
            dbManagerMock.Setup(x=> x.GetImportSettings(It.IsAny<string>(), It.IsAny<int>())).Returns(() => { return settings; });

            BackgroundTaskProcessor processor = new BackgroundTaskProcessor(1, string.Empty, dbManagerMock.Object, priceEngineMock.Object, logMock.Object);
            BackgroundTask task = new BackgroundTask()
            {
                TaskType = Constants.MessageActionType.EntityImported,
                SubTaskType = Constants.MessageSubActionType.ImportedIngredientPriceBand,
                ExtendedProperties = @"{'PRICE_BANDS':'55'}"
            };

            task.ExtendedProperties = string.Empty;
            task.ExtendedProperties = @"{'PRICE_BANDS':''}";

            //processor.ProcessMessage(task);
            Assert.True(true);
        }
    }
}
