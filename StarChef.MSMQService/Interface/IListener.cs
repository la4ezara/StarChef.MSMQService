using System.Collections;
using System.Threading.Tasks;

namespace StarChef.MSMQService
{
    public interface IListener {
        Task ExecuteAsync(Hashtable activeDatabases, Hashtable globalUpdateTimeStamps);

        bool CanProcess { get; set; }
        bool IsProcessing { get; }
    }
}