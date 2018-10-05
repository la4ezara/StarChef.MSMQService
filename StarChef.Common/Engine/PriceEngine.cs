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
        private readonly IPricingRepository _pricingRepo;

        public PriceEngine(IPricingRepository pricingRepo) {
            _pricingRepo = pricingRepo;
        }

        public void CalculatePrices(int groupId, int productId, int pbandId, int unitId, int psetId, DateTime messageArriveTime)
        {
            var globalRecalc = false;

            if ((groupId > 0 && productId == 0 && pbandId == 0 && unitId == 0 && psetId == 0) ||
                (groupId == 0 && productId == 0 && pbandId > 0 && unitId == 0 && psetId == 0) ||
                (groupId == 0 && productId == 0 && pbandId == 0 && unitId == 0 && psetId == 0)
                )
            {
                globalRecalc = true;
            }

            var tmp_dish = _pricingRepo.GetDishes();
            var tmp_ingredient = _pricingRepo.GetIngredients();
            var tmp_product = _pricingRepo.GetProducts();
            var tmp_product_part = _pricingRepo.GetProductParts();

            IEnumerable<Parts> parts = null;
            //load all temp data required for all recalculations
            IEnumerable<GroupProducts> group_products = new List<GroupProducts>();
            if (globalRecalc)
            {
                group_products = _pricingRepo.GetGroupProductPricesByGroup(0);
                parts = GetParts(group_products, tmp_product, tmp_product_part);
            }
            else
            {
                group_products = _pricingRepo.GetGroupProductPricesByProduct(groupId, productId, psetId, pbandId, unitId);
                parts = GetParts(tmp_product, tmp_product_part, productId, unitId, psetId, pbandId);
            }

            var newPrices = GetPrices(group_products, parts.ToList(), tmp_product, tmp_product_part, tmp_dish, tmp_ingredient, null);
            //_pricingRepo.UpdatePrices(newPrices);
            //proceed with global price recalculation
        }

        public IEnumerable<Parts> GetParts(IEnumerable<GroupProducts> groupProducts, IEnumerable<ProductItem> tmp_product, IEnumerable<ProductPartItem> tmp_product_part)
        {
            var affectedProducts = groupProducts.Select(gp => gp.ProductId).Distinct().ToList();

            var subProductIds = tmp_product_part.Select(pp => pp.SubProductId).Distinct().ToList();

            var topLevelItems = affectedProducts.Except(subProductIds).ToList();

            var parts = (from ap in affectedProducts
                         join p in tmp_product on ap equals p.ProductId
                         where topLevelItems.Contains(ap)
                         select new Parts()
                         {
                             EndDishId = p.ProductId,
                             DishId = p.ProductId,
                             IngredientId = p.ProductId,
                             Quantity = p.Number * p.Quantity,
                             UnitId = p.UnitId,
                             Level = 1,
                             Processed = 0,
                             PartId = 0,
                             RecipeTypeId = 0,
                         }).Distinct().ToList();

            return parts;
        }

        public IEnumerable<Parts> GetParts(IEnumerable<ProductItem> tmp_product, IEnumerable<ProductPartItem> tmp_product_part, int productId, int unitId, int psetId, int pbandId)
        {
            var tmpUsageList = new List<TmpUsageItem>();

            if (productId > 0 || unitId > 0)
            {
                var tmpUsages = (from p in tmp_product
                          where (p.ProductId == productId && productId > 0) 
                          || (p.UnitId == unitId && unitId > 0)
                          select new TmpUsageItem() { IngredientId = p.ProductId }).Distinct();
                tmpUsageList.AddRange(tmpUsages);

                tmpUsages = (from pp in tmp_product_part
                                where pp.UnitId == unitId && unitId > 0
                                select new TmpUsageItem() { IngredientId = pp.SubProductId, IsChoise = pp.IsChoise }).Distinct();

                tmpUsageList.AddRange(tmpUsages);
            }

            if (psetId > 0) {
                var list = _pricingRepo.GetPsetProducts(psetId);
                tmpUsageList.AddRange(list.Select(x => new TmpUsageItem() { IngredientId = x.ProductId }));
            }

            if (pbandId > 0)
            {
                var list = _pricingRepo.GetPsetGroupProducts(pbandId);
                tmpUsageList.AddRange(list.Select(x => new TmpUsageItem() { IngredientId = x.ProductId }));
            }

            int level = 1;
            var usageList = tmpUsageList.Select(x => new UsageItem() { IngredientId = x.IngredientId, DishId = x.IngredientId, Level = level, IsChoise = x.IsChoise }).ToList();

            while (usageList.Any(x => x.Level == level && !x.IsProcessed)) {

                //set all to process = 1
                usageList.ForEach(u => u.IsProcessed = false);

                var targetItem = (from u in usageList
                                  join pp in tmp_product_part on u.DishId equals pp.SubProductId
                                  select u).Distinct().ToList();

                targetItem.ForEach(u => u.IsProcessed = true);

                var targetItem2 = (from u in usageList
                                   join pp in tmp_product_part on u.DishId equals pp.SubProductId
                                   where u.Level == level
                                  select new UsageItem() { IngredientId = u.IngredientId, DishId = pp.ProductId, Level = level + 1, IsChoise = pp.IsChoise }).Distinct(new UsageItemComparer());


                usageList = usageList.Where(u => !u.IsProcessed).ToList();
                usageList.AddRange(targetItem2);
                level++;
            }
            var parts = (from u in usageList
                         join p in tmp_product on u.DishId equals p.ProductId
                         select new Parts() {
                             EndDishId = u.DishId,
                             DishId = u.DishId,
                             IngredientId = u.DishId,
                             Quantity = p.Number * p.Quantity,
                             UnitId = p.UnitId,
                             Level = 1,
                             PartId = 0,
                             RecipeTypeId = 0,
                             IsChoise = u.IsChoise
                         }).Distinct().ToList();

            return parts;
        }

        public IEnumerable<ProductPrice> GetPrices(IEnumerable<GroupProducts> groupProductPrice, List<Parts> parts, IEnumerable<ProductItem> tmp_product, IEnumerable<ProductPartItem> tmp_product_part, IEnumerable<DishItem> tmp_dish, IEnumerable<IngredientItem> tmp_ingredient, IEnumerable<GroupProducts> existingGroupProductPrices)
        {
            var affectedProducts = groupProductPrice.Select(gp => gp.ProductId).Distinct().ToList();

            int level = 1;
            while (parts.Any(p => p.Level == level && p.Processed == 0))
            {

                var itemsToUpdate = tmp_product_part.Where(pp => affectedProducts.Contains(pp.ProductId)).Select(pp => pp.ProductId).Distinct();

                parts.Where(p => itemsToUpdate.Contains(p.IngredientId)).ToList().ForEach(p => p.Processed = 2);
                parts.Where(p => !itemsToUpdate.Contains(p.IngredientId)).ToList().ForEach(p => p.Processed = 1);

                //add next nexted level from top to bottom
                var nextLevelParts = (from p in parts
                                      join pp in tmp_product_part on p.IngredientId equals pp.ProductId
                                      where p.Level == level && p.Processed == 2
                                      select new Parts()
                                      {
                                          EndDishId = p.EndDishId,
                                          DishId = pp.ProductId,
                                          IngredientId = pp.SubProductId,
                                          Quantity = pp.Quantity,
                                          UnitId = pp.UnitId,
                                          Level = level + 1,
                                          PartId = pp.ProductPartId,
                                          IsChoise = pp.IsChoise
                                      }).Distinct().ToList();

                parts.AddRange(nextLevelParts);

                level++;
            }

            /////////////////////////////////////////////////////////////////////////
            ///delete 

            var itemsToDeleteIngredients = parts.Where(p => p.EndDishId != p.IngredientId).Select(p => p.DishId).Distinct();
            var deletePartItems = parts.Where(p => p.EndDishId == p.IngredientId && itemsToDeleteIngredients.Contains(p.IngredientId)).ToList();

            deletePartItems.ForEach(dp => parts.Remove(dp));

            /////////////////////////////////////////////////////////////////////////
            ///update recipe type
            //UPDATE #parts
            //SET recipe_type_id = dish.recipe_type_id
            //FROM #parts
            //INNER JOIN dish ON dish.product_id = #parts.dish_id

            parts.ForEach(p =>
            {
                var dish = tmp_dish.FirstOrDefault(d => d.ProductId == p.DishId);
                if (dish != null)
                {
                    p.RecipeTypeId = (int)dish.RecipeTypeId;
                }
            });

            /////////////////////////////////////////////////////////////////////////
            ///convertion

            var itemsToConvert = (from pa in parts
                                  join p in tmp_product on pa.IngredientId equals p.ProductId
                                  select new ProductConvertionRatio() { ProductId = p.ProductId, SourceUnitId = (short)p.UnitId, TargetUnitId = (short)pa.UnitId }).Distinct().ToList();

            var convertions = this._pricingRepo.GetProductConvertionRatio(itemsToConvert);

            parts.ForEach(pa =>
            {

                var convertion = (from cr in convertions
                                  join p in tmp_product on cr.ProductId equals p.ProductId
                                  where p.UnitId == cr.SourceUnitId && cr.TargetUnitId == pa.UnitId
                                  && pa.IngredientId == p.ProductId
                                  select cr).FirstOrDefault();

                if (convertion != null)
                {
                    pa.Ratio = convertion.Ratio;
                }
            });

            /////////////////////////////////////////////////////////////////////////
            ///fill pricing 3 insert statements

            List<ProductPrice> pricing = new List<ProductPrice>();

            var res = (from p in tmp_product
                       join i in tmp_ingredient on p.ProductId equals i.ProductId
                       join pa in parts on p.ProductId equals pa.IngredientId
                       join g in groupProductPrice on p.ProductId equals g.ProductId
                       where pa.Processed == 1
                       group p.ProductId 
                       by new { p.ProductId, g.GroupId, g.Price, i.Wastage } into itemPrices
                       select itemPrices
             ).ToList();

            res.ForEach(g =>
            {
                decimal epPrice = 0;
                if (g.Key.Wastage <= 100)
                {
                    epPrice = g.Key.Price * 100.0m / (100.0m - g.Key.Wastage);
                }

                ProductPrice price = new ProductPrice()
                {
                    ProductId = g.Key.ProductId,
                    GroupId = g.Key.GroupId,
                    ApPrice = g.Key.Price,
                    EpPrice = epPrice
                };
                pricing.Add(price);
            });

            var res2 = (from p in tmp_product
                        join pa in parts on p.ProductId equals pa.IngredientId
                        join g in groupProductPrice on pa.DishId equals g.ProductId
                        join dpc in existingGroupProductPrices on p.ProductId equals dpc.ProductId
                       
                       where pa.Processed == 1 && !affectedProducts.Contains(p.ProductId)
                       group p.ProductId
                       by new { p.ProductId, g.GroupId, dpc.Price } into itemPrices
                       select itemPrices
             ).ToList();

            res2.ForEach(g =>
            {
                var wastage = tmp_ingredient.FirstOrDefault(i => i.ProductId == g.Key.ProductId);
                decimal epPrice = 0;
                if (wastage != null && wastage.Wastage <= 100)
                {
                    epPrice = g.Key.Price * 100.0m / (100.0m - wastage.Wastage);
                }

                ProductPrice price = new ProductPrice()
                {
                    ProductId = g.Key.ProductId,
                    GroupId = g.Key.GroupId,
                    ApPrice = g.Key.Price,
                    EpPrice = epPrice
                };
                pricing.Add(price);
            });

            var productPartsProductIds = tmp_product_part.Select(pp => pp.ProductId).Distinct();

            var res3 = (from p in tmp_product
                        join pa in parts on p.ProductId equals pa.IngredientId
                        join g in groupProductPrice on pa.DishId equals g.ProductId
                        where pa.Processed == 1 && p.ProductTypeId == ProductType.Dish
                        && !productPartsProductIds.Contains(p.ProductId) //repsent left join

                        select new ProductPrice() { EndDishId = p.ProductId, ProductId =p.ProductId, GroupId = g.GroupId }
                        
             ).ToList();

            pricing.AddRange(res3);

            /////////////////////////////////////////////////////////////////////////
            ///delete parts

            var itemsToDelete = parts.Where(p => p.EndDishId == p.IngredientId).ToList();
            itemsToDelete.ForEach(p => parts.Remove(p));

            /////////////////////////////////////////////////////////////////////////

            int maxLevel = parts.Max(p => p.Level);

            return new List<ProductPrice>();
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
        //public RecipeType RecipeTypeId { get; set; }
        public int RecipeTypeId { get; set; }
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

    public class UsageItemComparer : IEqualityComparer<UsageItem>
    {
        public bool Equals(UsageItem x, UsageItem y)
        {
            if (x.IngredientId.Equals(y.IngredientId) && x.IsChoise.Equals(y.IsChoise)
                && x.DishId.Equals(y.DishId)
                && x.Level.Equals(y.Level)
                && x.IsProcessed.Equals(y.IsProcessed))
            {
                return true;
            }

            return false;
        }

        public int GetHashCode(UsageItem obj)
        {
            return obj.IngredientId.GetHashCode() ^ obj.IsChoise.GetHashCode() ^ obj.DishId.GetHashCode() ^ obj.Level.GetHashCode() ^ obj.IsProcessed.GetHashCode();
        }
    }

    public class UsageItem : TmpUsageItem
    {
        public int DishId { get; set; }
        public int Level { get; set; }
        public bool IsProcessed { get; set; }
    }

    public class TmpUsageItem {
        public int IngredientId { get; set; }
        public bool IsChoise { get; set; }
    }
}