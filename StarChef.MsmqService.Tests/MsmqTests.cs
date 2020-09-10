using FluentAssertions;
using Fourth.StarChef.Invariables;
using Moq;
using StarChef.Common;
using StarChef.Common.Engine;
using StarChef.Common.Types;
using StarChef.MSMQService;
using StarChef.MSMQService.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Messaging;
using System.Threading.Tasks;
using Xunit;
using static Messaging.MSMQ.Enums;

namespace StarChef.MsmqService.Tests
{
    public class MsmqListenerTests
    {
        [Fact(DisplayName = "MSMQ Listener process multiple messages")]
        [Trait("MSMQ Listener", "ProcessMultipleItems")]
        public void ProcessMultipleItems()
        {
            Queue normalQueue = new Queue();
            Queue poisonQueue = new Queue();
            List<UpdateMessage> processMessages = new List<UpdateMessage>();
            IListener listener = null;

            var messageManager = GetMockMessageManager(normalQueue, poisonQueue, listener);

            var config = new Mock<IAppConfiguration>();
            config.Setup(x => x.GlobalUpdateWaitTime).Returns(10);

            List<string> calledStoredProcedures = new List<string>();
            var dbManager = GetMockDatabaseManager(calledStoredProcedures);

            listener = new Listener(config.Object, dbManager.Object, messageManager.Object);
            listener.MessageNotProcessing += delegate (System.Object o, MessageProcessEventArgs e) { listener.CanProcess = false; };

            Hashtable activeDatabases = new Hashtable();
            Hashtable timestamps = new Hashtable();

            //send 1 message to fake queue
            var singleMessage = new UpdateMessage() { DatabaseID = 1, ExternalId = "1", EntityTypeId = 20, DSN="a1", Action = (int)MessageActionType.UpdatedProductNutrient, GroupID = 1, ProductID = 1, ArrivedTime = DateTime.UtcNow };
            messageManager.Object.mqSend(singleMessage, MessagePriority.High);
            //check do we have message in queue
            normalQueue.Should().NotBeEmpty();
            normalQueue.Should().HaveCount(1, "single item send");

            singleMessage = new UpdateMessage() { DatabaseID = 2, ExternalId = "2", EntityTypeId = 20, Action = (int)MessageActionType.UpdatedProductIntolerance, GroupID = 2, ProductID = 2, ArrivedTime = DateTime.UtcNow };
            messageManager.Object.mqSend(singleMessage, MessagePriority.High);

            //check do we have message in queue
            normalQueue.Should().NotBeEmpty();
            normalQueue.Should().HaveCount(2, "single item send");

            listener.CanProcess = true;
            listener.ExecuteAsync(activeDatabases, timestamps);

            normalQueue.Should().BeEmpty();
            normalQueue.Should().HaveCount(0, "item processed");

            poisonQueue.Should().NotBeEmpty();
            poisonQueue.Should().HaveCount(1, "single poison item");
        }

