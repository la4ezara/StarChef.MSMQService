﻿using FluentAssertions;
using Moq;
using StarChef.Common;
using StarChef.Common.Types;
using StarChef.MSMQService;
using StarChef.MSMQService.Configuration.Impl;
using StarChef.Orchestrate;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Messaging;
using Xunit;
using static Fourth.StarChef.Invariables.Constants;

namespace StarChef.MsmqService.IntegrationTests
{

    public class MsmqIntegrationTests
    {
        [Theory]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(10000)]
        public void ProcessRealQueueMessages(int maxMessageCount)
        {
            var halfQueueSize = (int)(maxMessageCount / 2);
            var config = new AppConfiguration();

            //clear normal queue
            var queue = new MessageQueue(config.NormalQueueName);
            queue.Purge();

            var messageManager = new MsmqManager(config.NormalQueueName, config.PoisonQueueName);
            List<UpdateMessage> processMessages = new List<UpdateMessage>();
            IListener listener = null;

            List<string> calledStoredProcedures = new List<string>();

            //mock bd manager and stop processing of messages when half and all messages are read. Simulate start/pause/continue
            var dbManager = new Mock<IDatabaseManager>();
            dbManager.Setup(x => x.GetImportSettings(It.IsAny<string>(), It.IsAny<int>())).Returns(new Dictionary<string, ImportTypeSettings>());
            dbManager.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SqlParameter[]>())).Callback((string connectionString, string spName, SqlParameter[] parameters) =>
            {
                calledStoredProcedures.Add(spName);
                if (halfQueueSize == calledStoredProcedures.Count || maxMessageCount == calledStoredProcedures.Count)
                {
                    listener.CanProcess = false;
                }
            }).Returns(1);
            dbManager.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<SqlParameter[]>())).Callback((string connectionString, string spName, int timeout, SqlParameter[] parameters) =>
            {
                calledStoredProcedures.Add(spName);
                if (halfQueueSize == calledStoredProcedures.Count || maxMessageCount == calledStoredProcedures.Count)
                {
                    listener.CanProcess = false;
                }
            }).Returns(1);

            var sender = new Mock<IStarChefMessageSender>();
            listener = new Listener(config, sender.Object, dbManager.Object, messageManager);

            Hashtable activeDatabases = new Hashtable();
            Hashtable timestamps = new Hashtable();

            for (int i = 0; i < maxMessageCount; i++)
            {
                var singleMessage = new UpdateMessage() { DatabaseID = 1, ExternalId = "1", EntityTypeId = 20, Action = (int)MessageActionType.UpdatedProductCost, GroupID = 1, ProductID = i, ArrivedTime = DateTime.UtcNow };
                messageManager.mqSend(singleMessage, MessagePriority.Normal);
            }

            //try process half messages
            listener.CanProcess = true;
            listener.ExecuteAsync(activeDatabases, timestamps);

            var items = queue.GetAllMessages();
            items.Should().NotBeEmpty();
            items.Length.ShouldBeEquivalentTo<int>(maxMessageCount - halfQueueSize);
            calledStoredProcedures.Count.ShouldBeEquivalentTo<int>(halfQueueSize);

            //try process another half
            listener.CanProcess = true;
            listener.ExecuteAsync(activeDatabases, timestamps);

            items = queue.GetAllMessages();
            items.Should().BeEmpty();
            calledStoredProcedures.Count.ShouldBeEquivalentTo<int>(maxMessageCount);
        }
    }
}