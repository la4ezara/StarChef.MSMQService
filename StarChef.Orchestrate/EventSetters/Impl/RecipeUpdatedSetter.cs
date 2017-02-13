using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Fourth.Orchestration.Model.Menus;
using StarChef.Common;
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

            //Category Type
            string categoryTypeExternalId = string.Empty;
            var categoryTypeBuilder = Events.RecipeUpdated.Types.CategoryType.CreateBuilder();
            if (reader.NextResult())
            {
                while (reader.Read())
                {

                    var categoryType = new CategoryType
                    {
                        ExternalId = reader[0].ToString(),
                        Name = reader[1].ToString(),
                        CategoryExportType = OrchestrateHelper.MapCategoryExportType(reader[2].ToString()),
                        IsFood = reader.GetValueOrDefault<bool>(3)
                    };
                    categoryTypes.Add(categoryType);
                }
            }
           
            if (reader.NextResult())
            {
                while (reader.Read())
                {
                   var category = new Category
                    {
                        ExternalId = reader[0].ToString(),
                        Name = reader[1].ToString(),
                        ParentExternalId = reader[2].ToString(),
                        Sequence = reader.GetValueOrDefault<int>(73)
                    };
                    categories.Add(category);
                }
            }

            //CategoryType exists
            if (categoryTypes.Count > 0)
            {
                BuildCategoryTypes(builder, categoryTypes, categories);

                builder.AddCategoryTypes(categoryTypeBuilder);
            }

            return true;
        }
  
        private static void BuildCategoryTypes(Events.RecipeUpdated.Builder builder, List<CategoryType> categoryTypes, List<Category> categoryList)
        {
            foreach (var catType in categoryTypes)
            {
                var categoryTypeBuilder = Events.RecipeUpdated.Types.CategoryType.CreateBuilder();


                categoryTypeBuilder.SetExternalId(catType.ExternalId)
                    .SetCategoryTypeName(catType.Name)
                    .SetExportType(OrchestrateHelper.MapCategoryExportType(catType.CategoryExportType.ToString()))
                    .SetIsFoodType(catType.IsFood);



                var currentCategoryList = categoryList.Where(t => t.ProductTagId == catType.ProductTagId).ToList();

                //CategoryType exists
                if (currentCategoryList.Count > 0 && !string.IsNullOrEmpty(catType.ExternalId))
                {
                    foreach (var category in currentCategoryList.Where(t => t.ParentExternalId == catType.ExternalId && t.Sequence == catType.Sequence - 1))
                    {
                        var mainCategoryBuilder = Events.RecipeUpdated.Types.CategoryType.Types.Category.CreateBuilder();
                        mainCategoryBuilder.SetExternalId(category.ExternalId)
                            .SetCategoryName(category.Name)
                            .SetParentExternalId(category.ParentExternalId);
                        if (category.Sequence > 1)
                        {
                            BuildRcursive(category, mainCategoryBuilder, currentCategoryList);
                        }
                        categoryTypeBuilder.AddMainCategories(mainCategoryBuilder);
                    }
                }
                builder.SetCategoryTypes(categoryTypeBuilder);/// Todo AG : add category here: AddCategoryType(categoryTypeBuilder);
            }
        }

        private static void BuildRcursive(Category cat, Events.RecipeUpdated.Types.CategoryType.Types.Category.Builder categoryBuilder, List<Category> categoryList)
        {
            foreach (var category in categoryList.Where(t => t.ParentExternalId == cat.ExternalId && t.Sequence == cat.Sequence - 1))
            {
                var catBuilder = Events.RecipeUpdated.Types.CategoryType.Types.Category.CreateBuilder();
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

        public class CategoryType : Category
        {
            public Events.CategoryExportType CategoryExportType { get; set; }
            public bool IsFood { get; set; }
            public List<Category> MainCategories { get; set; }
        }
    }
}