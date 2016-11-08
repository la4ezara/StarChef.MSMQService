﻿using System;
using System.Threading.Tasks;
using System.Xml;
using StarChef.Listener.Commands;
using StarChef.Orchestrate.Models.TransferObjects;

namespace StarChef.Listener.Tests.Handlers.Fakes
{
    internal class TestDatabaseCommands : IDatabaseCommands
    {
        public bool IsCalledAnyMethod { get; private set; } = false;
        public bool IsExternalIdUpdated { get; private set; } = false;
        public bool IsUserUpdated { get; private set; } = false;

        public Task RecordMessagingEvent(string trackingId, bool isSuccessful, string code, string details = null, string payloadJson = null)
        {
            IsCalledAnyMethod = true;
            return Task.CompletedTask;
        }

        public Task SavePriceBandData(Guid organisationId, XmlDocument xmlDoc)
        {
            IsCalledAnyMethod = true;
            return Task.CompletedTask;
        }

        public Task UpdateExternalId(AccountCreatedTransferObject user)
        {
            IsExternalIdUpdated =
                IsCalledAnyMethod = true;
            return Task.CompletedTask;
        }

        public Task UpdateUser(string extrenalLoginId, string username, string firstName, string lastName, string emailAddress)
        {
            IsUserUpdated =
                IsCalledAnyMethod = true;
            return Task.CompletedTask;
        }
    }
}