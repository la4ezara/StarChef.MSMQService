using System;
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

        public Task RecordMessagingEvent(string trackingId, OperationFailedTransferObject operationFailed)
        {
            IsCalledAnyMethod = true;
            return Task.CompletedTask;
        }

        public Task RecordMessagingEvent(string trackingId, string jsonEvent, bool isSuccessful, string details = null)
        {
            IsCalledAnyMethod = true;
            return Task.CompletedTask;
        }

        public Task SavePriceBandData(Guid organisationId, XmlDocument xmlDoc)
        {
            IsCalledAnyMethod = true;
            return Task.CompletedTask;
        }

        public Task UpdateExternalId(UserTransferObject user)
        {
            IsExternalIdUpdated =
                IsCalledAnyMethod = true;
            return Task.CompletedTask;
        }

        public Task UpdateUser(UserTransferObject user)
        {
            IsUserUpdated =
                IsCalledAnyMethod = true;
            return Task.CompletedTask;
        }
    }
}