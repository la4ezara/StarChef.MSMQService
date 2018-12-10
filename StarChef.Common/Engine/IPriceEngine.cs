using StarChef.Common.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StarChef.Common.Engine
{
    public interface IPriceEngine
    {
        IEnumerable<DbPrice> ComparePrices(IEnumerable<DbPrice> existingPrices, IEnumerable<DbPrice> newPrices);

        Task<IEnumerable<DbPrice>> GlobalRecalculation(bool storePrices, DateTime? arrivedTime);

        Task<bool> IsEngineEnabled();

        Task<IEnumerable<DbPrice>> Recalculation(int productId, bool storePrices, DateTime? arrivedTime);

        void GetPriceDifferences(Dictionary<int, List<DbPrice>> existingPrices_dict, ConcurrentBag<DbPrice> bag, int groupId, List<DbPrice> groupPrices);
    }
}