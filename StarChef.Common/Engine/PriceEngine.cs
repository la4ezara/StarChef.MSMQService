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

        }

        public async Task<IEnumerable<DbPrice>> GlobalRecalculation(bool storePrices)
        {
            DateTime dt = DateTime.UtcNow;

            var products = await _pricingRepo.GetProducts();
            var parts = await _pricingRepo.GetProductParts();
            
            ProductForest pf = new ProductForest(products.ToList(), parts.ToList());
            pf.BuildForest();

            var groupPrices = await _pricingRepo.GetGroupProductPricesByGroup(0);
            Dictionary<int, Dictionary<int, decimal>> result = pf.CalculatePrice(groupPrices.ToList());

            if (storePrices)
            {
                await _pricingRepo.ClearPrices();
                var logId = await _pricingRepo.CreateMsmqLog("Global Price Recalculation", dt);

                bool isSuccess = true;
                foreach (var group in result)
                {
                    bool saveResult = _pricingRepo.InsertPrices(group.Value, group.Key == 0 ? new Nullable<int>() : group.Key, logId, dt);
                    if (!saveResult) {
                        isSuccess = false;
                    }
                }

                await _pricingRepo.UpdateMsmqLog(DateTime.UtcNow, logId, isSuccess);
            }

            List<DbPrice> prices = new List<DbPrice>();
            foreach (var group in result)
            {
                foreach (var productPrice in group.Value) {
                    prices.Add(new DbPrice() { GroupId = group.Key, ProductId = productPrice.Key, Price = productPrice.Value });
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
                            if (!target.Equals(p)) {
                                bag.Add(p);
                            }
                        }
                        else
                        {
                            bag.Add(p);
                        }
                    }
                }
            });

            IEnumerable<DbPrice> result = bag.Distinct().ToList();
            return result;
        }
    }
}