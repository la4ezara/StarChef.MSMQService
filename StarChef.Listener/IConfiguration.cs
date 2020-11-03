using System.Collections.Generic;

namespace StarChef.Listener
{
    public interface IConfiguration
    {
        int PriceBandBatchSize { get; }
        Dictionary<string, string> UserDefaults { get; }
    }
}