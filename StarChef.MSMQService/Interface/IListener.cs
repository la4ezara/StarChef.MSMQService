using System;
using System.Collections;
using System.Threading.Tasks;

namespace StarChef.MSMQService
{
    public interface IListener {
        Task<bool> ExecuteAsync(Hashtable activeDatabases, Hashtable globalUpdateTimeStamps);
        bool CanProcess { get; set; }

        event EventHandler<MessageProcessEventArgs> MessageProcessing;
        event EventHandler<MessageProcessEventArgs> MessageProcessed;
        event EventHandler<MessageProcessEventArgs> MessageNotProcessing;
    }

    
}