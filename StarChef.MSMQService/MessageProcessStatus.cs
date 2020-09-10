using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarChef.MSMQService
{
    public enum MessageProcessStatus
    {
        None = 0,
        Success =1,
        Processing = 2,
        NoMessage =3,
        ParallelDatabaseId = 4,
        SkipAndLost = 5,
        Invalid = 6
    }
}
