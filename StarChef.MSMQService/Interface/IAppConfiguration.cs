using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarChef.MSMQService.Configuration
{
    public interface IAppConfiguration
    {
        string Alias { get; }
        string FromAddress { get; }
        long Interval { get; }
        string QueueName { get; }
        string Subject { get; }
        string ToAddress { get; }
        int GlobalUpdateWaitTime { get; }
        string PoisonQueue { get; }
    }
}
