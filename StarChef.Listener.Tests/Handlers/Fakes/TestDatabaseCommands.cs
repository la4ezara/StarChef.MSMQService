using System;
using System.Threading.Tasks;
using System.Xml;
using StarChef.Listener.Commands;
using StarChef.Orchestrate.Models.TransferObjects;

namespace StarChef.Listener.Tests.Handlers.Fakes
{
    [Obsolete("User Moq instead of this class")]
    internal class TestDatabaseCommands : IDatabaseCommands
    {
        public bool IsCalledAnyMethod { get; private set; } = false;
        public bool IsExternalIdUpdated { get; private set; } = false;
        public bool IsUserUpdated { get; private set; } = false;
        public bool IsUserCreated { get; private set; } = false;
        public bool IsUserDisabled { get; private set; } = false;
        public bool IsUserEnabled { get; private set; } = false;

        public Task DisableLogin(int? loginId = default(int?), string externalLoginId = null)
        {
            IsUserDisabled =
                IsCalledAnyMethod = true;
            return Task.CompletedTask;
        }

        public Task EnableLogin(int? loginId = default(int?), string externalLoginId = null)
        {
            IsUserEnabled =
                IsCalledAnyMethod = true;
            return Task.CompletedTask;
        }

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

        public Task UpdateUser(string externalLoginId, string username, string firstName, string lastName, string emailAddress)
        {
            IsUserUpdated =
                IsCalledAnyMethod = true;
            return Task.CompletedTask;
        }

        public Task<int> AddUser(AccountCreatedTransferObject user)
        {
            IsUserCreated =
               IsCalledAnyMethod = true;
            return Task.FromResult(0);
        }

        public Task<Tuple<int, int, int, int>> GetUserId(int loginId)
        {
            IsCalledAnyMethod = true;
            return Task.FromResult(new Tuple<int, int, int, int>(1, 1, 1, 1));
        }

        public Task<Tuple<int, int, string>> GetLoginUserIdAndCustomerDb(int loginId)
        {
            IsCalledAnyMethod = true;
            return Task.FromResult(new Tuple<int, int, string>(1,1,"test"));
        }

        public Task<bool> IsEventEnabledForOrganization(string eventTypeShortName, Guid organizationId)
        {
            IsCalledAnyMethod = true;
            return Task.FromResult(true);
        }

        public Task<bool> IsEventEnabledForOrganization(string eventTypeShortName, int loginId)
        {
            IsCalledAnyMethod = true;
            return Task.FromResult(true);
        }

        public Task<bool> IsEventEnabledForOrganization(string eventTypeShortName, string externalId)
        {
            IsCalledAnyMethod = true;
            return Task.FromResult(true);
        }

        public Task<bool> IsUserExists(int? loginId = default(int?), string externalLoginId = null, string username = null)
        {
            IsCalledAnyMethod = true;
            return Task.FromResult(true);
        }
    }
}