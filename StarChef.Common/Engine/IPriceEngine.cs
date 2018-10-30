using StarChef.Common.Model;
using System.Collections.Generic;

namespace StarChef.Common.Engine
{
    public interface IPriceEngine
    {
        IEnumerable<DbPrice> ComparePrices(IEnumerable<DbPrice> existingPrices, IEnumerable<DbPrice> newPrices);

        IEnumerable<DbPrice> GlobalRecalculation();
    }
}