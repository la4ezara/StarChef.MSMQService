using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Fourth.Orchestration.Model.Menus;
using StarChef.Common;
using StarChef.Orchestrate.Helpers;
using StarChef.Orchestrate.Models;

namespace StarChef.Orchestrate
{
    public class RecipeUpdatedSetter : IEventSetter<Events.RecipeUpdated.Builder>
    {
        public bool SetForUpdate(Events.RecipeUpdated.Builder builder, string connectionString, int entityId, int databaseId)
        {
            if (builder == null) return false;

            var cust = new Customer(databaseId);
            var dbManager = new DatabaseManager();

            var reader = dbManager.ExecuteReaderMultiResultset(connectionString, "sc_event_recipe", new SqlParameter("@entity_id", entityId));
            if (reader.Read())
            {
                builder.SetCustomerId(cust.ExternalId)
                    .SetCustomerName(cust.Name)
                    .SetExternalId(reader[0].ToString())
                    .SetRecipeName(reader[1].ToString())
                    .SetRecipeType(OrchestrateHelper.MapRecipeType(reader[2].ToString()))
                    .SetUnitSizeQuantity(reader.GetValueOrDefault<double>(3))
                    .SetPortionSizeUnitCode(reader[4].ToString())
                    .SetUnitSizePackDescription(reader[5].ToString())
                    .SetMinimumCost(reader.GetValueOrDefault<double>(6))
                    .SetMaximumCost(reader.GetValueOrDefault<double>(7))
                    .SetCost(reader.GetValueOrDefault<double>(8))
                    .SetCurrencyIso4217Code(reader[9].ToString())
                    .SetVatType(OrchestrateHelper.MapVatType(reader[10].ToString()))
                    .SetVatPercentage(reader.GetValueOrDefault<double>(11))
                    .SetSellingPrice(reader.GetValueOrDefault<double>(12))
                    .SetPricingModel(Events.PricingModel.Margin)
                    .SetPricingModelValue(reader.GetValueOrDefault<double>(14))
                    .SetSuggestedSellingPrice(reader.GetValueOrDefault<double>(16))
                    .SetSuggestedPlu(reader[17].ToString())
                    .SetCreatedUserFirstName(reader[18].ToString())
                    .SetCreatedUserLastName(reader[19].ToString())
                    .SetModifiedUserFirstName(reader[20].ToString())
                    .SetModifiedUserLastName(reader[21].ToString())
                    .SetCaptureDate(Fourth.Orchestration.Model.UnixDateTime.FromDateTime(reader.GetValueOrDefault<DateTime>(22)))
                    .SetModifiedDate(Fourth.Orchestration.Model.UnixDateTime.FromDateTime(reader.GetValueOrDefault<DateTime>(23)));
            }

            //Ingredients
            var ingredientList = new List<RecipeIngredient>();
            if (reader.NextResult())
            {
                ingredientList = GetIngredients(reader, entityId);
            }

            //Kitchenarea
            var kitchenAreaLookup = new Dictionary<int, List<KitchenArea>>();
            if (reader.NextResult())
            {
                kitchenAreaLookup = KitchenAreaLookup(reader);
            }

            //Build Ingredients and KitchenArea
            BuildIngredients(builder, ingredientList, kitchenAreaLookup);

            //Groups
            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    var groupBuilder = Events.RecipeUpdated.Types.RecipeGroup.CreateBuilder();
                    groupBuilder.SetExternalId(reader[0].ToString())
                        .SetGroupName(reader[1].ToString());
                    builder.AddGroups(groupBuilder);
                }
            }

            var categoryTypes = new List<CategoryType>();
            var categories = new List<Category>();
            
            //read category types and categories
            if (reader.NextResult())
                reader.ReadCategories(out categoryTypes, out categories);
            if (categoryTypes.Count > 0)
                BuildCategoryTypes(builder, categoryTypes);