        [Fact(DisplayName = "MSMQ Listener process multiple messages start/pause/countinue")]
        [Trait("MSMQ Listener", "ProcessMultipleStartStops")]
        public void ProcessMultipleStartStops()
        {
            Queue normalQueue = new Queue();
            Queue poisonQueue = new Queue();
            List<UpdateMessage> processMessages = new List<UpdateMessage>();
            IListener listener = null;
            var messageManager = GetMockMessageManager(normalQueue, poisonQueue, listener);

            var config = new Mock<IAppConfiguration>();
            config.Setup(x => x.GlobalUpdateWaitTime).Returns(10);

            List<string> calledStoredProcedures = new List<string>();
            var dbManager = GetMockDatabaseManager(calledStoredProcedures);

            listener = new Listener(config.Object, dbManager.Object, messageManager.Object);
            listener.MessageNotProcessing += delegate (System.Object o, MessageProcessEventArgs e) { listener.CanProcess = false; };

            Hashtable activeDatabases = new Hashtable();
            Hashtable timestamps = new Hashtable();

            //send 1 message to fake queue
            var singleMessage = new UpdateMessage() { DatabaseID = 1, ExternalId = "1", DSN = "test", EntityTypeId = 20, Action = (int)MessageActionType.UpdatedProductNutrient, GroupID = 1, ProductID = 1, ArrivedTime = DateTime.UtcNow };
            messageManager.Object.mqSend(singleMessage, MessagePriority.High);

            singleMessage = new UpdateMessage() { DatabaseID = 2, ExternalId = "2", EntityTypeId = 20, Action = (int)MessageActionType.UpdatedProductIntolerance, GroupID = 2, ProductID = 2, ArrivedTime = DateTime.UtcNow };
            messageManager.Object.mqSend(singleMessage, MessagePriority.High);

            //check do we have message in queue
            normalQueue.Should().NotBeEmpty();
            normalQueue.Should().HaveCount(2, "single item send");

            listener.CanProcess = true;
            listener.ExecuteAsync(activeDatabases, timestamps);

            normalQueue.Should().BeEmpty();
            normalQueue.Should().HaveCount(0, "item processed");

            poisonQueue.Should().NotBeEmpty();
            poisonQueue.Should().HaveCount(1, "single poison item");
            listener.CanProcess.Should().BeFalse("poison message");

            singleMessage = new UpdateMessage() { DatabaseID = 3, ExternalId = "2", EntityTypeId = 20, Action = (int)MessageActionType.UpdatedProductNutrientInclusive, GroupID = 2, ProductID = 3, ArrivedTime = DateTime.UtcNow };
            messageManager.Object.mqSend(singleMessage, MessagePriority.High);

            singleMessage = new UpdateMessage() { DatabaseID = 4, ExternalId = "2", DSN="test", EntityTypeId = 20, Action = (int)MessageActionType.UpdatedProductIntolerance, GroupID = 2, ProductID = 4, ArrivedTime = DateTime.UtcNow };
            messageManager.Object.mqSend(singleMessage, MessagePriority.High);

            listener.CanProcess = true;
            listener.ExecuteAsync(activeDatabases, timestamps);

            normalQueue.Should().NotBeEmpty();
            normalQueue.Should().HaveCount(1, "single item remain");

            poisonQueue.Should().NotBeEmpty();
            poisonQueue.Should().HaveCount(2, "multiple poison items");

            listener.CanProcess.Should().BeFalse("poison message");
            listener.CanProcess = true;

            listener.ExecuteAsync(activeDatabases, timestamps);

            normalQueue.Should().BeEmpty();
            normalQueue.Should().HaveCount(0, "items processed");

            poisonQueue.Should().NotBeEmpty();
            poisonQueue.Should().HaveCount(2, "multiple poison items");



        }

        [Fact(DisplayName = "MSMQ Listener do not process GlobalUpdate messages")]
        [Trait("MSMQ Listener", "ProcessGlobalUpdateMessageNoProcessing")]
        public void ProcessGlobalUpdateMessageNoProcessing()
        {
            Queue normalQueue = new Queue();
            Queue poisonQueue = new Queue();
            List<UpdateMessage> nonProcessMessages = new List<UpdateMessage>();
            List<UpdateMessage> processMessages = new List<UpdateMessage>();
            IListener listener = null;
            var messageManager = GetMockMessageManager(normalQueue, poisonQueue, listener);
            
            var config = new Mock<IAppConfiguration>();
            config.Setup(x => x.GlobalUpdateWaitTime).Returns(10);

            List<string> calledStoredProcedures = new List<string>();
            var dbManager = GetMockDatabaseManager(calledStoredProcedures);

            listener = new Listener(config.Object, dbManager.Object, messageManager.Object);
            listener.MessageNotProcessing += delegate (System.Object o, MessageProcessEventArgs e)
            { listener.CanProcess = false; };

            Hashtable activeDatabases = new Hashtable();
            Hashtable timestamps = new Hashtable();
            int databaseId = 1;
            timestamps.Add(databaseId, DateTime.UtcNow);

            //send 1 message to fake queue
            var singleMessage = new UpdateMessage() { DatabaseID = databaseId, ExternalId = "1", EntityTypeId = 20, Action = (int)MessageActionType.GlobalUpdate, GroupID = 1, ProductID = 1, ArrivedTime = DateTime.UtcNow };
            messageManager.Object.mqSend(singleMessage, MessagePriority.High);

            //check do we have message in queue
            normalQueue.Should().NotBeEmpty();
            normalQueue.Should().HaveCount(1, "single item send");

            listener.CanProcess = true;
            listener.ExecuteAsync(activeDatabases, timestamps);

            normalQueue.Should().BeEmpty();
            calledStoredProcedures.Should().BeEmpty();
        }

