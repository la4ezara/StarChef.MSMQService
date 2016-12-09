using StarChef.Common;
using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Events = Fourth.Orchestration.Model.Menus.Events;

namespace StarChef.Orchestrate.Models
{
    public class Ingredient
    {
        public int Id { get; set; }
        public Ingredient() { }

        public Ingredient(int ingredientId)
        {
            Id = ingredientId;
        }

        #region ...Build Data...

        public Events.IngredientUpdated.Builder Build(Customer cust, string connectionString)
        {
            var rand = new Random();
            var builder = Events.IngredientUpdated.CreateBuilder();
            var dbManager = new DatabaseManager();

            using (var reader = dbManager.ExecuteReaderMultiResultset(connectionString, "sc_event_ingredient", new SqlParameter("@product_id", Id)))
            {
                if (reader.Read())
                {
                    builder
                        .SetExternalId(reader[0].ToString())
                        .SetCustomerId(cust.ExternalId)
                        .SetCustomerName(cust.Name)
                        .SetSequenceNumber(rand.Next(1, int.MaxValue))
                        .SetSource(Events.SourceSystem.STARCHEF)
                        .SetIngredientName(reader[1].ToString())
                        .SetUnitSizeNumber(reader.GetValueOrDefault<double>(2))
                        .SetUnitSizeUom(reader[3].ToString())
                        .SetUnitSizePackDescription(reader[4].ToString())
                        .SetVatCode(reader[5].ToString())
                        .SetCostModel((Events.CostModel) Enum.Parse(typeof(Events.CostModel), reader[6].ToString(), true))
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
                        .SetChangeType(Events.ChangeType.UPDATE);
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
                {
                    while (reader.Read())
                    {
                        if (reader.IsDBNull(6))
                        {
                            var categoryType = new CategoryType
                            {
                                ExternalId = reader[0].ToString(),
                                Name = reader[1].ToString(),
                                CategoryExportType = reader.GetValueOrDefault<int?>(3),
                                ProductTagId = reader.GetValueOrDefault<int>(5),
                                Sequence = reader.GetValueOrDefault<int>(7)
                            };
                            categoryTypes.Add(categoryType);
                        }
                        else
                        {
                            var category = new Category
                            {
                                ExternalId = reader[0].ToString(),
                                Name = reader[1].ToString(),
                                ParentExternalId = reader[2].ToString(),
                                ProductTagId = reader.GetValueOrDefault<int>(5),
                                Sequence = reader.GetValueOrDefault<int>(7)
                            };
                            categories.Add(category);
                        }
                    }
                }


                BuildSuppliedPackSizes(builder, suppliedPackList, categoryTypes, categories);


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
            }
            return builder;
        }

        private static void BuildSuppliedPackSizes(Events.IngredientUpdated.Builder builder, List<IngredientSuppliedPackSize> suppliedPackList, List<CategoryType> categoryTypes, List<Category> categories)
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
                                            .SetRankOrder(suppliedPack.RankOrder);

                    
                    BuildCategoryTypes(suppliedPackSizeBuilder, categoryTypes.Where(t=> t.ProductTagId == suppliedPack.ProductTagId).ToList(),categories);

                    builder.AddSuppliedPackSizes(suppliedPackSizeBuilder);
                }
            }
        }

        private static void BuildCategoryTypes(Events.IngredientUpdated.Types.SuppliedPackSize.Builder builder, List<CategoryType> categoryTypes, List<Category> categoryList)
        {
            foreach (var catType in categoryTypes)
            {
                var categoryTypeBuilder = Events.IngredientUpdated.Types.CategoryType.CreateBuilder();
               
                  
                categoryTypeBuilder.SetExternalId(catType.ExternalId)
                                   .SetCategoryTypeName(catType.Name)
                                   .SetExportType(OrchestrateHelper.MapCategoryExportType(catType.CategoryExportType.ToString()))
                                   .SetIsFoodType(catType.IsFood);
                


                var currentCategoryList = categoryList.Where(t => t.ProductTagId == catType.ProductTagId).ToList();

                //CategoryType exists
                if (currentCategoryList.Count > 0 && !string.IsNullOrEmpty(catType.ExternalId))
                {
                    foreach (var category in categoryList.Where(t => t.ParentExternalId == catType.ExternalId && t.Sequence == catType.Sequence-1))
                    {
                        var mainCategoryBuilder = Events.IngredientUpdated.Types.CategoryType.Types.Category.CreateBuilder();
                        mainCategoryBuilder.SetExternalId(category.ExternalId)
                       .SetCategoryName(category.Name)
                       .SetParentExternalId(category.ParentExternalId);
                        if (category.Sequence > 1)
                        {
                            BuildRcursive(category, mainCategoryBuilder, currentCategoryList);
                        }
                        categoryTypeBuilder.AddMainCategories(mainCategoryBuilder);
                    }
                    builder.AddCategoryTypes(categoryTypeBuilder);
                }
            }
        }

        private static void BuildRcursive(Category cat, Events.IngredientUpdated.Types.CategoryType.Types.Category.Builder categoryBuilder, List<Category> categoryList)
        {
            foreach (var category in categoryList.Where(t=> t.ParentExternalId == cat.ExternalId && t.Sequence == cat.Sequence -1))
            {
                var catBuilder = Events.IngredientUpdated.Types.CategoryType.Types.Category.CreateBuilder();
                catBuilder.SetExternalId(category.ExternalId)
                         .SetCategoryName(category.Name)
                         .SetParentExternalId(category.ParentExternalId);

                if (category.Sequence > 1)
                {
                    BuildRcursive(category, catBuilder, categoryList);
                }
                categoryBuilder.AddSubCategories(catBuilder);
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

        private static void BuildIngredientGroups(Events.IngredientUpdated.Builder builder, List<IngredientGroup> alternativeIssuingUnits)
        {
            if (alternativeIssuingUnits.Count > 0)
            {
                foreach (var ingredient in alternativeIssuingUnits)
                {
                    var ingredientGroupBuilder = Events.IngredientUpdated.Types.IngredientGroup.CreateBuilder();

                    ingredientGroupBuilder.SetExternalId(ingredient.ExternalId)
                                            .SetGroupName(ingredient.GroupName);

                    builder.AddGroups(ingredientGroupBuilder);
                }
            }
        }

        #endregion ...Build Data...

        #region ...Get Data...
        private List<IngredientSuppliedPackSize> GetSuppliedPackSizes(IDataReader reader)
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
                     CaptureDate = reader.GetValueOrDefault<DateTime>(17).Ticks,
                     ModifiedUserFirstName = reader[18].ToString(),
                     ModifiedUserLastName = reader[19].ToString(),
                     ModifiedDate = reader.GetValueOrDefault<DateTime>(20).Ticks,
                     RetailBarCodeDetails = reader[21].ToString(),
                     RankOrder = reader.GetValueOrDefault<long>(22),
                     ProductTagId = reader.GetValueOrDefault<int>(23),
                };
                suppliedPackList.Add(suppliedPack);
            }

            return suppliedPackList;
        }

        private List<AlternativeIssuingUnit> GetAlternativeIssuingUnits(IDataReader reader)
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

        private List<IngredientGroup> GetIngredientGroups(IDataReader reader)
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

        #endregion ...Get Data...

    }
}
