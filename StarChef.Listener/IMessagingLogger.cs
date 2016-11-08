using System.Threading.Tasks;
using StarChef.Orchestrate.Models.TransferObjects;

namespace StarChef.Listener
{
    public interface IMessagingLogger
    {
        Task ReceivedInvalidModel(string trackingId, object payload, string error);
        Task MessageProcessedSuccessfully(object payload, string trackingId);
        Task ReceivedFailedMessage(FailedTransferObject operationFailed, string trackingId);
    }
}