        [Fact(DisplayName = "MSMQ Listener process GlobalUpdate messages")]
        [Trait("MSMQ Listener", "ProcessGlobalUpdateMessageProcessing")]
        public void ProcessGlobalUpdateMessageProcessing()
        {
            
            Queue normalQueue = new Queue();
            Queue poisonQueue = new Queue();
            IListener listener = null;
            var messageManager = GetMockMessageManager(normalQueue, poisonQueue, listener);
            
            var config = new Mock<IAppConfiguration>();
            config.Setup(x => x.GlobalUpdateWaitTime).Returns(10);

            List<string> calledStoredProcedures = new List<string>();
            var dbManager = GetMockDatabaseManager(calledStoredProcedures);

            var ml = new Mock<Listener>(new object[] { config.Object, dbManager.Object, messageManager.Object });
            var ipe = new Mock<IPriceEngine>();
            ipe.Setup(x => x.IsEngineEnabled()).Returns(Task.FromResult(false));

            ml.Setup(x => x.GetPriceEngine(It.IsAny<string>())).Returns(ipe.Object);

            ml.SetupSequence(x => x.CanProcess).Returns(true).Returns(false);
            listener = ml.Object;

            Hashtable activeDatabases = new Hashtable();
            Hashtable timestamps = new Hashtable();
            int databaseId = 1;

            //send 1 message to fake queue
            var singleMessage = new UpdateMessage() { DatabaseID = databaseId, ExternalId = "1", DSN = "testdb", EntityTypeId = 20, Action = (int)MessageActionType.GlobalUpdate, GroupID = 1, ProductID = 1, ArrivedTime = DateTime.UtcNow };
            messageManager.Object.mqSend(singleMessage, MessagePriority.High);

            //check do we have message in queue
            normalQueue.Should().NotBeEmpty();
            normalQueue.Should().HaveCount(1, "single item send");

            listener.ExecuteAsync(activeDatabases, timestamps);
           
            normalQueue.Should().BeEmpty();

            calledStoredProcedures.Should().NotBeEmpty();
            calledStoredProcedures.Should().HaveCount(1);
            calledStoredProcedures.First().Should().Be("sc_calculate_dish_pricing");
        }