            return true;
        }

        internal static void BuildCategoryTypes(Events.RecipeUpdated.Builder builder, List<CategoryType> categoryTypes)
        {
            foreach (var catType in categoryTypes)
            {
                var categoryTypeBuilder = Events.RecipeUpdated.Types.CategoryType.CreateBuilder();

                var exportType = OrchestrateHelper.MapCategoryExportType(catType.CategoryExportType.ToString());
                categoryTypeBuilder
                    .SetExternalId(catType.ExternalId)
                    .SetCategoryTypeName(catType.Name)
                    .SetExportType(exportType)
                    .SetIsFoodType(catType.IsFood);

                foreach (var category in catType.MainCategories)
                {
                    var mainCategoryBuilder = Events.RecipeUpdated.Types.CategoryType.Types.Category.CreateBuilder();
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
                        var nestedBuilder = Events.RecipeUpdated.Types.CategoryType.Types.Category.CreateBuilder();
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

        private static void BuildIngredients(
            Events.RecipeUpdated.Builder builder,
            List<RecipeIngredient> ingredientList,
            Dictionary<int, List<KitchenArea>> kitchenAreaLookup
            )
        {
            if (ingredientList.Count > 0)
            {
                foreach (var ingredient in ingredientList)
                {
                    var ingredientBuilder = Events.RecipeUpdated.Types.RecipeIngredients.CreateBuilder();

                    ingredientBuilder.SetExternalId(ingredient.IngredientExternalId)
                        .SetIngredientName(ingredient.IngredientName)
                        .SetProductMeasure(ingredient.ProductMeasure)
                        .SetProductUom(ingredient.ProductUom);

                    if (kitchenAreaLookup.Count > 0 && kitchenAreaLookup.ContainsKey(ingredient.ProductPartId))
                    {
                        var kitchenAreas = kitchenAreaLookup[ingredient.ProductPartId];
                        foreach (var kitchenArea in kitchenAreas)
                        {
                            var kitchenAreaBuilder = Events.RecipeUpdated.Types.RecipeIngredients.Types.KitchenArea.CreateBuilder();
                            kitchenAreaBuilder.SetExternalId(kitchenArea.ExternalId)
                                .SetKitchenAreaName(kitchenArea.Name)
                                .SetDisplayOrder(kitchenArea.DisplayOrder);

                            ingredientBuilder.AddKitchenAreas(kitchenAreaBuilder);
                        }
                    }

                    builder.AddIngredients(ingredientBuilder);
                }
            }
        }

        private static Dictionary<int, List<KitchenArea>> KitchenAreaLookup(IDataReader reader)
        {
            var kitchenAreaLookup = new Dictionary<int, List<KitchenArea>>();
            var kitchenAreasList = new List<KitchenArea>();
            while (reader.Read())
            {
                var productPartId = reader.GetValueOrDefault<int>(0);
                if (!kitchenAreaLookup.ContainsKey(productPartId))
                    kitchenAreaLookup[productPartId] = new List<KitchenArea>();
                else
                    kitchenAreasList = kitchenAreaLookup[productPartId];

                var kitchenArea = new KitchenArea
                {
                    ExternalId = reader[1].ToString(),
                    Name = reader[2].ToString(),
                    DisplayOrder = int.Parse(reader[3].ToString())
                };

                kitchenAreasList.Add(kitchenArea);
                kitchenAreaLookup[productPartId] = kitchenAreasList;
            }

            return kitchenAreaLookup;
        }

        private List<RecipeIngredient> GetIngredients(IDataReader reader, int entityId)
        {
            var ingredientList = new List<RecipeIngredient>();

            while (reader.Read())
            {
                var recipeIngredient = new RecipeIngredient
                {
                    ProductPartId = int.Parse(reader[0].ToString()),
                    IngredientExternalId = reader[1].ToString(),
                    IngredientName = reader[2].ToString(),
                    ProductMeasure = reader.GetValueOrDefault<double>(3),
                    ProductUom = reader[4].ToString(),
                    RecipeId = entityId
                };
                ingredientList.Add(recipeIngredient);
            }

            return ingredientList;
        }

        private static void BuildCategoryTree(LinkedList<Category> list, Category p)
        {
            if (list.Count > 0)
            {
                var c = list.Last();
                p.SubCategories = new List<Category> { c };
                list.RemoveLast();
                BuildCategoryTree(list, c);
            }
        }

        private static void BuildCategoryObject(
            List<Category> categoryList,
            string childCategoryId,
            LinkedList<Category> categoryLinkedList
            )
        {
            var d = categoryList.Where(x => x.ParentExternalId == childCategoryId).FirstOrDefault();
            categoryLinkedList.AddLast(d);
            if (childCategoryId != d.ParentExternalId)
                BuildCategoryObject(categoryList, d.ParentExternalId, categoryLinkedList);
        }

        public bool SetForDelete(Events.RecipeUpdated.Builder builder, string entityExternalId, int databaseId)
        {
            if (builder == null) return false;

            var cust = new Customer(databaseId);
            builder
                .SetCustomerId(cust.ExternalId)
                .SetCustomerName(cust.Name)
                .SetExternalId(entityExternalId);

            return true;
        }

        public class KitchenArea
        {
            public int ProductPartId { get; set; }
            public string ExternalId { get; set; }
            public string Name { get; set; }
            public int DisplayOrder { get; set; }
        }

        public class RecipeIngredient
        {
            public int RecipeId { get; set; }
            public string IngredientExternalId { get; set; }
            public string IngredientName { get; set; }
            public double ProductMeasure { get; set; }
            public string ProductUom { get; set; }
            public int ProductPartId { get; set; }
        }
    }
}