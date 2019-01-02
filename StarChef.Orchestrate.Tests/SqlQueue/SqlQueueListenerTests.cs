using Fourth.StarChef.Invariables;
using Moq;
using StarChef.Common;
using StarChef.SqlQueue.Service;
using StarChef.SqlQueue.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace StarChef.Orchestrate.Tests.SqlQueue
{
    public class SqlQueueListenerTests
    {
        [Theory(DisplayName = "Sql Queue Listener StarChefEventsUpdated Success")]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.UserGroup)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.Ingredient)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.Dish)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.Menu)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.MealPeriodManagement)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.Group)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.Supplier)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.ProductSet)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.User)]
        [InlineData(Constants.MessageActionType.UserDeActivated, Constants.EntityType.User)]
        [InlineData(Constants.MessageActionType.EntityDeleted, Constants.EntityType.User)]
        [InlineData(Constants.MessageActionType.UserCreated, Constants.EntityType.User)]
        [InlineData(Constants.MessageActionType.UserActivated, Constants.EntityType.User)]
        [InlineData(Constants.MessageActionType.SalesForceUserCreated, Constants.EntityType.User)]
        [InlineData(Constants.MessageActionType.UpdatedGroup, Constants.EntityType.User)]
        [InlineData(Constants.MessageActionType.UpdatedProductCost, Constants.EntityType.NotSet)]
        public void TestProcessingEventUpdateSuccess(Constants.MessageActionType messageActionType, Constants.EntityType entityType)
        {

            HashSet<CalculateUpdateMessage> sendCommandMessages = new HashSet<CalculateUpdateMessage>();
            HashSet<CalculateUpdateMessage> sendDeleteMessages = new HashSet<CalculateUpdateMessage>();
            HashSet<CalculateUpdateMessage> sendMessages = new HashSet<CalculateUpdateMessage>();

            HashSet<OrchestrationLookup> orchestrationLookups = new HashSet<OrchestrationLookup>();
            orchestrationLookups.Add(new OrchestrationLookup((int)entityType, true));

            var moqLister = new Mock<Listener>(new object[] { new Mock<IAppConfiguration>().Object, new Mock<IDatabaseManager>().Object, new Mock<IStarChefMessageSender>().Object });
            var ud = new UserDatabase(0, string.Empty, string.Empty);
            ud.OrchestrationLookups = orchestrationLookups;

            var messagesToProcess = GetMessages(entityType, messageActionType, 2);

            moqLister.Setup(l => l.GetDatabaseMessages(It.IsAny<UserDatabase>())).Returns(messagesToProcess);

            moqLister.Setup(l => l.SendCommand(It.IsAny<CalculateUpdateMessage>(), It.IsAny<UserDatabase>())).Callback<CalculateUpdateMessage, UserDatabase>((message, database) =>
            {
                sendCommandMessages.Add(message);
            });

            moqLister.Setup(l => l.SendDeleteCommand(It.IsAny<CalculateUpdateMessage>(), It.IsAny<UserDatabase>())).Callback<CalculateUpdateMessage, UserDatabase>((message, database) =>
            {
                sendDeleteMessages.Add(message);
            });

            moqLister.Setup(l => l
            .Send(It.IsAny<EnumHelper.EntityTypeWrapper>(), It.IsAny<string>(), It.IsAny<CalculateUpdateMessage>(), It.IsAny<int>()))
                .Callback<EnumHelper.EntityTypeWrapper?, string, CalculateUpdateMessage, int>((wrapper, connectionString, message, databaseId) =>
                {
                    sendMessages.Add(message);
                });

            var obj = moqLister.Object;
            obj.CanProcess = true;
            //act

            Mock<StarChef.Common.Engine.IPriceEngine> priceEngine = new Mock<StarChef.Common.Engine.IPriceEngine>();
            priceEngine.Setup<Task<bool>>(x => x.IsEngineEnabled()).Returns(Task.FromResult(true));
            priceEngine.Setup<Task<IEnumerable<Common.Model.DbPrice>>>(x => x.GlobalRecalculation(It.IsAny<bool>(), It.IsAny<DateTime?>())).Returns(Task.FromResult((new List<Common.Model.DbPrice>().AsEnumerable())));

            obj.Process(priceEngine.Object, ud, messagesToProcess.Count);
            //assert results
            if (messageActionType == Constants.MessageActionType.StarChefEventsUpdated ||
                messageActionType == Constants.MessageActionType.UserCreated ||
                messageActionType == Constants.MessageActionType.UserActivated ||
                messageActionType == Constants.MessageActionType.SalesForceUserCreated)
            {
                Assert.Equal(sendMessages.Count, messagesToProcess.Count);
                Assert.Empty(sendCommandMessages);
                Assert.Empty(sendDeleteMessages);
            }
            else if (messageActionType == Constants.MessageActionType.UserDeActivated)
            {
                Assert.Equal(sendCommandMessages.Count, messagesToProcess.Count);
                Assert.Empty(sendMessages);
                Assert.Empty(sendDeleteMessages);
            }
            else if (messageActionType == Constants.MessageActionType.EntityDeleted)
            {
                Assert.Equal(sendDeleteMessages.Count, messagesToProcess.Count);
                Assert.Empty(sendCommandMessages);
                Assert.Empty(sendMessages);
            }
        }

        [Theory(DisplayName = "Sql Queue Listener StarChefEventsUpdated Success")]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.UserUnit)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.Category)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.SuppliedDish)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.Document)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.Package)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.IngredientCostPrice)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.DishDetails)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.DishPricing)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.DishAdvancedNutrition)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.Manufacturer)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.Nutrition)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.GroupFilter)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.BasicReports)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.StandardReports)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.AdvancedReports)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.AdminReports)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.PriceBand)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.PriceBandOverride)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.UserPreferences)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.MenuCycle)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.UserLogin)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.ListManager)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.MenuSet)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.MenuPeriod)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.IngredientCostManagement)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.GlobalSearchReplace)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.GroupManagement)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.DbSettings)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.DataManagement)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.PricingManagement)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.SystemDelete)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.AllowReplaceRemoveRecs)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.PriceBandManagement)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.DlineLookup)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.MasterListExportMangement)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.InterfaceManagerManagement)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.MassCopyRecipe)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.MassEditRecipe)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.Intolerance)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.BudgetedCostManagement)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.ProductSpecificationManagement)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.IngredientImport)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.SetManagement)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.StarChefLiteManagement)]
        [InlineData(Constants.MessageActionType.UserCreated, Constants.EntityType.Dish)]
        [InlineData(Constants.MessageActionType.UserActivated, Constants.EntityType.Dish)]
        [InlineData(Constants.MessageActionType.SalesForceUserCreated, Constants.EntityType.Dish)]
        [InlineData(Constants.MessageActionType.UpdatedProductCost, Constants.EntityType.NotSet)]
        public void TestProcessingEventUpdateNotSent(Constants.MessageActionType messageActionType, Constants.EntityType entityType)
        {

            HashSet<CalculateUpdateMessage> sendCommandMessages = new HashSet<CalculateUpdateMessage>();
            HashSet<CalculateUpdateMessage> sendDeleteMessages = new HashSet<CalculateUpdateMessage>();
            HashSet<CalculateUpdateMessage> sendMessages = new HashSet<CalculateUpdateMessage>();

            HashSet<OrchestrationLookup> orchestrationLookups = new HashSet<OrchestrationLookup>();
            orchestrationLookups.Add(new OrchestrationLookup((int)entityType, true));

            var moqLister = new Mock<Listener>(new object[] { new Mock<IAppConfiguration>().Object, new Mock<IDatabaseManager>().Object, new Mock<IStarChefMessageSender>().Object });
            var ud = new UserDatabase(0, string.Empty, string.Empty);
            ud.OrchestrationLookups = orchestrationLookups;

            var messagesToProcess = GetMessages(entityType, messageActionType, 2);

            moqLister.Setup(l => l.GetDatabaseMessages(It.IsAny<UserDatabase>())).Returns(messagesToProcess);

            moqLister.Setup(l => l.SendCommand(It.IsAny<CalculateUpdateMessage>(), It.IsAny<UserDatabase>())).Callback<CalculateUpdateMessage, UserDatabase>((message, database) => {
                sendCommandMessages.Add(message);
            });

            moqLister.Setup(l => l.SendDeleteCommand(It.IsAny<CalculateUpdateMessage>(), It.IsAny<UserDatabase>())).Callback<CalculateUpdateMessage, UserDatabase>((message, database) => {
                sendDeleteMessages.Add(message);
            });
            var enqueueCount = 0;
            moqLister.Setup(l => l.Enqueue(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<OrchestrationQueueStatus>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                .Callback<string,int,int,OrchestrationQueueStatus,int,DateTime,int,string,int>((conn,entityId, entityTypeId, status, retryCount, dateCreated, databaseId, externalId, messageActionTypeId)=> {
                    enqueueCount++;
                });

            moqLister.Setup(l => l
            .Send(It.IsAny<EnumHelper.EntityTypeWrapper>(), It.IsAny<string>(), It.IsAny<CalculateUpdateMessage>(), It.IsAny<int>()))
                .Callback<EnumHelper.EntityTypeWrapper?, string, CalculateUpdateMessage, int>((wrapper, connectionString, message, databaseId) => {
                    sendMessages.Add(message);
                });

            var obj = moqLister.Object;
            obj.CanProcess = true;
            //act

            Mock<StarChef.Common.Engine.IPriceEngine> priceEngine = new Mock<StarChef.Common.Engine.IPriceEngine>();
            priceEngine.Setup<Task<bool>>(x => x.IsEngineEnabled()).Returns(Task.FromResult(false));
            priceEngine.Setup<Task<IEnumerable<Common.Model.DbPrice>>>(x => x.GlobalRecalculation(It.IsAny<bool>(), It.IsAny<DateTime?>())).Returns(Task.FromResult((new List<Common.Model.DbPrice>().AsEnumerable())));

            obj.Process(priceEngine.Object, ud, messagesToProcess.Count);

            //assert results
            if (messageActionType == Constants.MessageActionType.StarChefEventsUpdated ||
                messageActionType == Constants.MessageActionType.UserCreated ||
                messageActionType == Constants.MessageActionType.UserActivated ||
                messageActionType == Constants.MessageActionType.SalesForceUserCreated || messageActionType == Constants.MessageActionType.UpdatedProductCost)
            {
                Assert.Equal(messagesToProcess.Count, enqueueCount);
                Assert.Empty(sendMessages);
                Assert.Empty(sendCommandMessages);
                Assert.Empty(sendDeleteMessages);
            }
        }

        [Theory(DisplayName = "Sql Queue Listener StarChefEventsUpdated Publish not Enabled")]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.UserUnit)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.Category)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.SuppliedDish)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.Document)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.Package)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.IngredientCostPrice)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.DishDetails)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.DishPricing)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.DishAdvancedNutrition)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.Manufacturer)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.Nutrition)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.GroupFilter)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.BasicReports)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.StandardReports)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.AdvancedReports)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.AdminReports)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.PriceBand)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.PriceBandOverride)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.UserPreferences)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.MenuCycle)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.UserLogin)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.ListManager)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.MenuSet)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.MenuPeriod)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.IngredientCostManagement)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.GlobalSearchReplace)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.GroupManagement)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.DbSettings)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.DataManagement)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.PricingManagement)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.SystemDelete)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.AllowReplaceRemoveRecs)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.PriceBandManagement)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.DlineLookup)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.MasterListExportMangement)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.InterfaceManagerManagement)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.MassCopyRecipe)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.MassEditRecipe)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.Intolerance)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.BudgetedCostManagement)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.ProductSpecificationManagement)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.IngredientImport)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.SetManagement)]
        [InlineData(Constants.MessageActionType.StarChefEventsUpdated, Constants.EntityType.StarChefLiteManagement)]
        [InlineData(Constants.MessageActionType.UserCreated, Constants.EntityType.Dish)]
        [InlineData(Constants.MessageActionType.UserActivated, Constants.EntityType.Dish)]
        [InlineData(Constants.MessageActionType.SalesForceUserCreated, Constants.EntityType.Dish)]
        [InlineData(Constants.MessageActionType.UpdatedProductCost, Constants.EntityType.NotSet)]
        public void TestProcessingEventEnqueuesNotProcessedMessages(Constants.MessageActionType messageActionType, Constants.EntityType entityType)
        {
            HashSet<OrchestrationLookup> orchestrationLookups = new HashSet<OrchestrationLookup>();
            orchestrationLookups.Add(new OrchestrationLookup((int)entityType, false));

            var moqLister = new Mock<Listener>(new object[] { new Mock<IAppConfiguration>().Object, new Mock<IDatabaseManager>().Object, new Mock<IStarChefMessageSender>().Object });
            var ud = new UserDatabase(0, string.Empty, string.Empty);
            ud.OrchestrationLookups = orchestrationLookups;

            var messagesToProcess = GetMessages(entityType, messageActionType, 2);

            moqLister.Setup(l => l.GetDatabaseMessages(It.IsAny<UserDatabase>())).Returns(messagesToProcess);

            var enqueueCount = 0;
            moqLister.Setup(l => l.Enqueue(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<OrchestrationQueueStatus>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                .Callback<string, int, int, OrchestrationQueueStatus, int, DateTime, int, string, int>((conn, entityId, entityTypeId, status, retryCount, dateCreated, databaseId, externalId, messageActionTypeId) =>
                {
                    enqueueCount++;
                });

            var obj = moqLister.Object;
            obj.CanProcess = true;

            Mock<StarChef.Common.Engine.IPriceEngine> priceEngine = new Mock<StarChef.Common.Engine.IPriceEngine>();
            priceEngine.Setup(x => x.IsEngineEnabled()).Returns(Task.FromResult(false));
            priceEngine.Setup(x => x.GlobalRecalculation(It.IsAny<bool>(), It.IsAny<DateTime?>())).Returns(Task.FromResult((new List<Common.Model.DbPrice>().AsEnumerable())));

            // Act
            obj.Process(priceEngine.Object, ud, messagesToProcess.Count);

            // Assert
            moqLister.Verify(x => x.Enqueue(
                It.Is<string>(c => c == ud.ConnectionString),
                It.Is<int>(c => c == messagesToProcess.First().ProductID),
                It.Is<int>(c => c == messagesToProcess.First().EntityTypeId),
                It.Is<OrchestrationQueueStatus>(c => c == OrchestrationQueueStatus.Ignored),
                It.Is<int>(c => c == messagesToProcess.First().RetryCount),
                It.Is<DateTime>(c => c == messagesToProcess.First().ArrivedTime),
                It.Is<int>(c => c == ud.DatabaseId),
                It.Is<string>(c => c == messagesToProcess.First().ExternalId),
                It.Is<int>(c => c == messagesToProcess.First().Action)));

            Assert.Equal(messagesToProcess.Count, enqueueCount);
        }

        protected HashSet<CalculateUpdateMessage> GetMessages(Constants.EntityType entityType, Constants.MessageActionType messageActionType, int count)
        {
            HashSet<CalculateUpdateMessage> messages = new HashSet<CalculateUpdateMessage>();

            for (int i = 0; i < count; i++)
            {
                var calculateMessage = new CalculateUpdateMessage(i, string.Empty, (int)messageActionType, 0, (int)entityType, i, 0, OrchestrationQueueStatus.New);
                calculateMessage.ExternalId = "ExternalId";
                messages.Add(calculateMessage);
            }

            return messages;
        }
    }
}
