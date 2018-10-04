using StarChef.Common.Model;
using System;
using System.Collections.Generic;

namespace StarChef.Common.Engine
{
    interface IPriceEngine
    {
        void CalculatePrices(string connectionString, int groupId, int productId, int pbandId, int unitId, int psetId, DateTime messageArriveTime);

        /// <summary>
        /// fill same as in procedure table #parts
        /// </summary>
        /// <param name="groupProducts"></param>
        /// <returns></returns>
        IEnumerable<Parts> GetParts(IEnumerable<GroupProducts> groupProducts);

        IEnumerable<ProductPrice> GetPrices(IEnumerable<GroupProducts> groupProductPrice, IEnumerable<Parts> parts, IEnumerable<ProductItem> tmp_product, IEnumerable<ProductPartItem> tmp_product_part, IEnumerable<DishItem> tmp_dish, IEnumerable<IngredientItem> tmp_ingredient, IEnumerable<GroupProducts> existingGroupProductPrices);
    }
}
