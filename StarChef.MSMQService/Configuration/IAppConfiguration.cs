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
        int MsmqThreadCount { get; }
        string QueuePath { get; }
        string Subject { get; }
        string ToAddress { get; }
    }
}
