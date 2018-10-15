using StarChef.Common.Model;
using StarChef.Common.Repository;
using System;
using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<int, List<ProductConvertionRatio>> _convertions_dict;
        private readonly int _maxDegreeOfParallelism;


        public PriceEngine(IPricingRepository pricingRepo)
        {
            _pricingRepo = pricingRepo;
            _convertions_dict = new ConcurrentDictionary<int, List<ProductConvertionRatio>>();
            _maxDegreeOfParallelism = 5;
        }

        public IEnumerable<DbPrice> ComparePrices(IEnumerable<DbPrice> existingPrices, IEnumerable<DbPrice> newPrices)
        {
            List<DbPrice> result = new List<DbPrice>();
            ConcurrentBag<DbPrice> bag = new ConcurrentBag<DbPrice>();
            
            var comparedItem = newPrices.Except(existingPrices, new DbPrice()).ToList();

            var comparedItem_byGroup = comparedItem.GroupBy(g => g.GroupId).ToList();
            var existingPrices_dict = existingPrices.GroupBy(g => g.GroupId).ToDictionary(k => k.Key, v => v.ToList());

            Parallel.ForEach(comparedItem_byGroup, new ParallelOptions() { MaxDegreeOfParallelism = _maxDegreeOfParallelism }, group =>
            {
                if (existingPrices_dict.ContainsKey(group.Key))
                {
                    var lookup = existingPrices_dict[group.Key].ToDictionary(k => k.ProductId, v => v);
                    var items = group.ToList();

                    for (int i = 0; i < items.Count; i++)
                    {
                        var p = items[i];
                        if (lookup.ContainsKey(p.ProductId))
                        {
                            var target = lookup[p.ProductId];
                            if (!target.Price.Equals(p.Price))
                            {
                                var ss = decimal.Subtract(target.Price, p.Price);
                                if (Math.Abs(ss) > 0.00000001m)
                                {
                                    bag.Add(p);
                                }
                            }
                        }
                        else
                        {
                            bag.Add(p);
                        }
                    }
                }
            });

            result = bag.Distinct().ToList();
            return result;
        }

        public IEnumerable<DbPrice> CalculatePrices(int groupId, int productId, int pbandId, int unitId, int psetId)
        {
            System.Diagnostics.Stopwatch swGlobal = new System.Diagnostics.Stopwatch();
            swGlobal.Start();
            _convertions_dict.Clear();
            var globalRecalc = false;

            //if ((groupId > 0 && productId == 0 && pbandId == 0 && unitId == 0 && psetId == 0) ||
            if ((groupId == 0 && productId == 0 && pbandId > 0 && unitId == 0 && psetId == 0) ||
                (groupId == 0 && productId == 0 && pbandId == 0 && unitId == 0 && psetId == 0)
                )
            {
                globalRecalc = true;
            }
            System.Diagnostics.Trace.WriteLine("Load Initial Data");
            System.Diagnostics.Trace.WriteLine(DateTime.Now);
            var tmp_dish = _pricingRepo.GetDishes();
            var tmp_ingredient = _pricingRepo.GetIngredients();
            var tmp_product = _pricingRepo.GetProducts();
            var tmp_product_part = _pricingRepo.GetProductParts();

            System.Diagnostics.Trace.WriteLine("Load Initial Data End");
            System.Diagnostics.Trace.WriteLine(DateTime.Now);

            IEnumerable<Parts> parts = null;
            //load all temp data required for all recalculations
            IEnumerable<GroupProducts> group_products = new List<GroupProducts>();
            if (globalRecalc)
            {
                System.Diagnostics.Trace.WriteLine("GetGroupProductPricesByGroup");
                System.Diagnostics.Trace.WriteLine(DateTime.Now);
                group_products = _pricingRepo.GetGroupProductPricesByGroup(groupId);
            }
            else
            {
                System.Diagnostics.Trace.WriteLine("GetGroupProductPricesByProduct - "+ DateTime.Now);
                group_products = _pricingRepo.GetGroupProductPricesByProduct(groupId, productId, psetId, pbandId, unitId);

                System.Diagnostics.Trace.WriteLine("GetParts - " + DateTime.Now);
                parts = GetParts(tmp_product, tmp_product_part, productId, unitId, psetId, pbandId);
            }

            System.Diagnostics.Trace.WriteLine("GetPrices - " + DateTime.Now);

            var prices = new List<DbPrice>();
            
            var groups = group_products.GroupBy(gp => gp.GroupId).OrderBy(g => g.Key).ToList();

            IEnumerable<DbPrice> existingDbPrices = new List<DbPrice>();
            //if (groupId > 0)
            //{
            //    existingDbPrices = _pricingRepo.GetPrices(groupId);
            //}
            //else
            //{
            //    existingDbPrices = _pricingRepo.GetPrices();
            //}

            Parallel.ForEach(groups, new ParallelOptions() { MaxDegreeOfParallelism = _maxDegreeOfParallelism }, gp=>{
            
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                var gpItems = gp.ToList();
                List<Parts> processingParts = new List<Parts>();
                if (globalRecalc)
                {
                    System.Diagnostics.Trace.WriteLine($"GetParts - {DateTime.Now}");
                    processingParts = GetParts(gpItems, tmp_product, tmp_product_part).ToList();
                }
                else
                {
                    if (parts.Any())
                    {
                        processingParts = parts.ToList();
                    }
                }

                var newPrices = GetPrices(gpItems, processingParts, tmp_product, tmp_product_part, tmp_dish, tmp_ingredient, existingDbPrices).ToList();

                if (newPrices.Any())
                {
                    sw.Stop();
                    System.Diagnostics.Trace.WriteLine($"Group {gp.Key } prices - {newPrices.Count} total time seconds { sw.Elapsed.TotalSeconds}");
                    prices.AddRange(newPrices);
                    System.Diagnostics.Trace.WriteLine($"Adding {newPrices.Count } prices - " + DateTime.Now);
                }
                else {
                    System.Diagnostics.Trace.WriteLine($"Adding {gpItems } prices - " + DateTime.Now);
                    gpItems.ForEach(gpi => {
                        var x = new DbPrice() { GroupId = gpi.GroupId, ProductId = gpi.ProductId, Price = gpi.Price };
                        prices.Add(x);
                    });
                    sw.Stop();
                    System.Diagnostics.Trace.WriteLine($"Group {gp.Key } prices - {newPrices.Count} total time seconds { sw.Elapsed.TotalSeconds}");
                }
            });

            swGlobal.Stop();
            System.Diagnostics.Trace.WriteLine($"Total Time - {swGlobal.Elapsed.TotalSeconds}");
            System.Diagnostics.Trace.WriteLine($"Total Prices - {prices.Count}");
            
            return prices;
        }

        public IEnumerable<Parts> GetParts(IEnumerable<GroupProducts> groupProducts, IEnumerable<ProductItem> tmp_product, IEnumerable<ProductPartItem> tmp_product_part)
        {
            var affectedProducts = groupProducts.Select(gp => gp.ProductId).Distinct();
            var apz = affectedProducts.ToDictionary(x => x);
            var subProductIds = tmp_product_part.Where(pp=> apz.ContainsKey(pp.ProductId)).Select(pp => pp.SubProductId).Distinct().ToList();
            //subProductIds = subProductIds.Intersect(affectedProducts).ToList();

            //var topLevelItems = affectedProducts.Except(subProductIds).ToList();

            var parts = (from ap in affectedProducts
                         join p in tmp_product on ap equals p.ProductId
                         //where !topLevelItems.Contains(ap)
                         where !subProductIds.Contains(ap)
                         select new Parts()
                         {
                             EndDishId = p.ProductId,
                             DishId = p.ProductId,
                             IngredientId = p.ProductId,
                             Quantity = p.Number * p.Quantity,
                             UnitId = p.UnitId,
                             Level = 1,
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

            if (psetId > 0)
            {
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

            while (usageList.Any(x => x.Level == level && !x.IsProcessed))
            {
                //set all to process = 1
                usageList.ForEach(u => u.IsProcessed = false);

                var targetItem = (from u in usageList
                                  join pp in tmp_product_part on u.DishId equals pp.SubProductId
                                  select u).Distinct().ToList();

                targetItem.ForEach(u => u.IsProcessed = true);

                var targetItem2 = (from u in usageList
                                   join pp in tmp_product_part on u.DishId equals pp.SubProductId
                                   where u.Level == level
                                   select new UsageItem() { IngredientId = u.IngredientId, DishId = pp.ProductId, Level = level + 1, IsChoise = pp.IsChoise }).Distinct(new UsageItem());


                usageList = usageList.Where(u => !u.IsProcessed).ToList();
                usageList.AddRange(targetItem2);
                level++;
            }
            var parts = (from u in usageList
                         join p in tmp_product on u.DishId equals p.ProductId
                         select new Parts()
                         {
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

        public IEnumerable<DbPrice> GetPrices(IEnumerable<GroupProducts> groupProductPrice, List<Parts> parts, IEnumerable<ProductItem> tmp_product, IEnumerable<ProductPartItem> tmp_product_part, IEnumerable<DishItem> tmp_dish, IEnumerable<IngredientItem> tmp_ingredient, IEnumerable<DbPrice> existingGroupProductPrices)
        {
            var affectedProducts = groupProductPrice.Select(gp => gp.ProductId).Distinct().ToList();
            Dictionary<int, RecipeType> tmp_dish_dic = tmp_dish.ToDictionary(x => x.ProductId, y => y.RecipeTypeId);
            int level = 1;

            System.Diagnostics.Trace.WriteLine($"Update parts - {DateTime.Now}");
            
            var itemsToUpdate = tmp_product_part.Select(pp => pp.ProductId).Distinct().Intersect(affectedProducts).ToList();

            while (parts.Any(p => p.Level == level && p.Processed == 0))
            {
                System.Diagnostics.Trace.WriteLine($"Level {level}");
                
                parts.ForEach(p => p.Processed = 1);

                var partsToUpdate = (from p in parts
                                     join x in itemsToUpdate on p.IngredientId equals x
                                     select p).ToList();

                //get counts

                partsToUpdate.ForEach(p => p.Processed = 2);

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
                                          PortionType = pp.PortionTypeId,
                                          IsChoise = pp.IsChoise
                                      }).Distinct(new Parts()).ToList();

                parts.AddRange(nextLevelParts);
                //System.Diagnostics.Trace.WriteLine($"Parts {parts.Count}");
                level++;
            }

            /////////////////////////////////////////////////////////////////////////
            ///delete 
            System.Diagnostics.Trace.WriteLine("LINQ 3 - " + DateTime.Now);

            var itemsToDeleteIngredients = parts.Where(p => p.EndDishId != p.IngredientId).Select(p => p.DishId).Distinct().ToList();

            System.Diagnostics.Trace.WriteLine("LINQ 4 - " + DateTime.Now);

            var deletePartItems = parts.Where(p => p.EndDishId == p.IngredientId && 
            itemsToDeleteIngredients.Contains(p.IngredientId)).ToList();

            System.Diagnostics.Trace.WriteLine("LINQ 5 - " + DateTime.Now);

            parts = parts.Except(deletePartItems).ToList();

            /////////////////////////////////////////////////////////////////////////
            ///update recipe type
            
            System.Diagnostics.Trace.WriteLine("LINQ 7 - " + DateTime.Now);

            parts.ForEach(p =>
            {
                if (tmp_dish_dic.ContainsKey(p.DishId))
                {
                    p.RecipeTypeId = (int)tmp_dish_dic[p.DishId];
                }
            });

            /////////////////////////////////////////////////////////////////////////
            ///convertion
            ///
            System.Diagnostics.Trace.WriteLine("LINQ 8 - " + DateTime.Now);

            var itemsToConvert = (from pa in parts
                                  join p in tmp_product on pa.IngredientId equals p.ProductId
                                  select new ProductConvertionRatio() { ProductId = p.ProductId, SourceUnitId = (short)p.UnitId, TargetUnitId = (short)pa.UnitId }).Distinct(new ProductConvertionRatio()).ToList();

            System.Diagnostics.Trace.WriteLine("LINQ 9 - " + DateTime.Now);

            var convertions = this._pricingRepo.GetProductConvertionRatio(itemsToConvert).ToList();
            Dictionary<int, List<ProductConvertionRatio>> convertions_dict = convertions.GroupBy(g => g.ProductId).ToDictionary(k => k.Key, v => v.ToList());

            //List<ProductConvertionRatio> getConvertionsFor = new List<ProductConvertionRatio>();
            //foreach (var c in itemsToConvert) {
            //    if (!_convertions_dict.ContainsKey(c.ProductId))
            //    {
            //        getConvertionsFor.Add(c);
            //    }
            //    else {
            //        var existingItem = _convertions_dict[c.ProductId];
            //        if (!existingItem.Any(x => x.SourceUnitId == c.SourceUnitId && x.TargetUnitId == c.TargetUnitId)) {
            //            getConvertionsFor.Add(c);
            //        }
            //    }
            //}

            //if (getConvertionsFor.Any())
            //{
            //    var convertions = this._pricingRepo.GetProductConvertionRatio(getConvertionsFor).ToList();
            //    System.Diagnostics.Trace.WriteLine("LINQ 100 - " + DateTime.Now);

            //    if (convertions.Any())
            //    {
            //        convertions.GroupBy(c => c.ProductId).ToList().ForEach(cg =>
            //        {
            //            var newItems = cg.ToList();
            //            _convertions_dict.AddOrUpdate(cg.Key, newItems, (k, v) => {
            //                var items = v.Except(newItems, new ProductConvertionRatio()).Distinct(new ProductConvertionRatio());
            //                foreach (var c in items.ToList()) {

            //                    v.Add(c);
            //                }
            //                return v.Distinct().ToList();
            //            });
            //        });
            //    }
            //}

            Dictionary<int, int> tmp_product_unit_dic = tmp_product.ToDictionary(key => key.ProductId, value => value.UnitId);

            for (int i = 0; i < parts.Count; i++)
            {
                var pa = parts[i];
                if (convertions_dict.ContainsKey(pa.IngredientId))
                {
                    List<ProductConvertionRatio> filteredConversions = convertions_dict[pa.IngredientId].ToList();
                    var unit = tmp_product_unit_dic[pa.IngredientId];
                    if (filteredConversions.Count == 1)
                    {
                        var convertion = filteredConversions[0];
                        if (convertion.TargetUnitId == pa.UnitId && convertion.SourceUnitId == unit)
                        {
                            pa.Ratio = convertion.Ratio;
                        }
                        else {
                            var x = "1";
                        }
                    }
                    else
                    {
                        var convertion = (from cr in filteredConversions
                                          where unit == cr.SourceUnitId && cr.TargetUnitId == pa.UnitId
                                          select cr).FirstOrDefault();

                        if (convertion != null)
                        {
                            pa.Ratio = convertion.Ratio;
                        }
                        else {
                            var x = "1";
                        }
                    }
                }
                else {
                    var x = "1";
                }
            }

            System.Diagnostics.Trace.WriteLine($"LINQ 10 - {DateTime.Now}");

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

            System.Diagnostics.Trace.WriteLine($"LINQ 11 - {DateTime.Now}");

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

            res.Clear();

            System.Diagnostics.Trace.WriteLine($"LINQ 12 - {DateTime.Now}");

            var res2 = (from p in tmp_product
                        join pa in parts on p.ProductId equals pa.IngredientId
                        join g in groupProductPrice on pa.DishId equals g.ProductId
                        join dpc in existingGroupProductPrices on p.ProductId equals dpc.ProductId

                        where pa.Processed == 1 && !affectedProducts.Contains(p.ProductId)
                        group p.ProductId
                        by new { p.ProductId, g.GroupId, dpc.Price } into itemPrices
                        select itemPrices
             ).ToList();

            System.Diagnostics.Trace.WriteLine($"LINQ 13 - {DateTime.Now}");

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

            res2.Clear();

            System.Diagnostics.Trace.WriteLine($"LINQ 14 - {DateTime.Now}");

            var productPartsProductIds = tmp_product_part.Select(pp => pp.ProductId).Distinct().ToList();

            var res3 = (from p in tmp_product
                        join pa in parts on p.ProductId equals pa.IngredientId
                        join g in groupProductPrice on pa.DishId equals g.ProductId
                        where pa.Processed == 1 && p.ProductTypeId == ProductType.Dish
                        && !productPartsProductIds.Contains(p.ProductId) //repsent left join

                        select new ProductPrice() { EndDishId = p.ProductId, ProductId = p.ProductId, GroupId = g.GroupId , ApPrice = 0, EpPrice = 0 }

             ).ToList();

            System.Diagnostics.Trace.WriteLine($"LINQ 15 - {DateTime.Now}");

            pricing.AddRange(res3);
            res3.Clear();
            /////////////////////////////////////////////////////////////////////////
            ///delete parts
            //instead removing just create new list on filter
            parts = parts.Where(p => p.EndDishId != p.IngredientId).ToList();

            /////////////////////////////////////////////////////////////////////////

            if (parts.Any())
            {
                int maxLevel = parts.Max(p => p.Level);

                var partsDictionary = parts.GroupBy(p => p.Level).ToDictionary(key => key.Key, value => value.ToList());

                while (partsDictionary.ContainsKey(maxLevel))
                {
                    var levelParts = partsDictionary[maxLevel];
                    List<TmpItem> tmp = new List<TmpItem>();

                    System.Diagnostics.Trace.WriteLine($"maxLevel - {maxLevel} {DateTime.Now}");
                    //System.Diagnostics.Trace.WriteLine($"calculatedPriceItems - {DateTime.Now}");
                    System.Diagnostics.Trace.WriteLine($"calculatedPriceItems2 - {DateTime.Now}");
                    //System.Diagnostics.Trace.WriteLine($"levelParts - {levelParts.Count}");
                    //System.Diagnostics.Trace.WriteLine($"tmp_product_part - {tmp_product_part.Count()}");
                    //System.Diagnostics.Trace.WriteLine($"tmp_product - {tmp_product.Count()}");
                    //System.Diagnostics.Trace.WriteLine($"groupProductPrice - {groupProductPrice.Count()}");
                    //System.Diagnostics.Trace.WriteLine($"pricing - {pricing.Count}");

                    //items with prices
                    var calculatedPriceItems2 = //(from pa in parts
                        (from pa in levelParts
                         //join pp in tmp_product_part on pa.PartId equals pp.ProductPartId
                         join p in tmp_product on pa.IngredientId equals p.ProductId
                         join g in groupProductPrice on pa.DishId equals g.ProductId
                         join pr in pricing on pa.IngredientId equals pr.ProductId
                         where g.GroupId == pr.GroupId
                         let ratio = p.Quantity * p.Number * pa.Ratio
                         let ep_price = pr.EpPrice * pa.Quantity
                         let ap_price = pr.ApPrice * pa.Quantity
                         //let price = pp.PortionTypeId == PortionType.EP ?
                         let price = pa.PortionType == PortionType.EP ?
                            (ep_price / ratio) : (ap_price / ratio)
                         let finalPrice = ratio == 0 ? 0 : price
                         group new
                         {
                             pa_dishid = pa.DishId,
                             pa_quantity = pa.Quantity,
                             pa_unit_id = pa.UnitId,
                             pa_ingredient = pa.IngredientId,
                             g_group_id = g.GroupId,
                             price = finalPrice
                         } by new { DishId = pa.DishId,
                             Quantity = pa.Quantity,
                             UnitId = pa.UnitId,
                             IngredientId = pa.IngredientId,
                             GroupId = g.GroupId,
                             Price = finalPrice
                         }
                         into items
                         select items).ToList();
                    System.Diagnostics.Trace.WriteLine($"leftJoin - {DateTime.Now}");
                    var leftJoin = //(from pa in parts
                        (from pa in levelParts
                         //join pp in tmp_product_part on pa.PartId equals pp.ProductPartId
                         join g in groupProductPrice on pa.DishId equals g.ProductId
                         join pr in pricing on new { ProductId = pa.IngredientId, g.GroupId } equals new { pr.ProductId, pr.GroupId } into prDef
                         from zzzz in prDef.DefaultIfEmpty()
                         where zzzz == null
                         select new TmpItem() {  ProductId = pa.DishId, GroupId = g.GroupId, Quantity = pa.Quantity, SubProductId = pa.IngredientId, UnitId = pa.UnitId}).ToList();

                    tmp.AddRange(leftJoin);

                    //System.Diagnostics.Trace.WriteLine("TmpPriceItems_all each- " + DateTime.Now);
                    System.Diagnostics.Trace.WriteLine($"calculatedPriceItems2 foreach - {DateTime.Now}");
                    calculatedPriceItems2.ForEach(x =>
                    {
                        var xx = x.FirstOrDefault();

                        tmp.Add(new TmpItem()
                        {
                            ProductId = xx.pa_dishid,
                            Quantity = xx.pa_quantity,
                            UnitId = xx.pa_unit_id,
                            SubProductId = xx.pa_ingredient,
                            GroupId = xx.g_group_id,
                            Price = xx.price.Value
                        });
                    });

                    //check total tmp item on each loop
                    System.Diagnostics.Trace.WriteLine($"delete tmp - {DateTime.Now}");
                    var q = (from t in tmp
                             join t2 in tmp on t.ProductId equals t2.ProductId
                             where t.GroupId == t2.GroupId && !t.Price.HasValue
                             select t2).ToList();

                    tmp = tmp.Except(q).ToList();

                    //check total tmp item on each loop
                    System.Diagnostics.Trace.WriteLine($"tmp_product_dict - {DateTime.Now}");

                    var tmp_product_dict = tmp.GroupBy(t => t.ProductId).ToDictionary(key => key.Key, value => value.ToList());
                    tmp = new List<TmpItem>();

                    System.Diagnostics.Trace.WriteLine($"levelParts_product_dict - {DateTime.Now}");
                    var levelParts_product_dict = levelParts.GroupBy(lp => lp.DishId).ToDictionary(key => key.Key, value => value.ToList());
                    levelParts.Clear();
                    System.Diagnostics.Trace.WriteLine($"levelParts_product_dict each- {DateTime.Now}");
                    levelParts_product_dict.Keys.ToList().ForEach(dishId =>
                    {
                        var partsItems = levelParts_product_dict[dishId];
                        if (tmp_product_dict.ContainsKey(dishId))
                        {
                            var tmpItems = tmp_product_dict[dishId];

                            var tmp_grouping111 = (from pa in partsItems
                                                   join t in tmpItems on pa.IngredientId equals t.SubProductId
                                                   where pa.Quantity == t.Quantity && pa.UnitId == t.UnitId &&
                                               ((pa.IsChoise && pa.RecipeTypeId == 3) || pa.RecipeTypeId != 3)
                                                   group t.Price
                                       by new { pa.EndDishId, t.GroupId } into itemPrices
                                                   select itemPrices
                                 ).ToList();

                            foreach (var z in tmp_grouping111)
                            {
                                var pPrice = new ProductPrice()
                                {
                                    EndDishId = z.Key.EndDishId,
                                    ProductId = dishId,
                                    GroupId = z.Key.GroupId
                                };

                                pPrice.ApPrice = z.Sum();
                                pPrice.EpPrice = pPrice.ApPrice;

                                pricing.Add(pPrice);
                            }
                        }
                    });

                    //System.Diagnostics.Trace.WriteLine($"pricing count - {pricing.Count}");

                    levelParts_product_dict.Clear();

                    System.Diagnostics.Trace.WriteLine($"set 0 to pricing- {DateTime.Now}");
                    var filteredParts = parts.Where(p => p.RecipeTypeId == 4 && p.Level <= maxLevel).ToDictionary(key => key.DishId, value => value);
                    //System.Diagnostics.Trace.WriteLine("pricing foreach- " + DateTime.Now);
                    pricing.ForEach(p =>
                    {
                        if (filteredParts.ContainsKey(p.ProductId))
                        {
                            var pPartsExist = filteredParts[p.ProductId];
                            if (pPartsExist != null)
                            {
                                p.ApPrice = p.EpPrice = 0;
                            }
                        }
                    });

                    maxLevel--;
                }
                
                System.Diagnostics.Trace.WriteLine($"Clear group for private/draft - {DateTime.Now}");
                //create dictionary to reduce calls

                var tmp_product_scope_dict = tmp_product.Where(p => p.ScopeId > 1).Select(p=> p.ProductId).ToDictionary(key=> key, value=> value);

                for (int i = 0; i < pricing.Count; i++)
                {
                    var price = pricing[i];
                    if (tmp_product_scope_dict.ContainsKey(price.ProductId))
                    {
                        price.GroupId = 0;
                    }
                }
            }
            System.Diagnostics.Trace.WriteLine($"filter affectedProducts - {DateTime.Now}");

            ////var ccc1 = pricing.Count(p => affectedProducts.Contains(p.ProductId));
            //var ccc2 = pricing.Count(p => affectedProducts.Contains(p.ProductId) && p.ApPrice.HasValue && p.ApPrice == 0);
            ////this should not be null apPrices
            //var ccc3 = pricing.Count(p => affectedProducts.Contains(p.ProductId) && !p.ApPrice.HasValue);
            //var ccc4 = pricing.Count(p => affectedProducts.Contains(p.ProductId) && p.ApPrice.HasValue);
            
            ////OK
            //var ccc5 = pricing.Where(p => affectedProducts.Contains(p.ProductId) && p.ApPrice.HasValue && p.ApPrice != 0).Select(p => new DbPrice() { GroupId = p.GroupId, ProductId = p.ProductId, Price = p.ApPrice.Value }).Distinct(new DbPrice()).ToList();

            //final result
            var result = pricing.Where(p => affectedProducts.Contains(p.ProductId) && p.ApPrice.HasValue).Select(p=> new DbPrice() { GroupId = p.GroupId, ProductId = p.ProductId, Price = p.ApPrice.Value }).Distinct(new DbPrice()).ToList();
            System.Diagnostics.Trace.WriteLine($"done - {DateTime.Now}");
            return result;
        }
    }

    /// <summary>
    /// #pricing
    /// </summary>
    public class ProductPrice {
        public int EndDishId { get; set; }
        public int ProductId { get; set; }
        public int GroupId { get; set; }
        public decimal? ApPrice { get; set; }
        public decimal? EpPrice { get; set; }
    }

    public class UsageItem : TmpUsageItem, IEqualityComparer<UsageItem>
    {
        public int DishId { get; set; }
        public int Level { get; set; }
        public bool IsProcessed { get; set; }

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

    public class TmpUsageItem {
        public int IngredientId { get; set; }
        public bool IsChoise { get; set; }
    }

    public class TmpItem {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
        public int UnitId { get; set; }
        public int SubProductId { get; set; }
        public int GroupId { get; set; }
        public decimal? Price { get; set; }

        public TmpItem() { }
    }
}