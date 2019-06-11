using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Fourth.Orchestration.Model.Menus;
using StarChef.Common;
using StarChef.Orchestrate.EventSetters.Impl;
using StarChef.Orchestrate.Helpers;
using StarChef.Orchestrate.Models;
using Decimal = Fourth.Orchestration.Model.Common.Decimal;

namespace StarChef.Orchestrate
{
    public class IngredientUpdateSetter : IEventSetter<Events.IngredientUpdated.Builder>
    {
        public bool SetForUpdate(Events.IngredientUpdated.Builder builder, string connectionString, int entityId, int databaseId)
        {
            if (builder == null) return false;

            var cust = new Customer(databaseId);
            var dbManager = new DatabaseManager();
            using (var reader = dbManager.ExecuteReaderMultiResultset(connectionString, "sc_event_ingredient", new SqlParameter("@product_id", entityId)))
            {
                if (reader.Read())
                {
                    var changeTypeSc = reader["changetype"].ToString();
                    var changeType = Events.ChangeType.UPDATE;
                    if (changeTypeSc == "Archive")
                    {
                        changeType = Events.ChangeType.ARCHIVE;
                    }
                              
                    builder
                        .SetExternalId(reader[0].ToString())
                        .SetCustomerId(cust.ExternalId)
                        .SetCustomerName(cust.Name)
                        .SetIngredientName(reader[1].ToString())
                        .SetUnitSizeNumber(reader.GetValueOrDefault<double>(2))
                        .SetUnitSizeUom(reader[3].ToString())
                        .SetUnitSizePackDescription(reader[4].ToString())
                        .SetVatCode(reader[5].ToString())
                        .SetCostModel((Events.CostModel)Enum.Parse(typeof(Events.CostModel), string.IsNullOrEmpty(reader[6].ToString()) ? "None" : reader[6].ToString(), true))
                        .SetIsOverhead(reader.GetValueOrDefault<bool>(7))
                        .SetIsSplittable(reader.GetValueOrDefault<bool>(8))
                        .SetYield(reader.GetValueOrDefault<double>(9))
                        .SetOrderableCaseSizeQuantity(reader.GetValueOrDefault<int>(10))
                        .SetOrderType(reader[11].ToString())
                        .SetBinNumber(reader[12].ToString())
                        .SetIsAdjustmentItem(reader.GetValueOrDefault<bool>(13))
                        .SetIsPreventUseInRecipe(reader.GetValueOrDefault<bool>(14))
                        .SetIsTransferStatusAvailable(reader.GetValueOrDefault<bool>(15))
                        .SetIsExcludeFromGpCalc(reader.GetValueOrDefault<bool>(16))
                        .SetIsZeroStockCount(reader.GetValueOrDefault<bool>(17))
                        .SetVintage(reader[18].ToString())
                        .SetMinimumShelfLife(reader.GetValueOrDefault<long>(19))
                        .SetIsExpiryDateInputRequired(reader.GetValueOrDefault<bool>(20))
                        .SetImageUrl(reader[21].ToString())
                        .SetChangeType(changeType);
                }

                //SuppliedPackSizes
                var suppliedPackList = new List<IngredientSuppliedPackSize>();
                if (reader.NextResult())
                {
                    suppliedPackList = GetSuppliedPackSizes(reader);
                }

                var categoryTypes = new List<CategoryType>();
                var categories = new List<Category>();
                //read category types and categories
                if (reader.NextResult())
                    reader.ReadCategories(out categoryTypes, out categories);

                //PriceBands
                var ingredientPriceBands = new List<IngredientPriceBand>();
                if (reader.NextResult())
                {
                    ingredientPriceBands = GetIngredientPriceBands(reader);
                }

                BuildSuppliedPackSizes(builder, suppliedPackList, categoryTypes, ingredientPriceBands);

                //AlternativeIssuingUnits
                var alternativeIssuingUnits = new List<AlternativeIssuingUnit>();
                if (reader.NextResult())
                {
                    alternativeIssuingUnits = GetAlternativeIssuingUnits(reader);
                }
                BuildAlternativeIssuingUnits(builder, alternativeIssuingUnits);

                //IngredientGroups
                var ingredientGroups = new List<IngredientGroup>();
                if (reader.NextResult())
                {
                    ingredientGroups = GetIngredientGroups(reader);
                }
                BuildIngredientGroups(builder, ingredientGroups);

                //Ingredient sets
                var ingredientSets = new List<IngredientSet>();
                if (reader.NextResult())
                {
                    ingredientSets = GetIngredientSets(reader);
                }

                if (!ingredientSets.Any())
                {
                    builder.SetChangeType(Events.ChangeType.ARCHIVE);
                }

                BuildIngredientSets(builder, ingredientSets);
            }

            return true;
        }

