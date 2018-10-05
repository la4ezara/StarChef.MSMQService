using StarChef.Common.Model;
using System;
using System.Collections.Generic;

namespace StarChef.Common.Engine
{
    public interface IPriceEngine
    {
        void CalculatePrices(int groupId, int productId, int pbandId, int unitId, int psetId, DateTime messageArriveTime);

        IEnumerable<Parts> GetParts(IEnumerable<GroupProducts> groupProducts, IEnumerable<ProductItem> tmp_product, IEnumerable<ProductPartItem> tmp_product_part);

        IEnumerable<Parts> GetParts(IEnumerable<ProductItem> tmp_product, IEnumerable<ProductPartItem> tmp_product_part, int productId, int unitId, int psetId, int pbandId);

        IEnumerable<ProductPrice> GetPrices(IEnumerable<GroupProducts> groupProductPrice, List<Parts> parts, IEnumerable<ProductItem> tmp_product, IEnumerable<ProductPartItem> tmp_product_part, IEnumerable<DishItem> tmp_dish, IEnumerable<IngredientItem> tmp_ingredient, IEnumerable<GroupProducts> existingGroupProductPrices);
    }
}
