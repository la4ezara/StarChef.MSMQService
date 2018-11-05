using StarChef.Common.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StarChef.Common.Engine
{
    public interface IPriceEngine
    {
        IEnumerable<DbPrice> ComparePrices(IEnumerable<DbPrice> existingPrices, IEnumerable<DbPrice> newPrices);

        Task<IEnumerable<DbPrice>> GlobalRecalculation();
    }
}