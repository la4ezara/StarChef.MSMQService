using FluentAssertions;
using Moq;
using StarChef.Common;
using StarChef.Common.Types;
using StarChef.MSMQService;
using StarChef.MSMQService.Configuration;
using StarChef.Orchestrate;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Messaging;
using Xunit;
using static Messaging.MSMQ.Enums;

namespace StarChef.MsmqService.Tests
{
    public class MsmqListenerTests
    {
        [Fact]
        [Trait("MSMQ Listener", "ProcessMultipleItems")]
        public void ProcessMultipleItems()
        {
            var messageManager = new Mock<IMessageManager>();
            Queue normalQueue = new Queue();
            Queue poisonQueue = new Queue();
            List<UpdateMessage> processMessages = new List<UpdateMessage>();
            IListener listener = null;

            messageManager.Setup(x => x.mqSend(It.IsAny<UpdateMessage>(), It.IsAny<MessagePriority>())).Callback((UpdateMessage msg, MessagePriority priority) => {
                var mockMsg = new Mock<Message>(new object[] { msg });
                normalQueue.Enqueue(mockMsg.Object); });

            messageManager.Setup(x => x.mqSendToPoisonQueue(It.IsAny<UpdateMessage>(), It.IsAny<MessagePriority>())).Callback((UpdateMessage msg, MessagePriority priority) => {
                var mockMsg = new Mock<Message>(new object[] { msg });
                mockMsg.SetupGet(a => a.ArrivedTime).Returns(DateTime.Now);
                poisonQueue.Enqueue(mockMsg.Object);
            });

            messageManager.Setup(x => x.mqPeek(It.IsAny<TimeSpan>())).Returns(() => {
                if (normalQueue.Count == 0) {
                    if (listener != null)
                    {
                        listener.CanProcess = false;
                    }

                    return null;
                }

                return (Message)normalQueue.Peek();
            });
            messageManager.Setup(x => x.mqReceive(It.IsAny<string>(), It.IsAny<TimeSpan>())).Returns(() => { return (Message)normalQueue.Dequeue(); });
            var config = new Mock<IAppConfiguration>();
            config.Setup(x => x.GlobalUpdateWaitTime).Returns(10);

            var dbManager = new Mock<IDatabaseManager>();
            dbManager.Setup(x => x.GetImportSettings(It.IsAny<string>(), It.IsAny<int>())).Returns(new Dictionary<string, ImportTypeSettings>());
            dbManager.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SqlParameter[]>())).Returns(1);
            dbManager.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SqlParameter[]>())).Returns(1);

            var sender = new Mock<IStarChefMessageSender>();
            listener = new Listener(config.Object, sender.Object, dbManager.Object, messageManager.Object);

            Hashtable activeDatabases = new Hashtable();
            Hashtable timestamps = new Hashtable();

            //send 1 message to fake queue
            var singleMessage = new UpdateMessage() { DatabaseID = 1, ExternalId = "1", EntityTypeId = 20, Action = (int)MessageActionType.UpdatedProductCost, GroupID = 1, ProductID = 1, ArrivedTime = DateTime.UtcNow };
            messageManager.Object.mqSend(singleMessage, MessagePriority.High);
            //check do we have message in queue
            normalQueue.Should().NotBeEmpty();
            normalQueue.Should().HaveCount(1, "single item send");

            singleMessage = new UpdateMessage() { DatabaseID = 2, ExternalId = "2", EntityTypeId = 20, Action = (int)MessageActionType.UpdatedProductCost, GroupID = 2, ProductID = 2, ArrivedTime = DateTime.UtcNow };
            messageManager.Object.mqSend(singleMessage, MessagePriority.High);

            //check do we have message in queue
            normalQueue.Should().NotBeEmpty();
            normalQueue.Should().HaveCount(2, "single item send");

            listener.CanProcess = true;
            listener.ExecuteAsync(activeDatabases, timestamps);

            normalQueue.Should().BeEmpty();
            normalQueue.Should().HaveCount(0, "item processed");
        }

        [Fact]
        [Trait("MSMQ Listener", "ProcessMultipleStartStops")]
        public void ProcessMultipleStartStops()
        {
            var messageManager = new Mock<IMessageManager>();
            Queue normalQueue = new Queue();
            Queue poisonQueue = new Queue();
            List<UpdateMessage> processMessages = new List<UpdateMessage>();
            IListener listener = null;

            messageManager.Setup(x => x.mqSend(It.IsAny<UpdateMessage>(), It.IsAny<MessagePriority>())).Callback((UpdateMessage msg, MessagePriority priority) => {
                var mockMsg = new Mock<Message>(new object[] { msg });
                normalQueue.Enqueue(mockMsg.Object);
            });

            messageManager.Setup(x => x.mqSendToPoisonQueue(It.IsAny<UpdateMessage>(), It.IsAny<MessagePriority>())).Callback((UpdateMessage msg, MessagePriority priority) => {
                var mockMsg = new Mock<Message>(new object[] { msg });
                mockMsg.SetupGet(a => a.ArrivedTime).Returns(DateTime.Now);
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
            var config = new Mock<IAppConfiguration>();
            config.Setup(x => x.GlobalUpdateWaitTime).Returns(10);

            var dbManager = new Mock<IDatabaseManager>();
            dbManager.Setup(x => x.GetImportSettings(It.IsAny<string>(), It.IsAny<int>())).Returns(new Dictionary<string, ImportTypeSettings>());
            dbManager.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SqlParameter[]>())).Returns(1);
            dbManager.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SqlParameter[]>())).Returns(1);

            var sender = new Mock<IStarChefMessageSender>();
            listener = new Listener(config.Object, sender.Object, dbManager.Object, messageManager.Object);

            Hashtable activeDatabases = new Hashtable();
            Hashtable timestamps = new Hashtable();

            //send 1 message to fake queue
            var singleMessage = new UpdateMessage() { DatabaseID = 1, ExternalId = "1", EntityTypeId = 20, Action = (int)MessageActionType.UpdatedProductCost, GroupID = 1, ProductID = 1, ArrivedTime = DateTime.UtcNow };
            messageManager.Object.mqSend(singleMessage, MessagePriority.High);

            singleMessage = new UpdateMessage() { DatabaseID = 2, ExternalId = "2", EntityTypeId = 20, Action = (int)MessageActionType.UpdatedProductCost, GroupID = 2, ProductID = 2, ArrivedTime = DateTime.UtcNow };
            messageManager.Object.mqSend(singleMessage, MessagePriority.High);

            //check do we have message in queue
            normalQueue.Should().NotBeEmpty();
            normalQueue.Should().HaveCount(2, "single item send");

            listener.CanProcess = true;
            listener.ExecuteAsync(activeDatabases, timestamps);

            normalQueue.Should().BeEmpty();
            normalQueue.Should().HaveCount(0, "item processed");

            singleMessage = new UpdateMessage() { DatabaseID = 3, ExternalId = "2", EntityTypeId = 20, Action = (int)MessageActionType.UpdatedProductCost, GroupID = 2, ProductID = 3, ArrivedTime = DateTime.UtcNow };
            messageManager.Object.mqSend(singleMessage, MessagePriority.High);

            singleMessage = new UpdateMessage() { DatabaseID = 4, ExternalId = "2", EntityTypeId = 20, Action = (int)MessageActionType.UpdatedProductCost, GroupID = 2, ProductID = 4, ArrivedTime = DateTime.UtcNow };
            messageManager.Object.mqSend(singleMessage, MessagePriority.High);

            listener.CanProcess = true;
            listener.ExecuteAsync(activeDatabases, timestamps);

            normalQueue.Should().BeEmpty();
            normalQueue.Should().HaveCount(0, "item processed");
        }

        [Fact]
        [Trait("MSMQ Listener", "ProcessGlobalUpdateMessageNoProcessing")]
        public void ProcessGlobalUpdateMessageNoProcessing()
        {
            var messageManager = new Mock<IMessageManager>();
            Queue normalQueue = new Queue();
            Queue poisonQueue = new Queue();
            List<UpdateMessage> processMessages = new List<UpdateMessage>();
            IListener listener = null;

            messageManager.Setup(x => x.mqSend(It.IsAny<UpdateMessage>(), It.IsAny<MessagePriority>())).Callback((UpdateMessage msg, MessagePriority priority) => {
                var mockMsg = new Mock<Message>(new object[] { msg });
                normalQueue.Enqueue(mockMsg.Object);
            });

            messageManager.Setup(x => x.mqSendToPoisonQueue(It.IsAny<UpdateMessage>(), It.IsAny<MessagePriority>())).Callback((UpdateMessage msg, MessagePriority priority) => {
                var mockMsg = new Mock<Message>(new object[] { msg });
                mockMsg.SetupGet(a => a.ArrivedTime).Returns(DateTime.Now);
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
            var config = new Mock<IAppConfiguration>();
            config.Setup(x => x.GlobalUpdateWaitTime).Returns(10);

            List<string> calledStoredProcedures = new List<string>();

            var dbManager = new Mock<IDatabaseManager>();
            dbManager.Setup(x => x.GetImportSettings(It.IsAny<string>(), It.IsAny<int>())).Returns(new Dictionary<string, ImportTypeSettings>());
            dbManager.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SqlParameter[]>())).Callback((string connectionString, string spName, SqlParameter[] parameters) => { calledStoredProcedures.Add(spName); }).Returns(1);
            dbManager.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SqlParameter[]>())).Callback((string connectionString, string spName, int timeout, SqlParameter[] parameters) => { calledStoredProcedures.Add(spName); }).Returns(1);

            var sender = new Mock<IStarChefMessageSender>();
            listener = new Listener(config.Object, sender.Object, dbManager.Object, messageManager.Object);

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

        [Fact]
        [Trait("MSMQ Listener", "ProcessGlobalUpdateMessageProcessing")]
        public void ProcessGlobalUpdateMessageProcessing()
        {
            var messageManager = new Mock<IMessageManager>();
            Queue normalQueue = new Queue();
            Queue poisonQueue = new Queue();
            List<UpdateMessage> processMessages = new List<UpdateMessage>();
            IListener listener = null;

            messageManager.Setup(x => x.mqSend(It.IsAny<UpdateMessage>(), It.IsAny<MessagePriority>())).Callback((UpdateMessage msg, MessagePriority priority) => {
                var mockMsg = new Mock<Message>(new object[] { msg });
                normalQueue.Enqueue(mockMsg.Object);
            });

            messageManager.Setup(x => x.mqSendToPoisonQueue(It.IsAny<UpdateMessage>(), It.IsAny<MessagePriority>())).Callback((UpdateMessage msg, MessagePriority priority) => {
                var mockMsg = new Mock<Message>(new object[] { msg });
                mockMsg.SetupGet(a => a.ArrivedTime).Returns(DateTime.Now);
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
            var config = new Mock<IAppConfiguration>();
            config.Setup(x => x.GlobalUpdateWaitTime).Returns(10);

            List<string> calledStoredProcedures = new List<string>();

            var dbManager = new Mock<IDatabaseManager>();
            dbManager.Setup(x => x.GetImportSettings(It.IsAny<string>(), It.IsAny<int>())).Returns(new Dictionary<string, ImportTypeSettings>());
            dbManager.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SqlParameter[]>())).Callback((string connectionString, string spName, SqlParameter[] parameters) => { calledStoredProcedures.Add(spName); }).Returns(1);
            dbManager.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SqlParameter[]>())).Callback((string connectionString, string spName, int timeout, SqlParameter[] parameters) => { calledStoredProcedures.Add(spName); }).Returns(1);

            var sender = new Mock<IStarChefMessageSender>();
            listener = new Listener(config.Object, sender.Object, dbManager.Object, messageManager.Object);

            Hashtable activeDatabases = new Hashtable();
            Hashtable timestamps = new Hashtable();
            int databaseId = 1;
            //timestamps.Add(databaseId, DateTime.UtcNow);

            //send 1 message to fake queue
            var singleMessage = new UpdateMessage() { DatabaseID = databaseId, ExternalId = "1", EntityTypeId = 20, Action = (int)MessageActionType.GlobalUpdate, GroupID = 1, ProductID = 1, ArrivedTime = DateTime.UtcNow };
            messageManager.Object.mqSend(singleMessage, MessagePriority.High);

            //check do we have message in queue
            normalQueue.Should().NotBeEmpty();
            normalQueue.Should().HaveCount(1, "single item send");

            listener.CanProcess = true;
            listener.ExecuteAsync(activeDatabases, timestamps);

            normalQueue.Should().BeEmpty();

            calledStoredProcedures.Should().NotBeEmpty();
            calledStoredProcedures.Should().HaveCount(1);
            calledStoredProcedures.First().Should().Be("sc_calculate_dish_pricing");
        }

        [Fact]
        [Trait("MSMQ Listener", "ProcessPoisonMesssage")]
        public void ProcessPoisonMesssage()
        {
            var messageManager = new Mock<IMessageManager>();
            Queue normalQueue = new Queue();
            Queue poisonQueue = new Queue();
            List<UpdateMessage> processMessages = new List<UpdateMessage>();
            IListener listener = null;

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
            var config = new Mock<IAppConfiguration>();
            config.Setup(x => x.GlobalUpdateWaitTime).Returns(10);
            config.Setup(x => x.SendPoisonMessageNotification).Returns(false);

            List<string> calledStoredProcedures = new List<string>();

            var dbManager = new Mock<IDatabaseManager>();
            dbManager.Setup(x => x.GetImportSettings(It.IsAny<string>(), It.IsAny<int>())).Returns(new Dictionary<string, ImportTypeSettings>());
            dbManager.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SqlParameter[]>())).Callback((string connectionString, string spName, SqlParameter[] parameters) => { calledStoredProcedures.Add(spName); }).Returns(() => { throw new Exception("test"); return 1; });
            dbManager.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SqlParameter[]>())).Callback((string connectionString, string spName, int timeout, SqlParameter[] parameters) => { calledStoredProcedures.Add(spName); }).Returns(() => { throw new Exception("test"); return 1; });

            var sender = new Mock<IStarChefMessageSender>();
            listener = new Listener(config.Object, sender.Object, dbManager.Object, messageManager.Object);

            Hashtable activeDatabases = new Hashtable();
            Hashtable timestamps = new Hashtable();
            int databaseId = 1;

            //send 1 message to fake queue
            var singleMessage = new UpdateMessage() { DatabaseID = databaseId, ExternalId = "1", EntityTypeId = 20, Action = (int)MessageActionType.GlobalUpdate, GroupID = 1, ProductID = 1, ArrivedTime = DateTime.UtcNow };
            messageManager.Object.mqSend(singleMessage, MessagePriority.High);

            //check do we have message in queue
            normalQueue.Should().NotBeEmpty();
            normalQueue.Should().HaveCount(1, "single item send");

            listener.CanProcess = true;
            listener.ExecuteAsync(activeDatabases, timestamps);

            normalQueue.Should().BeEmpty();
            poisonQueue.Should().NotBeEmpty();
        }
    }
}