        [Fact(DisplayName = "MSMQ Listener process GlobalUpdate messages new engine")]
        [Trait("MSMQ Listener", "ProcessGlobalUpdateMessageProcessing")]
        public void ProcessGlobalUpdateMessageProcessingNewEngine()
        {

            Queue normalQueue = new Queue();
            Queue poisonQueue = new Queue();
            IListener listener = null;
            var messageManager = GetMockMessageManager(normalQueue, poisonQueue, listener);

            var config = new Mock<IAppConfiguration>();
            config.Setup(x => x.GlobalUpdateWaitTime).Returns(10);

            List<string> calledStoredProcedures = new List<string>();
            var dbManager = GetMockDatabaseManager(calledStoredProcedures);

            bool globalPriceRecalcExec = false;
            bool productPriceRecalcExec = false;
            int productPriceRecalcExecId = 0;

            var ml = new Mock<Listener>(new object[] { config.Object, dbManager.Object, messageManager.Object });
            var ipe = new Mock<IPriceEngine>();
            ipe.Setup(x => x.IsEngineEnabled()).Returns(Task.FromResult(true));
            ipe.Setup(x => x.GlobalRecalculation(It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime?>())).Returns(Task.FromResult(new List<Common.Model.DbPrice>().AsEnumerable())).Callback((bool store, int groupid, int pbandid, int psetid, int unitid, DateTime? arriveTime) => { globalPriceRecalcExec = true; });
            ipe.Setup(x => x.Recalculation(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<DateTime?>())).Returns(Task.FromResult(new List<Common.Model.DbPrice>().AsEnumerable())).Callback((int productId, int groupid, int pbandid, int psetid, int unitid, bool store, DateTime? arriveTime) => { productPriceRecalcExec = true; productPriceRecalcExecId = productId; });
            ipe.Setup(x => x.GlobalRecalculation(It.IsAny<bool>(), It.IsAny<DateTime?>())).Returns(Task.FromResult(new List<Common.Model.DbPrice>().AsEnumerable())).Callback((bool store, DateTime? arriveTime) => { globalPriceRecalcExec = true; });
            ipe.Setup(x => x.Recalculation(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<DateTime?>())).Returns(Task.FromResult(new List<Common.Model.DbPrice>().AsEnumerable())).Callback((int productId, bool store, DateTime? arriveTime) => { productPriceRecalcExec = true; productPriceRecalcExecId = productId; });

            ml.Setup(x => x.GetPriceEngine(It.IsAny<string>())).Returns(ipe.Object);

            ml.SetupSequence(x => x.CanProcess).Returns(true).Returns(false).Returns(true).Returns(false);
            listener = ml.Object;

            Hashtable activeDatabases = new Hashtable();
            Hashtable timestamps = new Hashtable();
            int databaseId = 1;

            //send 1 message to fake queue
            var singleMessage = new UpdateMessage() { DatabaseID = databaseId, ExternalId = "1", DSN = "testdb", EntityTypeId = 20, Action = (int)MessageActionType.GlobalUpdate, GroupID = 1, ProductID = 1, ArrivedTime = DateTime.UtcNow };
            messageManager.Object.mqSend(singleMessage, MessagePriority.High);

            //check do we have message in queue
            normalQueue.Should().NotBeEmpty();
            normalQueue.Should().HaveCount(1, "single item send");

            listener.ExecuteAsync(activeDatabases, timestamps);

            normalQueue.Should().BeEmpty();

            globalPriceRecalcExec.Should().BeTrue();

            calledStoredProcedures.Should().BeEmpty();

            //send 1 message to fake queue
            singleMessage = new UpdateMessage() { DatabaseID = databaseId, ExternalId = "1", DSN = "testdb", EntityTypeId = 20, Action = (int)MessageActionType.UpdatedProductCost, GroupID = 1, ProductID = 1, ArrivedTime = DateTime.UtcNow };
            messageManager.Object.mqSend(singleMessage, MessagePriority.High);

            //check do we have message in queue
            normalQueue.Should().NotBeEmpty();
            normalQueue.Should().HaveCount(1, "single item send");
            listener = ml.Object;
            listener.ExecuteAsync(activeDatabases, timestamps);

            normalQueue.Should().BeEmpty();

            productPriceRecalcExec.Should().BeTrue();
            productPriceRecalcExecId.Should().Be(1);

            calledStoredProcedures.Should().BeEmpty();
        }

        [Theory]
        [InlineData("", "")]
        [InlineData("test", "")]
        [InlineData("test;data;", "")]
        [InlineData("test=;data;", "")]
        [InlineData("initial =;data;", "")]
        [InlineData("initial catalog=;data;", "")]
        [InlineData("initial catalog=scnet_qa;data;", "scnet_qa")]
        [Trait("MSMQ Listener", "ProcessPoisonMesssag1e")]
        public void GetCustomerByDsn(string dsn, string expectedResult)
        {
            var messageManager = new Mock<IMessageManager>();
            var config = new Mock<IAppConfiguration>();
            var dbManager = new Mock<IDatabaseManager>();

            Listener listener = new Listener(config.Object, dbManager.Object, messageManager.Object);

            var result = listener.GetCustomerFromDsn(dsn);
            Assert.Equal(expectedResult, result);
        }

        [Fact(DisplayName = "MSMQ Listener process message to poison queue")]
        [Trait("MSMQ Listener", "ProcessPoisonMesssage")]
        public void ProcessPoisonMesssage()
        {
            Queue normalQueue = new Queue();
            Queue poisonQueue = new Queue();
            List<UpdateMessage> processMessages = new List<UpdateMessage>();
            IListener listener = null;
            var messageManager = GetMockMessageManager(normalQueue, poisonQueue, listener);

            var config = new Mock<IAppConfiguration>();
            config.Setup(x => x.GlobalUpdateWaitTime).Returns(10);
            config.Setup(x => x.SendPoisonMessageNotification).Returns(false);

            List<string> calledStoredProcedures = new List<string>();
            var dbManager = GetMockDatabaseManager(calledStoredProcedures);
            dbManager.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SqlParameter[]>())).Callback((string connectionString, string spName, int timeout, SqlParameter[] parameters) => { calledStoredProcedures.Add(spName); }).Returns(() => { throw new Exception("test"); });

