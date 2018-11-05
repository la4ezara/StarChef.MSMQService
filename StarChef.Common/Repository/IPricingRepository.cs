using StarChef.Common.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StarChef.Common.Repository
{
    public interface IPricingRepository
    {
        Task<IEnumerable<GroupProducts>> GetGroupProductPricesByGroup(int groupId);
        Task<IEnumerable<DbPrice>> GetPrices();
        Task<IEnumerable<DbPrice>> GetPrices(int groupId);
        Task<IEnumerable<Product>> GetProducts();
        Task<IEnumerable<ProductPart>> GetProductParts();

        bool UpdatePrices(IEnumerable<GroupProducts> prices);
    }
}
