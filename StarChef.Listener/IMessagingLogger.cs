using System.Threading.Tasks;
using StarChef.Orchestrate.Models.TransferObjects;

namespace StarChef.Listener
{
    public interface IMessagingLogger
    {
        Task RegisterInvalidModel(string error, object payload, string trackingId);
        Task RegisterSuccess(object payload, string trackingId);
        Task RegisterFailedMessage(OperationFailedTransferObject operationFailed, string trackingId);
    }
}