            listener = new Listener(config.Object, dbManager.Object, messageManager.Object);
            listener.MessageNotProcessing += delegate (System.Object o, MessageProcessEventArgs e)
            { listener.CanProcess = false; };

            Hashtable activeDatabases = new Hashtable();
            Hashtable timestamps = new Hashtable();
            int databaseId = 1;

            //send 1 message to fake queue
            var singleMessage = new UpdateMessage() { DatabaseID = databaseId, ExternalId = "1", DSN = "testdb",EntityTypeId = 20, Action = (int)MessageActionType.GlobalUpdate, GroupID = 1, ProductID = 1, ArrivedTime = DateTime.UtcNow };
            messageManager.Object.mqSend(singleMessage, MessagePriority.High);

            //check do we have message in queue
            normalQueue.Should().NotBeEmpty();
            normalQueue.Should().HaveCount(1, "single item send");

            listener.CanProcess = true;
            listener.ExecuteAsync(activeDatabases, timestamps);

            normalQueue.Should().BeEmpty();
            poisonQueue.Should().NotBeEmpty();
        }

        [Fact(DisplayName = "MSMQ Listener do not process messages")]
        [Trait("MSMQ Listener", "DisableProcessMessage")]
        public void DisableProcessMessage()
        {
            
            Queue normalQueue = new Queue();
            Queue poisonQueue = new Queue();
            List<UpdateMessage> processMessages = new List<UpdateMessage>();
            IListener listener = null;
            var messageManager = GetMockMessageManager(normalQueue, poisonQueue, listener);
            
            var config = new Mock<IAppConfiguration>();
            config.Setup(x => x.GlobalUpdateWaitTime).Returns(10);
            config.Setup(x => x.SendPoisonMessageNotification).Returns(false);

            List<string> calledStoredProcedures = new List<string>();
            var dbManager = GetMockDatabaseManager(calledStoredProcedures);

            listener = new Listener(config.Object, dbManager.Object, messageManager.Object);

            Hashtable activeDatabases = new Hashtable();
            Hashtable timestamps = new Hashtable();
            int databaseId = 1;

            //send 1 message to fake queue
            var singleMessage = new UpdateMessage() { DatabaseID = databaseId, ExternalId = "1", EntityTypeId = 20, Action = (int)MessageActionType.GlobalUpdate, GroupID = 1, ProductID = 1, ArrivedTime = DateTime.UtcNow };
            messageManager.Object.mqSend(singleMessage, MessagePriority.High);

            //check do we have message in queue
            normalQueue.Should().NotBeEmpty();
            normalQueue.Should().HaveCount(1, "single item send");

            listener.CanProcess = false;
            listener.ExecuteAsync(activeDatabases, timestamps);

            normalQueue.Should().NotBeEmpty();
            poisonQueue.Should().BeEmpty();
        }

        [Fact(DisplayName = "MSMQ Listener do not process messages - parallel status")]
        [Trait("MSMQ Listener", "DisableProcessMessage")]
        public void ParallelProcessMessage()
        {

            Queue normalQueue = new Queue();
            Queue poisonQueue = new Queue();
            IListener listener = null;
            var messageManager = GetMockMessageManager(normalQueue, poisonQueue, listener);

            var config = new Mock<IAppConfiguration>();
            config.Setup(x => x.GlobalUpdateWaitTime).Returns(10);
            config.Setup(x => x.SendPoisonMessageNotification).Returns(false);

            List<string> calledStoredProcedures = new List<string>();
            var dbManager = GetMockDatabaseManager(calledStoredProcedures);

            MessageProcessEventArgs result = null;
            listener = new Listener(config.Object, dbManager.Object, messageManager.Object);
            listener.MessageNotProcessing += delegate (System.Object o, MessageProcessEventArgs e)
            { result = e; listener.CanProcess = false; };

            Hashtable activeDatabases = new Hashtable();
            Hashtable timestamps = new Hashtable();
            int databaseId = 1;

            //send 1 message to fake queue
            var singleMessage = new UpdateMessage() { DatabaseID = databaseId, ExternalId = "1", EntityTypeId = 20, Action = (int)MessageActionType.GlobalUpdate, GroupID = 1, ProductID = 1, ArrivedTime = DateTime.UtcNow };
            messageManager.Object.mqSend(singleMessage, MessagePriority.High);

            //check do we have message in queue
            normalQueue.Should().NotBeEmpty();
            normalQueue.Should().HaveCount(1, "single item send");
            activeDatabases.Add(databaseId, DateTime.UtcNow);
            listener.ExecuteAsync(activeDatabases, timestamps);

            normalQueue.Should().NotBeEmpty();
            normalQueue.Should().HaveCount(1, "single item send");
            poisonQueue.Should().BeEmpty();
            result.Should().NotBeNull();
            result.Status.Should().BeEquivalentTo<MessageProcessStatus>(MessageProcessStatus.ParallelDatabaseId);
        }

        [Fact(DisplayName = "MSMQ Listener no message for processing")]
        [Trait("MSMQ Listener", "DisableProcessMessage")]
        public void NoMessageProcesing()
        {

            Queue normalQueue = new Queue();
            Queue poisonQueue = new Queue();
            IListener listener = null;
            var messageManager = GetMockMessageManager(normalQueue, poisonQueue, listener);

            var config = new Mock<IAppConfiguration>();
            config.Setup(x => x.GlobalUpdateWaitTime).Returns(10);
            config.Setup(x => x.SendPoisonMessageNotification).Returns(false);

            List<string> calledStoredProcedures = new List<string>();
            var dbManager = GetMockDatabaseManager(calledStoredProcedures);

            MessageProcessEventArgs result = null;
            listener = new Listener(config.Object, dbManager.Object, messageManager.Object);
            listener.MessageNotProcessing += delegate (System.Object o, MessageProcessEventArgs e)
            { result = e; listener.CanProcess = false; };

            Hashtable activeDatabases = new Hashtable();
            Hashtable timestamps = new Hashtable();
            
            //act
            listener.ExecuteAsync(activeDatabases, timestamps);
            
            //assert
            result.Should().NotBeNull();
            result.Status.Should().BeEquivalentTo<MessageProcessStatus>(MessageProcessStatus.NoMessage);
        }

        protected Mock<IMessageManager> GetMockMessageManager(Queue normalQueue, Queue poisonQueue, IListener listener)
        {
            var messageManager = new Mock<IMessageManager>();
            messageManager.Setup(x => x.mqSend(It.IsAny<UpdateMessage>(), It.IsAny<MessagePriority>())).Callback((UpdateMessage msg, MessagePriority priority) => {
                var mockMsg = new Mock<Message>(new object[] { msg });
                normalQueue.Enqueue(mockMsg.Object);
            });

            messageManager.Setup(x => x.mqSendToPoisonQueue(It.IsAny<UpdateMessage>(), It.IsAny<MessagePriority>())).Callback((UpdateMessage msg, MessagePriority priority) => {
                var mockMsg = new Mock<Message>(new object[] { msg });
                poisonQueue.Enqueue(mockMsg.Object);
            });

            messageManager.Setup(x => x.mqPeek(It.IsAny<TimeSpan>())).Returns(() => {
                if (normalQueue.Count == 0)
                {
                    if (listener != null)
                    {
                        listener.CanProcess = false;
                    }

                    return null;
                }

                return (Message)normalQueue.Peek();
            });
            messageManager.Setup(x => x.mqReceive(It.IsAny<string>(), It.IsAny<TimeSpan>())).Returns(() => { return (Message)normalQueue.Dequeue(); });
            return messageManager;
        }

        protected Mock<IDatabaseManager> GetMockDatabaseManager(List<string> calledStoredProcedures)
        {
            var dbManager = new Mock<IDatabaseManager>();
            dbManager.Setup(x => x.GetImportSettings(It.IsAny<string>(), It.IsAny<int>())).Returns(new Dictionary<string, ImportTypeSettings>());
            dbManager.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SqlParameter[]>())).Callback((string connectionString, string spName, SqlParameter[] parameters) => { calledStoredProcedures.Add(spName); }).Returns(1);
            dbManager.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SqlParameter[]>())).Callback((string connectionString, string spName, int timeout, SqlParameter[] parameters) => { calledStoredProcedures.Add(spName); }).Returns(1);
            return dbManager;
        }
    }
}