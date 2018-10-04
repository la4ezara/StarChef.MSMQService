using StarChef.Common.Model;
using StarChef.Common.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Fourth.StarChef.Invariables.Constants;

namespace StarChef.Common.Engine
{
    
    public class PriceEngine : IPriceEngine
    {
        readonly IPricingRepository _pricingRepo;

        //TODO:: Implement constructor which create instance for specific customer database
        public PriceEngine(IPricingRepository pricingRepo) {
            _pricingRepo = pricingRepo;
        }

        public void CalculatePrices(string connectionString, int groupId, int productId, int pbandId, int unitId, int psetId, DateTime messageArriveTime)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Parts> GetParts(IEnumerable<GroupProducts> groupProducts)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ProductPrice> GetPrices(IEnumerable<GroupProducts> groupProductPrice, IEnumerable<Parts> parts, IEnumerable<ProductItem> tmp_product, IEnumerable<ProductPartItem> tmp_product_part, IEnumerable<DishItem> tmp_dish, IEnumerable<IngredientItem> tmp_ingredient, IEnumerable<GroupProducts> existingGroupProductPrices)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// #parts
    /// </summary>
    public class Parts {
        public int EndDishId { get; set; }
        public int DishId { get; set; }
        public int IngredientId { get; set; }
        public float Quantity { get; set; }
        public int UnitId { get; set; }
        public int Level { get; set; }
        public int Processed { get; set; }
        public int PartId { get; set; }
        public decimal Ratio { get; set; }
        public RecipeType RecipeTypeId { get; set; }
        public bool IsChoise { get; set; }
    }

    /// <summary>
    /// #pricing
    /// </summary>
    public class ProductPrice {
        public int EndDishId { get; set; }
        public int ProductId { get; set; }
        public int GroupId { get; set; }
        public decimal ApPrice { get; set; }
        public decimal EpPrice { get; set; }
    }
}