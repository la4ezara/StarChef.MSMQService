using System;
using System.Collections;
using System.Threading.Tasks;

namespace StarChef.MSMQService
{
    public interface IListener {
        Task ExecuteAsync(Hashtable activeDatabases, Hashtable globalUpdateTimeStamps);
        bool CanProcess { get; set; }
        bool IsProcessing { get; }

        event EventHandler<MessageProcessEventArgs> MessageProcessing;
        event EventHandler<MessageProcessEventArgs> MessageProcessed;
        event EventHandler<MessageProcessEventArgs> MessageNotProcessing;
    }

    
}