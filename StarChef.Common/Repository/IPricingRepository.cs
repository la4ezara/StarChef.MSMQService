using StarChef.Common.Model;
using System.Collections.Generic;

namespace StarChef.Common.Repository
{
    public interface IPricingRepository
    {
        /// <summary>
        /// sc_costing_populate_group_products_full from sc_calculate_dish_pricing_v3_1
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        IEnumerable<GroupProducts> GetGroupProductPricesByGroup(int groupId);
        IEnumerable<DbPrice> GetPrices();
        IEnumerable<Product> GetProducts();
        IEnumerable<ProductPart> GetProductParts();

        bool UpdatePrices(IEnumerable<GroupProducts> prices);
    }
}