        private static List<IngredientSet> GetIngredientSets(IDataReader reader)
        {
            List<IngredientSet> ingredientSets = new List<IngredientSet>();

            while (reader.Read())
            {
                var ingredientSet = new IngredientSet
                {
                    Id = reader.GetValueOrDefault<int>("pset_id"),
                    Name = reader.GetValueOrDefault<string>("pset_name")
                };
                ingredientSets.Add(ingredientSet);
            }

            return ingredientSets;
        }

        private static void BuildIngredientSets(Events.IngredientUpdated.Builder builder, List<IngredientSet> ingredientSets)
        {
            foreach (var ingredientSet in ingredientSets)
            {
                var setBuilder = Events.IngredientUpdated.Types.Set.CreateBuilder();

                setBuilder
                    .SetExternalId(ingredientSet.Id)
                    .SetSetName(ingredientSet.Name);

                builder.AddSets(setBuilder);
            }
        }

        private static List<IngredientSuppliedPackSize> GetSuppliedPackSizes(IDataReader reader)
        {
            var suppliedPackList = new List<IngredientSuppliedPackSize>();

            while (reader.Read())
            {
                var suppliedPack = new IngredientSuppliedPackSize
                {

                    IsDefault = reader.GetValueOrDefault<bool>(0),
                    IsPreferred = reader.GetValueOrDefault<bool>(1),
                    DistributorName = reader[2].ToString(),
                    DistributorCode = reader[3].ToString(),
                    DistributorUom = reader[4].ToString(),
                    SupplyQuantityNumber = reader.GetValueOrDefault<double>(5),
                    SupplyQuantityQuantity = reader.GetValueOrDefault<double>(6),
                    SupplyQuantityUom = reader[7].ToString(),
                    SupplyQuantityPackDescription = reader[8].ToString(),
                    CostPrice = reader.GetValueOrDefault<double>(9),
                    CostPricePerItem = reader.GetValueOrDefault<double>(10),
                    CurrencyIso4217Code = reader[11].ToString(),
                    IsOrderable = reader.GetValueOrDefault<bool>(12),
                    MaxOrderQuantity = reader.GetValueOrDefault<long>(13),
                    MinOrderQuantity = reader.GetValueOrDefault<long>(14),
                    CreatedUserFirstName = reader[15].ToString(),
                    CreatedUserLastName = reader[16].ToString(),
                    CaptureDate = Fourth.Orchestration.Model.UnixDateTime.FromDateTime(reader.GetValueOrDefault<DateTime>(17)),
                    ModifiedUserFirstName = reader[18].ToString(),
                    ModifiedUserLastName = reader[19].ToString(),
                    ModifiedDate = Fourth.Orchestration.Model.UnixDateTime.FromDateTime(reader.GetValueOrDefault<DateTime>(20)),
                    RetailBarCodeDetails = reader[21].ToString(),
                    RankOrder = reader.GetValueOrDefault<long>(22),
                    ProductId = reader.GetValueOrDefault<int>(23),
                    InvoicePrice = reader.GetValueOrDefault<decimal>(25),
                    InvoiceUnitOfMeasure = reader.GetValueOrDefault<string>(26),
                    IsVariableWeighted = reader.GetValueOrDefault<bool>(27),
                };
                suppliedPackList.Add(suppliedPack);
            }

            return suppliedPackList;
        }

        private static List<AlternativeIssuingUnit> GetAlternativeIssuingUnits(IDataReader reader)
        {
            var alternativeIssuingUnits = new List<AlternativeIssuingUnit>();

            while (reader.Read())
            {
                var suppliedPack = new AlternativeIssuingUnit
                {

                    Quantity = reader.GetValueOrDefault<double>(0),
                    UnitCode = reader[1].ToString(),
                    PackDescription = reader[2].ToString(),

                };
                alternativeIssuingUnits.Add(suppliedPack);
            }

            return alternativeIssuingUnits;
        }

