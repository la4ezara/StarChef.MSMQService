using StarChef.Common.Hierarchy;
using StarChef.Common.Model;
using StarChef.Common.Repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace StarChef.Common.Engine
{
    public class PriceEngine : IPriceEngine
    {
        private readonly IPricingRepository _pricingRepo;

        public PriceEngine(IPricingRepository pricingRepo)
        {
            _pricingRepo = pricingRepo;
        }

        public bool CanRecalculate(MsmqLog log, DateTime arrivedTime)
        {
            if (log != null)
            {
                //last recalculation was done
                if (log.EndTime.HasValue)
                {
                    //last arrivedTime is older that recalculation end time so no need of action.
                    if (log.EndTime.Value > arrivedTime)
                    {
                        return false;
                    }
                }
                //last recalculation was not finished and it is the same or newer that arrived time.
                else if (log.StartTime.HasValue && log.StartTime.Value >= arrivedTime)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<IEnumerable<DbPrice>> Recalculation(int productId, bool storePrices, DateTime? arrivedTime) {
            List<DbPrice> prices = new List<DbPrice>();
            DateTime dt = DateTime.UtcNow;
            if (arrivedTime.HasValue)
            {
                dt = arrivedTime.Value;
            }

            var lastRecalculationTime = await _pricingRepo.GetLastMsmqStartTime(productId);
            var canRecalculate = CanRecalculate(lastRecalculationTime, dt);

            //validate - is it last recalculation more actual that current request date
            if (!canRecalculate)
            {
                var logId = await _pricingRepo.CreateMsmqLog("Dish Pricing Calculation Skipped", productId, dt);
                await _pricingRepo.UpdateMsmqLog(dt, logId, true);
            }
            else
            {
                var productsAndParts = await _pricingRepo.GetProductsAndParts(productId);

                var products = productsAndParts.Item1;
                var parts = productsAndParts.Item2;

                bool restictBySupplier = await _pricingRepo.IsIngredientAccess();
                IEnumerable<IngredientAlternate> alternates = null;
                if (restictBySupplier)
                {
                    alternates = await _pricingRepo.GetIngredientAlternates(null);
                }

                ProductForest pf = new ProductForest(products.ToList(), parts.ToList());
                pf.Alternates = alternates;
                pf.BuildForest();

                var productIds = products.Select(x => x.ProductId).ToList();
                var groupPrices = await _pricingRepo.GetGroupProductPricesByProduct(productId);
                //groupPrices = groupPrices.Where(x => productIds.Contains(x.ProductId)).ToList();

                Dictionary<int, Dictionary<int, decimal>> newProductPrices = pf.CalculatePrice(groupPrices.ToList(), restictBySupplier);
                var logId = await _pricingRepo.CreateMsmqLog("Dish Pricing Calculation", productId, dt);

                bool isSuccess = true;
                foreach (var group in newProductPrices)
                {
                    //delete old prices which are no longer valid
                    var requiredPrices = groupPrices.Where(x => (x.GroupId.HasValue && x.GroupId == group.Key) ||(group.Key == 0 && !x.GroupId.HasValue));
                    List<int> itemsToDelete = new List<int>();
                    foreach (var price in requiredPrices)
                    {
                        if (!group.Value.ContainsKey((price.ProductId)))
                        {
                            itemsToDelete.Add(price.ProductId);
                        }
                    }

                    int? groupId = null;
                    if (group.Key > 0)
                    {
                        groupId = group.Key;
                    }

                    //delete items for which we require prices but prices was not generated
                    if (itemsToDelete.Any())
                    {
                        await _pricingRepo.ClearPrices(itemsToDelete, groupId);
                    }

                    bool saveResult = _pricingRepo.UpdatePrices(group.Value, groupId, logId, dt);
                    if (!saveResult)
                    {
                        isSuccess = false;
                    }
                }

                await _pricingRepo.UpdateMsmqLog(DateTime.UtcNow, logId, isSuccess);

                foreach (var group in newProductPrices)
                {
                    foreach (var productPrice in group.Value)
                    {
                        prices.Add(new DbPrice() { GroupId = group.Key, ProductId = productPrice.Key, Price = productPrice.Value });
                    }
                }
            }
            return prices;
        }

        public async Task<IEnumerable<DbPrice>> GlobalRecalculation(bool storePrices, DateTime? arrivedTime)
        {
            List<DbPrice> prices = new List<DbPrice>();
            DateTime dt = DateTime.UtcNow;
            if (arrivedTime.HasValue)
            {
                dt = arrivedTime.Value;
            }
            var lastRecalculationTime = await _pricingRepo.GetLastMsmqStartTime(0);
            var canRecalculate = CanRecalculate(lastRecalculationTime, dt);
            //validate is it last recalculation more actual that current request date
            if (!canRecalculate)
            {
                var logId = await _pricingRepo.CreateMsmqLog("Dish Pricing Calculation Skipped", 0, dt);
                await _pricingRepo.UpdateMsmqLog(dt, logId, true);
            }
            else
            {
                var products = await _pricingRepo.GetProducts();
                var parts = await _pricingRepo.GetProductParts();

                bool restictBySupplier = await _pricingRepo.IsIngredientAccess();
                IEnumerable<IngredientAlternate> alternates = null;
                if (restictBySupplier)
                {
                    alternates=await _pricingRepo.GetIngredientAlternates(null);
                }

                ProductForest pf = new ProductForest(products.ToList(), parts.ToList());
                pf.Alternates = alternates;
                pf.BuildForest();

                var groupPrices = await _pricingRepo.GetGroupProductPricesByGroup(0);

                Dictionary<int, Dictionary<int, decimal>> result = pf.CalculatePrice(groupPrices.ToList(), restictBySupplier);

                if (storePrices)
                {
                    var logId = await _pricingRepo.CreateMsmqLog("Dish Pricing Calculation", 0, dt);

                    bool isSuccess = true;
                    foreach (var group in result)
                    {
                        var groupId = group.Key == 0 ? new Nullable<int>() : group.Key;
                        await _pricingRepo.ClearPrices(groupId);
                        bool saveResult = _pricingRepo.InsertPrices(group.Value, groupId, logId, dt);
                        if (!saveResult)
                        {
                            isSuccess = false;
                        }
                    }

                    await _pricingRepo.UpdateMsmqLog(DateTime.UtcNow, logId, isSuccess);
                }

                foreach (var group in result)
                {
                    foreach (var productPrice in group.Value)
                    {
                        prices.Add(new DbPrice() { GroupId = group.Key, ProductId = productPrice.Key, Price = productPrice.Value });
                    }
                }
            }
            return prices;
        }

        public IEnumerable<DbPrice> ComparePrices(IEnumerable<DbPrice> existingPrices, IEnumerable<DbPrice> newPrices)
        {
            ConcurrentBag<DbPrice> bag = new ConcurrentBag<DbPrice>();
            
            var comparedItem = newPrices.Except(existingPrices, new DbPrice()).ToList();

            var comparedItem_byGroup = comparedItem.GroupBy(g => g.GroupId).ToList();
            var existingPrices_dict = existingPrices.GroupBy(g => g.GroupId).ToDictionary(k => k.Key, v => v.ToList());

            foreach (var group in comparedItem_byGroup)
            {
                GetPriceDifferences(existingPrices_dict, bag, group.Key, group.ToList());
            }
            
            IEnumerable<DbPrice> result = bag.Distinct().ToList();
            return result;
        }

        public void GetPriceDifferences(Dictionary<int,List<DbPrice>> existingPrices_dict, ConcurrentBag<DbPrice> bag, int groupId, List<DbPrice> groupPrices) {
            if (existingPrices_dict.ContainsKey(groupId))
            {
                var lookup = existingPrices_dict[groupId].ToDictionary(k => k.ProductId, v => v);
                var items = groupPrices;

                for (int i = 0; i < items.Count; i++)
                {
                    var p = items[i];
                    if (lookup.ContainsKey(p.ProductId))
                    {
                        var target = lookup[p.ProductId];
                        if (!target.Equals(p))
                        {
                            bag.Add(p);
                        }
                    }
                    else
                    {
                        bag.Add(p);
                    }
                }
            }
            else {
                groupPrices.ForEach(gp => bag.Add(gp));
            }
        }

        public async Task<bool> IsEngineEnabled() {
            bool result = false;

            var dbSetting = await _pricingRepo.GetDbSetting(Fourth.StarChef.Invariables.Constants.PRICE_RECALC_CODE_ENGINE);
            if (!string.IsNullOrWhiteSpace(dbSetting) && dbSetting.Equals("1")) {
                result = true;
            }
            return result;
        }
    }
}