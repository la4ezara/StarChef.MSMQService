using System;
using System.Collections;
using System.Threading.Tasks;

namespace StarChef.MSMQService
{
    public interface IListener {
        void Execute(Hashtable activeDatabases, Hashtable globalUpdateTimeStamps);
        bool CanProcess { get; set; }
        bool IsProcessing { get; }

        event EventHandler<MessageProcessEventArgs> MessageProcessing;
        event EventHandler<MessageProcessEventArgs> MessageProcessed;
        event EventHandler<MessageProcessEventArgs> MessageNotProcessing;
    }

    
}