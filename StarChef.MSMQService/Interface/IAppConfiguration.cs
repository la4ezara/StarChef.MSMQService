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
        string NormalQueueName { get; }
        string Subject { get; }
        string ToAddress { get; }
        int GlobalUpdateWaitTime { get; }
        string PoisonQueueName { get; }
        bool SendPoisonMessageNotification { get; }
    }
}
