using StarChef.Common.Model;
using System.Collections.Generic;

namespace StarChef.Common.Engine
{
    public interface IPriceEngine
    {
        IEnumerable<DbPrice> CalculatePrices(int groupId, int productId, int pbandId, int unitId, int psetId);

        IEnumerable<Parts> GetParts(IEnumerable<GroupProducts> groupProducts, IEnumerable<ProductItem> tmp_product, IEnumerable<ProductPartItem> tmp_product_part);

        IEnumerable<Parts> GetParts(IEnumerable<ProductItem> tmp_product, IEnumerable<ProductPartItem> tmp_product_part, int productId, int unitId, int psetId, int pbandId);

        IEnumerable<DbPrice> GetPrices(IEnumerable<GroupProducts> groupProductPrice, List<Parts> parts, IEnumerable<ProductItem> tmp_product, IEnumerable<ProductPartItem> tmp_product_part, IEnumerable<DishItem> tmp_dish, IEnumerable<IngredientItem> tmp_ingredient, IEnumerable<DbPrice> existingGroupProductPrices, bool globalRecalc);

        IEnumerable<DbPrice> ComparePrices(IEnumerable<DbPrice> existingPrices, IEnumerable<DbPrice> newPrices);
    }
}
