﻿using StarChef.Common.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StarChef.Common.Repository
{
    public interface IPricingRepository
    {
        Task<IEnumerable<GroupProducts>> GetGroupProductPricesByGroup(int groupId);

        Task<IEnumerable<GroupProducts>> GetGroupProductPricesByProduct(int productId);
        Task<IEnumerable<DbPrice>> GetPrices();
        Task<IEnumerable<DbPrice>> GetPrices(int groupId);
        Task<int> GetPricesCount();
        Task<IEnumerable<Product>> GetProducts();
        Task<IEnumerable<ProductPart>> GetProductParts();
        Task<IEnumerable<GroupSets>> GetGroupSets(int groupId, int includeDescendants);
        Task<IEnumerable<ProductPset>> GetProductPsets();

        Task<string> GetDbSetting(string settingName);

        Task ClearPrices();
        Task<int> CreateMsmqLog(string action, int productId, DateTime logDate);
        Task<int> UpdateMsmqLog(DateTime logDate, int logId, bool isSuccess);
        Task<DateTime?> GetLastMsmqStartTime();
        bool InsertPrices(Dictionary<int, decimal> prices, int? groupId, int logId, DateTime logDate);
        bool UpdatePrices(Dictionary<int, decimal> prices, int? groupId, int logId, DateTime logDate);
    }
}