        private static List<IngredientGroup> GetIngredientGroups(IDataReader reader)
        {
            var ingredientGroups = new List<IngredientGroup>();

            while (reader.Read())
            {
                var ingredientGroup = new IngredientGroup
                {
                    ExternalId = reader[0].ToString(),
                    GroupName = reader[1].ToString(),
                };
                ingredientGroups.Add(ingredientGroup);
            }

            return ingredientGroups;
        }

        private static List<IngredientPriceBand> GetIngredientPriceBands(IDataReader reader)
        {
            var ingredientPriceBands = new List<IngredientPriceBand>();

            while (reader.Read())
            {
                var ingredientPriceBand = new IngredientPriceBand
                {
                    ProductId = reader.GetValueOrDefault<int>(0),
                    Name = reader[1].ToString(),
                    Price = reader.GetValueOrDefault<decimal>(2),
                    Id = reader.GetValueOrDefault<short>(3)
                };
                ingredientPriceBands.Add(ingredientPriceBand);
            }
            return ingredientPriceBands;
        }

        private static void BuildSuppliedPackSizes(Events.IngredientUpdated.Builder builder, List<IngredientSuppliedPackSize> suppliedPackList, List<CategoryType> categoryTypes, List<IngredientPriceBand> priceBands)
        {
            if (suppliedPackList.Count > 0)
            {
                foreach (var suppliedPack in suppliedPackList)
                {
                    var suppliedPackSizeBuilder = Events.IngredientUpdated.Types.SuppliedPackSize.CreateBuilder();

                    suppliedPackSizeBuilder.SetIsDefault(suppliedPack.IsDefault)
                        .SetIsPreferred(suppliedPack.IsPreferred)
                        .SetDistributorName(suppliedPack.DistributorName)
                        .SetDistributorCode(suppliedPack.DistributorCode)
                        .SetDistributorUom(suppliedPack.DistributorUom)
                        .SetSupplyQuantityNumber(suppliedPack.SupplyQuantityNumber)
                        .SetSupplyQuantityQuantity(suppliedPack.SupplyQuantityQuantity)
                        .SetSupplyQuantityUom(suppliedPack.SupplyQuantityUom)
                        .SetSupplyQuantityPackDescription(suppliedPack.SupplyQuantityPackDescription)
                        .SetCostPrice(suppliedPack.CostPrice)
                        .SetCostPricePerItem(suppliedPack.CostPricePerItem)
                        .SetCurrencyIso4217Code(suppliedPack.CurrencyIso4217Code)
                        .SetIsOrderable(suppliedPack.IsOrderable)
                        .SetMaxOrderQuantity(suppliedPack.MaxOrderQuantity)
                        .SetMinOrderQuantity(suppliedPack.MinOrderQuantity)
                        .SetCreatedUserFirstName(suppliedPack.CreatedUserFirstName)
                        .SetCreatedUserLastName(suppliedPack.CreatedUserLastName)
                        .SetCaptureDate(suppliedPack.CaptureDate)
                        .SetModifiedUserFirstName(suppliedPack.ModifiedUserFirstName)
                        .SetModifiedUserLastName(suppliedPack.ModifiedUserLastName)
                        .SetModifiedDate(suppliedPack.ModifiedDate)
                        .SetRetailBarCodeDetails(suppliedPack.RetailBarCodeDetails)
                        .SetRankOrder(suppliedPack.RankOrder)
                        .SetInvoicePrice(Decimal.BuildFromDecimal(suppliedPack.InvoicePrice))
                        .SetInvoiceUnitOfMeasure(suppliedPack.InvoiceUnitOfMeasure)
                        .SetIsVariableWeighted(suppliedPack.IsVariableWeighted)
                        ;

                    var productCategoryTypes = categoryTypes.Where(ct => ct.ProductId == suppliedPack.ProductId).ToList();
                    Func<dynamic> createCategoryType = () => Events.IngredientUpdated.Types.CategoryType.CreateBuilder();
                    Func<dynamic> createCategory = () => Events.IngredientUpdated.Types.CategoryType.Types.Category.CreateBuilder();
                    BuilderHelpers.BuildCategoryTypes(suppliedPackSizeBuilder, createCategoryType, createCategory, productCategoryTypes);

                    BuildPriceBands(suppliedPackSizeBuilder, priceBands.Where(p=>p.ProductId == suppliedPack.ProductId).ToList());

                    builder.AddSuppliedPackSizes(suppliedPackSizeBuilder);
                }
            }
        }

