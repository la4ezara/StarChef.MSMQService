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
using static Fourth.Orchestration.Model.People.Events;
using log4net.Core;
using System.Linq;
using log4net;
using StarChef.Listener.Tests.Fixtures;

namespace StarChef.Listener.Tests.Handlers
{
    public class PriceBandEventHandlerTests : IClassFixture<ObjectMappingFixture>
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
        
        [Fact]
        public void All_logs_should_have_correct_database_guid()
        {
            // arrange
            string dbGuid = Guid.NewGuid().ToString();

            var priceBandUpdated = PriceBandUpdated.CreateBuilder();
            priceBandUpdated.SetCustomerId(dbGuid);

            var priceBand = PriceBand.CreateBuilder();
            priceBand.SetId(Guid.NewGuid().ToString()).SetMinimumPrice(0).SetMaximumPrice(1);
            priceBandUpdated.AddPriceBands(priceBand.Build());

            var payload = priceBandUpdated.Build();

            var dbCommands = new Mock<IDatabaseCommands>();
            var validator = new Mock<IEventValidator>(MockBehavior.Strict);
            validator.Setup(m => m.IsStarChefEvent(payload)).Returns(true);

            validator.Setup(m => m.IsEnabled(It.IsAny<object>())).Returns(true);
            validator.Setup(m => m.IsValid(It.IsAny<object>())).Returns(true);
            var messagingLogger = new Mock<IMessagingLogger>();

            var configuration = new Mock<IConfiguration>();
            configuration.SetupGet(p => p.PriceBandBatchSize).Returns(1);

            var handler = new PriceBandEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object, configuration.Object);
            var logChecker = new LogChecker(handler.GetType(), Level.All);

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.All(messageList, item =>
            {
                Assert.Equal(item.Properties[PriceBandEventHandler.DATABASE_GUID], dbGuid);
            });

