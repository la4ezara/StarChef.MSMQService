using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Fourth.Orchestration.Model.Menus;
using StarChef.Common;
using StarChef.Orchestrate.Models;

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
                        .SetImageUrl(reader[21].ToString());
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
                    ReadCategories(reader, out categoryTypes, out categories);

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

            return true;
        }

        [DebuggerDisplay(@"\{{TagName}\}")]
        private struct CategoryRecord
        {
            public int ProductId { get; set; }
            public int TagId { get; set; }
            public string TagName { get; set; }
            public int? TagParentId { get; set; }
            public int? TagExportTypeId { get; set; }
            public bool IsFoodType { get; set; }
            public Guid TagGuid { get; set; }
            public Guid? TagParentGuid { get; set; }
            public bool IsSelected { get; set; }

            internal Category CreateCategory()
            {
                var category = new Category
                {
                    Id = TagId,
                    ProductId = ProductId,
                    ExternalId = Convert.ToString(TagGuid),
                    Name = TagName,
                    ParentExternalId = Convert.ToString(TagParentGuid)
                };
                return category;
            }
        }

        internal static void ReadCategories(IDataReader reader, out List<CategoryType> categoryTypes, out List<Category> categories)
        {
            #region load data from reader
            var records = new List<CategoryRecord>();

            while (reader.Read())
            {
                var cat = new CategoryRecord
                {
                    ProductId = reader.GetValueOrDefault<int>("product_id"),
                    TagId = reader.GetValueOrDefault<int>("tag_id"),
                    TagName = reader.GetValueOrDefault<string>("tag_name"),
                    TagParentId = reader.GetValueOrDefault<int?>("tag_parent_id"),
                    TagExportTypeId = reader.GetValueOrDefault<int?>("tag_export_type_id"),
                    IsFoodType = reader.GetValueOrDefault<bool>("IsFoodType"),
                    TagGuid = reader.GetValueOrDefault<Guid>("tag_guid"),
                    TagParentGuid = reader.GetValueOrDefault<Guid?>("tag_parent_guid"),
                    IsSelected = reader.GetValueOrDefault<bool>("IsSelected")
                };
                records.Add(cat);
            }
            #endregion
            #region get category types
            // category types don't have a parent
            categoryTypes = records.Where(i => !i.TagParentId.HasValue).Select(i => new CategoryType
            {
                ProductId = i.ProductId,
                Id = i.TagId,
                ExternalId = Convert.ToString(i.TagGuid),
                Name = i.TagName,
                CategoryExportType = i.TagExportTypeId,
                MainCategories = new List<Category>() // will be at least one item in category
            }).ToList();
            #endregion

            #region build category legs for selected ones
            categories = new List<Category>();
            var stack = new Stack<CategoryRecord>();
            records.Where(i => i.IsSelected).ToList().ForEach(c => stack.Push(c));

            // for each category take all parents up to the type (the type is not included)
            while (stack.Count > 0)
            {
                var categoryRecord = stack.Peek();
                if (categoryRecord.TagParentId.HasValue)
                {
                    var parentRecord = records.Single(i => i.TagId == categoryRecord.TagParentId.Value && !i.IsSelected);
                    stack.Push(parentRecord);
                }
                else // met a category type 
                {
                    // build the leg of items from the top to the selected one
                    var typeRecord = stack.Pop(); // skip the type
                    var categoryType = categoryTypes.Single(t => t.Id == typeRecord.TagId);

                    var currentRecord = stack.Pop();
                    var parentCategory = currentRecord.CreateCategory();
                    categories.Add(parentCategory); // this item maybe the selected one, so that sub items will be created later if needed
                    categoryType.MainCategories.Add(parentCategory); // all category legs should be added to their types respectively
                    while (!currentRecord.IsSelected) // if the current category is not selected, then need to create nested one
                    {
                        // take the next one and create category item
                        currentRecord = stack.Pop();
                        var currentCategory = currentRecord.CreateCategory();

                        // add category to its parent and go down in the hierarchy
                        parentCategory.SubCategories = new List<Category> { currentCategory };
                        parentCategory = currentCategory;
                    }
                }
            }
            #endregion
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


                    var productCategoryTypes = categoryTypes.Where(ct => ct.ProductId == suppliedPack.ProductId).ToList();
                    BuildCategoryTypes(suppliedPackSizeBuilder, productCategoryTypes);

                    builder.AddSuppliedPackSizes(suppliedPackSizeBuilder);
                }
            }
        }

        internal static void BuildCategoryTypes(Events.IngredientUpdated.Types.SuppliedPackSize.Builder builder, List<CategoryType> categoryTypes)
        {
            foreach (var catType in categoryTypes)
            {
                var categoryTypeBuilder = Events.IngredientUpdated.Types.CategoryType.CreateBuilder();

                var exportType = OrchestrateHelper.MapCategoryExportType(catType.CategoryExportType.ToString());
                categoryTypeBuilder
                    .SetExternalId(catType.ExternalId)
                    .SetCategoryTypeName(catType.Name)
                    .SetExportType(exportType)
                    .SetIsFoodType(catType.IsFood);

                foreach (var category in catType.MainCategories)
                {
                    var mainCategoryBuilder = Events.IngredientUpdated.Types.CategoryType.Types.Category.CreateBuilder();
                    mainCategoryBuilder
                        .SetExternalId(category.ExternalId)
                        .SetCategoryName(category.Name)
                        .SetParentExternalId(category.ParentExternalId);

                    #region build nested items

                    var parentItem = category;
                    var parentBuilder = mainCategoryBuilder;
                    while (parentItem.SubCategories != null)
                    {
                        var nestedItem = category.SubCategories.First();
                        var nestedBuilder = Events.IngredientUpdated.Types.CategoryType.Types.Category.CreateBuilder();
                        nestedBuilder
                            .SetExternalId(nestedItem.ExternalId)
                            .SetCategoryName(nestedItem.Name)
                            .SetParentExternalId(nestedItem.ParentExternalId);
                        parentBuilder.AddSubCategories(nestedBuilder);
                        parentItem = nestedItem;
                        parentBuilder = nestedBuilder;
                    }

                    #endregion

                    categoryTypeBuilder.AddMainCategories(mainCategoryBuilder);
                }
                builder.AddCategoryTypes(categoryTypeBuilder);
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
            //repeated CategoryType CategoryTypes = 24{ get; set; }
        }

        public class IngredientGroup
        {
            public string ExternalId { get; set; }
            public string GroupName { get; set; }
        }

        public class AlternativeIssuingUnit
        {
            public double Quantity { get; set; }
            public string UnitCode { get; set; }
            public string PackDescription { get; set; }
        }

        public class CategoryType : Category
        {
            public int? CategoryExportType { get; set; }
            public bool IsFood { get; set; }
            public List<Category> MainCategories { get; set; }
        }
    }
}