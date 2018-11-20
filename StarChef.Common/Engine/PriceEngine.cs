using StarChef.Common.Hierarchy;
using StarChef.Common.Model;
using StarChef.Common.Repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StarChef.Common.Engine
{
    public class PriceEngine : IPriceEngine
    {
        private readonly IPricingRepository _pricingRepo;
        private readonly int _maxDegreeOfParallelism;


        public PriceEngine(IPricingRepository pricingRepo)
        {
            _pricingRepo = pricingRepo;
            _maxDegreeOfParallelism = 1;
        }

        public async Task<IEnumerable<DbPrice>> Recalculation(int productId, bool storePrices) {
            throw new NotImplementedException();
            DateTime dt = DateTime.UtcNow;

            var products = await _pricingRepo.GetProducts();
            var parts = await _pricingRepo.GetProductParts();

            ProductForest pf = new ProductForest(products.ToList(), parts.ToList());
            pf.BuildForest();
            var forestCuts = pf.GetAffectedCuts(productId);
            pf.ReAssignForest(forestCuts);

            var groupPrices = await _pricingRepo.GetGroupProductPricesByProduct(productId);
            Dictionary<int, Dictionary<int, decimal>> newProductPrices = pf.CalculatePrice(groupPrices.ToList());
            var logId = await _pricingRepo.CreateMsmqLog("Partial Pricing Calculation", productId, dt);

            bool isSuccess = true;
            foreach (var group in newProductPrices)
            {
                bool saveResult = _pricingRepo.UpdatePrices(group.Value, group.Key == 0 ? new Nullable<int>() : group.Key, logId, dt);
                if (!saveResult)
                {
                    isSuccess = false;
                }
            }

            await _pricingRepo.UpdateMsmqLog(DateTime.UtcNow, logId, isSuccess);

            List<DbPrice> prices = new List<DbPrice>();
            foreach (var group in newProductPrices)
            {
                foreach (var productPrice in group.Value)
                {
                    prices.Add(new DbPrice() { GroupId = group.Key, ProductId = productPrice.Key, Price = productPrice.Value });
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
            bool executeRecalculation = true;
            var lastRecalculationTime = await _pricingRepo.GetLastMsmqStartTime();

            //validate is it last recalculation more actual that current request date
            if (lastRecalculationTime.HasValue && lastRecalculationTime.Value > dt)
            {
                executeRecalculation = false;
                var logId = await _pricingRepo.CreateMsmqLog("Dish Pricing Calculation Skipped", 0, dt);
                await _pricingRepo.UpdateMsmqLog(dt, logId, true);
            }

            if (executeRecalculation)
            {
                var products = await _pricingRepo.GetProducts();
                var parts = await _pricingRepo.GetProductParts();

                ProductForest pf = new ProductForest(products.ToList(), parts.ToList());
                pf.BuildForest();

                var groupPrices = await _pricingRepo.GetGroupProductPricesByGroup(0);
                Dictionary<int, Dictionary<int, decimal>> result = pf.CalculatePrice(groupPrices.ToList());

                if (storePrices)
                {
                    await _pricingRepo.ClearPrices();
                    var logId = await _pricingRepo.CreateMsmqLog("Dish Pricing Calculation", 0, dt);

                    bool isSuccess = true;
                    foreach (var group in result)
                    {
                        bool saveResult = _pricingRepo.InsertPrices(group.Value, group.Key == 0 ? new Nullable<int>() : group.Key, logId, dt);
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

            Parallel.ForEach(comparedItem_byGroup, new ParallelOptions() { MaxDegreeOfParallelism = _maxDegreeOfParallelism }, group =>
            {
                GetPriceDifferences(existingPrices_dict, bag, group.Key, group.ToList());
            });
            
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