            Assert.Null(ThreadContext.Properties[PriceBandEventHandler.DATABASE_GUID]);
        }

        [Fact]
        public void All_logs_should_have_correct_database_guid_no_pricebands_in_list()
        {
            // arrange
            string dbGuid = Guid.NewGuid().ToString();

            var priceBandUpdated = PriceBandUpdated.CreateBuilder();
            priceBandUpdated.SetCustomerId(dbGuid);

            var payload = priceBandUpdated.Build();

            var dbCommands = new Mock<IDatabaseCommands>();
            var validator = new Mock<IEventValidator>(MockBehavior.Strict);
            validator.Setup(m => m.IsStarChefEvent(payload)).Returns(true);

            validator.Setup(m => m.IsEnabled(It.IsAny<object>())).Returns(true);
            validator.Setup(m => m.IsValid(It.IsAny<object>())).Returns(true);
            var messagingLogger = new Mock<IMessagingLogger>();

            var configuration = new Mock<IConfiguration>();
            configuration.SetupGet(p => p.PriceBandBatchSize).Returns(1);

            var handler = new PriceBandEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object, configuration.Object);
            var logChecker = new LogChecker(handler.GetType(), Level.All);

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.All(messageList, item =>
            {
                Assert.Equal(item.Properties[PriceBandEventHandler.DATABASE_GUID], dbGuid);
            });

            Assert.Null(ThreadContext.Properties[PriceBandEventHandler.DATABASE_GUID]);
        }

        [Fact]
        public void All_logs_should_have_correct_database_guid_disabled_event()
        {
            // arrange
            string dbGuid = Guid.NewGuid().ToString();

            var priceBandUpdated = PriceBandUpdated.CreateBuilder();
            priceBandUpdated.SetCustomerId(dbGuid);

            var priceBand = PriceBand.CreateBuilder();
            priceBand.SetId(Guid.NewGuid().ToString()).SetMinimumPrice(0).SetMaximumPrice(1);
            priceBandUpdated.AddPriceBands(priceBand.Build());

            var payload = priceBandUpdated.Build();

            var dbCommands = new Mock<IDatabaseCommands>();
            var validator = new Mock<IEventValidator>(MockBehavior.Strict);
            validator.Setup(m => m.IsStarChefEvent(payload)).Returns(true);

            validator.Setup(m => m.IsEnabled(It.IsAny<object>())).Returns(false);
            validator.Setup(m => m.IsValid(It.IsAny<object>())).Returns(true);
            var messagingLogger = new Mock<IMessagingLogger>();

            var configuration = new Mock<IConfiguration>();
            configuration.SetupGet(p => p.PriceBandBatchSize).Returns(1);

            var handler = new PriceBandEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object, configuration.Object);
            var logChecker = new LogChecker(handler.GetType(), Level.All);

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.All(messageList, item =>
            {
                Assert.Equal(item.Properties[PriceBandEventHandler.DATABASE_GUID], dbGuid);
            });

            Assert.Null(ThreadContext.Properties[PriceBandEventHandler.DATABASE_GUID]);
        }

        [Fact]
        public void All_logs_should_have_correct_database_guid_invalid_payload()
        {
            // arrange
            string dbGuid = Guid.NewGuid().ToString();

            var priceBandUpdated = PriceBandUpdated.CreateBuilder();
            priceBandUpdated.SetCustomerId(dbGuid);

            var priceBand = PriceBand.CreateBuilder();
            priceBand.SetId(Guid.NewGuid().ToString()).SetMinimumPrice(0).SetMaximumPrice(1);
            priceBandUpdated.AddPriceBands(priceBand.Build());

            var payload = priceBandUpdated.Build();


            var dbCommands = new Mock<IDatabaseCommands>();
            var validator = new Mock<IEventValidator>(MockBehavior.Strict);
            validator.Setup(m => m.IsStarChefEvent(payload)).Returns(true);

            validator.Setup(m => m.IsEnabled(It.IsAny<object>())).Returns(true);
            validator.Setup(m => m.IsValid(It.IsAny<object>())).Returns(false);
            validator.Setup(m => m.GetErrors()).Returns(string.Empty);
            var messagingLogger = new Mock<IMessagingLogger>();

            var configuration = new Mock<IConfiguration>();
            configuration.SetupGet(p => p.PriceBandBatchSize).Returns(1);

            var handler = new PriceBandEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object, configuration.Object);
            var logChecker = new LogChecker(handler.GetType(), Level.All);

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.All(messageList, item =>
            {
                Assert.Equal(item.Properties[PriceBandEventHandler.DATABASE_GUID], dbGuid);
            });

            Assert.Null(ThreadContext.Properties[PriceBandEventHandler.DATABASE_GUID]);
        }

        [Fact]
        public void Should_log_exceptions_and_have_correct_database_guid()
        {
            // arrange
            string dbGuid = Guid.NewGuid().ToString();

            var priceBandUpdated = PriceBandUpdated.CreateBuilder();
            priceBandUpdated.SetCustomerId(dbGuid);

            var priceBand = PriceBand.CreateBuilder();
            priceBand.SetId(Guid.NewGuid().ToString()).SetMinimumPrice(0).SetMaximumPrice(1);
            priceBandUpdated.AddPriceBands(priceBand.Build());

            var payload = priceBandUpdated.Build();


            var dbCommands = new Mock<IDatabaseCommands>();
            dbCommands.Setup(d => d.SavePriceBandData(It.IsAny<Guid>(), It.IsAny<XmlDocument>())).Throws(new Exception());
            var validator = new Mock<IEventValidator>(MockBehavior.Strict);
            validator.Setup(m => m.IsStarChefEvent(payload)).Returns(true);

            validator.Setup(m => m.IsEnabled(It.IsAny<object>())).Returns(true);
            validator.Setup(m => m.IsValid(It.IsAny<object>())).Returns(true);
            var messagingLogger = new Mock<IMessagingLogger>();

            var configuration = new Mock<IConfiguration>();
            configuration.SetupGet(p => p.PriceBandBatchSize).Returns(1);

            var handler = new PriceBandEventHandler(dbCommands.Object, validator.Object, messagingLogger.Object, configuration.Object);
            var logChecker = new LogChecker(handler.GetType(), Level.All);

            // act
            var result = handler.HandleAsync(payload, "1").Result;

            var messageList = logChecker.LoggingEvents;
            logChecker.Dispose();

            // assert
            Assert.All(messageList, item =>
            {
                Assert.Equal(item.Properties[PriceBandEventHandler.DATABASE_GUID], dbGuid);
            });

            Assert.NotNull(
                messageList.Where(
                    item =>
                        item.ExceptionObject != null &&
                        item.ExceptionObject.GetType() == typeof(Exception)).FirstOrDefault()
                );

            Assert.Null(ThreadContext.Properties[PriceBandEventHandler.DATABASE_GUID]);
        }
    }
}