        private static void BuildAlternativeIssuingUnits(Events.IngredientUpdated.Builder builder, List<AlternativeIssuingUnit> alternativeIssuingUnits)
        {
            if (alternativeIssuingUnits.Count > 0)
            {
                foreach (var ingredient in alternativeIssuingUnits)
                {
                    var alternativeIssuingUnitBuilder = Events.IngredientUpdated.Types.AlternativeIssuingUnit.CreateBuilder();

                    alternativeIssuingUnitBuilder.SetQuantity(ingredient.Quantity)
                        .SetUnitCode(ingredient.UnitCode)
                        .SetPackDescription(ingredient.PackDescription);

                    builder.AddAlternativeIssuingUnits(alternativeIssuingUnitBuilder);
                }
            }
        }

        private static void BuildPriceBands(Events.IngredientUpdated.Types.SuppliedPackSize.Builder builder, List<IngredientPriceBand> priceBands)
        {
            foreach (var priceBand in priceBands)
            {
                var priceBandBuilder = Events.IngredientUpdated.Types.SuppliedPackSize.Types.PriceBand.CreateBuilder();

                priceBandBuilder.SetPrice(Decimal.BuildFromDecimal(priceBand.Price))
                                .SetName(priceBand.Name)
                                .SetId(priceBand.Id.ToString());
                builder.AddPriceBands(priceBandBuilder);
            }
        }

        private static void BuildIngredientGroups(Events.IngredientUpdated.Builder builder, List<IngredientGroup> ingredientGroups)
        {
            if (ingredientGroups.Count > 0)
            {
                foreach (var ingredient in ingredientGroups)
                {
                    var ingredientGroupBuilder = Events.IngredientUpdated.Types.IngredientGroup.CreateBuilder();

                    ingredientGroupBuilder.SetExternalId(ingredient.ExternalId)
                        .SetGroupName(ingredient.GroupName);

                    builder.AddGroups(ingredientGroupBuilder);
                }
            }
        }

        public bool SetForDelete(Events.IngredientUpdated.Builder builder, string entityExternalId, int databaseId)
        {
            if (builder == null) return false;

            var cust = new Customer(databaseId);
            builder
                .SetCustomerId(cust.ExternalId)
                .SetCustomerName(cust.Name)
                .SetExternalId(entityExternalId);

            return true;
        }

        public class IngredientSuppliedPackSize
        {
            public bool IsDefault { get; set; }
            public bool IsPreferred { get; set; }
            public string DistributorName { get; set; }
            public string DistributorCode { get; set; }
            public string DistributorUom { get; set; }
            public double SupplyQuantityNumber { get; set; }
            public double SupplyQuantityQuantity { get; set; }
            public string SupplyQuantityUom { get; set; }
            public string SupplyQuantityPackDescription { get; set; }
            public double CostPrice { get; set; }
            public double CostPricePerItem { get; set; }
            public string CurrencyIso4217Code { get; set; }
            public bool IsOrderable { get; set; }
            public long MaxOrderQuantity { get; set; }
            public long MinOrderQuantity { get; set; }
            public string CreatedUserFirstName { get; set; }
            public string CreatedUserLastName { get; set; }
            public long CaptureDate { get; set; }
            public string ModifiedUserFirstName { get; set; }
            public string ModifiedUserLastName { get; set; }
            public long ModifiedDate { get; set; }
            public string RetailBarCodeDetails { get; set; }
            public long RankOrder { get; set; }
            public int ProductId { get; set; }
            public decimal InvoicePrice { get; set; }
            public string InvoiceUnitOfMeasure { get; set; }
            public bool IsVariableWeighted { get; set; }

        }

        public class IngredientGroup
        {
            public string ExternalId { get; set; }
            public string GroupName { get; set; }
        }

        public class IngredientPriceBand
        {
            public int ProductId { get; set; }
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
        }

        public class AlternativeIssuingUnit
        {
            public double Quantity { get; set; }
            public string UnitCode { get; set; }
            public string PackDescription { get; set; }
        }

        private class IngredientSet
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}