using StarChef.Common.Model;
using System.Collections.Generic;

namespace StarChef.Common.Repository
{
    public interface IPricingRepository
    {
        /// <summary>
        /// upfast_sc_GetAvailableProductsWithIngredientPrices2 from sc_calculate_dish_pricing
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="productId"></param>
        /// <param name="psetId"></param>
        /// <param name="pbandId"></param>
        /// <param name="unitId"></param>
        /// <returns></returns>
        IEnumerable<GroupProducts> GetGroupProductPricesByProduct(int groupId, int productId, int psetId, int pbandId, int unitId);

        /// <summary>
        /// sc_costing_populate_group_products_full from sc_calculate_dish_pricing_v3_1
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        IEnumerable<GroupProducts> GetGroupProductPricesByGroup(int groupId);
        IEnumerable<GroupProductPriceItem> GetPrices();
        IEnumerable<ProductItem> GetProducts();
        IEnumerable<ProductPartItem> GetProductParts();
        IEnumerable<DishItem> GetDishes();
        IEnumerable<IngredientItem> GetIngredients();
        bool UpdatePrices(IEnumerable<GroupProductPriceItem> prices);
    }
}
