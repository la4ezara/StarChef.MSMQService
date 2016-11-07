using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StarChef.Orchestrate.Models.TransferObjects;

namespace StarChef.Listener.Tests.Handlers.Fakes
{
    class TestMessagingLogger : IMessagingLogger
    {
        public bool IsInvalidModelRegistered { get; private set; } = false;
        public bool IsCalledAnyMethod { get; private set; } = false;
        public bool IsSuccessfulOperationRegistered { get; private set; } = false;
        public bool IsFailedMessageRegistered { get; private set; } = false;
        public Task RegisterFailedMessage(OperationFailedTransferObject operationFailed, string trackingId)
        {
            IsFailedMessageRegistered =
                IsCalledAnyMethod = true;
            return Task.CompletedTask;
        }

        public Task RegisterInvalidModel(string error, object payload, string trackingId)
        {
            IsInvalidModelRegistered =
                IsCalledAnyMethod = true;
            return Task.CompletedTask;
        }

        public Task RegisterSuccess(object payload, string trackingId)
        {
            IsSuccessfulOperationRegistered =
                IsCalledAnyMethod = true;
            return Task.CompletedTask;
        }
    }
}
