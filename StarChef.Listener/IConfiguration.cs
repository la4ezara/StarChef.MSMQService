using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarChef.Listener
{
    public interface IConfiguration
    {
        int PriceBandBatchSize { get; }
        Dictionary<string, string> UserDefaults { get; }
    }
}
