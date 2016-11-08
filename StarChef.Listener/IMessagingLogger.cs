using System.Threading.Tasks;
using StarChef.Listener.Exceptions;
using StarChef.Orchestrate.Models.TransferObjects;

namespace StarChef.Listener
{
    public interface IMessagingLogger
    {
        /// <exception cref="ConnectionStringNotFoundException">Login connection string is not found</exception>
        /// <exception cref="DatabaseException">Error is occurred while saving data to database</exception>
        Task ReceivedInvalidModel(string trackingId, object payload, string error);
        
        /// <exception cref="ConnectionStringNotFoundException">Login connection string is not found</exception>
        /// <exception cref="DatabaseException">Error is occurred while saving data to database</exception>
        Task MessageProcessedSuccessfully(object payload, string trackingId);
        
        /// <exception cref="ConnectionStringNotFoundException">Login connection string is not found</exception>
        /// <exception cref="DatabaseException">Error is occurred while saving data to database</exception>
        Task ReceivedFailedMessage(FailedTransferObject operationFailed, string trackingId);
    }
}