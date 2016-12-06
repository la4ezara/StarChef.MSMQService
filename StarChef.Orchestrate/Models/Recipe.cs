using Fourth.Orchestration.Model.Menus;
using StarChef.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace StarChef.Orchestrate.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public Recipe(int recipeId)
        {
            Id = recipeId;
        }

        public Events.RecipeUpdated.Builder Build(Customer cust, string connectionString)
        {
            var rand = new Random();
            var builder = Events.RecipeUpdated.CreateBuilder();
            var dbManager = new DatabaseManager();

            var reader = dbManager.ExecuteReaderMultiResultset(connectionString,
                                    "sc_event_recipe",
                                    new SqlParameter("@entity_id", Id));
            if (reader.Read())
            {
                builder.SetCustomerId(cust.ExternalId)
                       .SetCustomerName(cust.Name)
                       .SetExternalId(reader[0].ToString())
                       .SetRecipeName(reader[1].ToString())
                       .SetRecipeType(OrchestrateHelper.MapRecipeType(reader[2].ToString()))
                       .SetUnitSizeQuantity(double.Parse(reader[3].ToString()))
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
                       .SetCaptureDate((DateTime.Parse(reader[22].ToString())).Ticks)
                       .SetModifiedDate((DateTime.Parse(reader[23].ToString())).Ticks)
                       .SetSource(Events.SourceSystem.STARCHEF)
                       .SetChangeType(Events.ChangeType.UPDATE)    
                       .SetSequenceNumber(rand.Next(1, int.MaxValue));
            }

            //Ingredients
            var ingredientList = new List<RecipeIngredient>();
            if (reader.NextResult())
            {
                ingredientList = GetIngredients(reader);
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

            //Category Type
            string categoryTypeExternalId = string.Empty;
            var categoryTypeBuilder = Events.RecipeUpdated.Types.CategoryType.CreateBuilder();
            if (reader.NextResult())
            {
                if (reader.Read())
                {
                    categoryTypeExternalId = reader[0].ToString();
                    categoryTypeBuilder.SetExternalId(categoryTypeExternalId)
                                       .SetCategoryTypeName(reader[1].ToString())
                                       .SetExportType(OrchestrateHelper.MapCategoryExportType(reader[2].ToString()))
                                       .SetIsFoodType(int.Parse(reader[3].ToString()) == 1);
                }
            }

            //MainCategory
            var mainCategoryBuilder = Events.RecipeUpdated.Types.CategoryType.Types.Category.CreateBuilder();

            var categoryLinkedList = new LinkedList<Category>();

            var categoryList = new List<Category>();
            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    categoryList.Add(new Category
                    {
                        ExternalId = reader[0].ToString(),
                        Name = reader[1].ToString(),
                        ParentExternalId = reader[2].ToString()
                    });
                }
            }

            //CategoryType exists
            if (categoryList.Count > 0 && !string.IsNullOrEmpty(categoryTypeExternalId))
            {
                BuildCategoryHierarchy(categoryTypeExternalId, 
                                       categoryTypeBuilder, 
                                       mainCategoryBuilder, 
                                       categoryLinkedList, 
                                       categoryList);

                builder.SetCategoryTypes(categoryTypeBuilder);
            }

            return builder;
        }

        private static void BuildCategoryHierarchy(
            string categoryTypeExternalId, 
            Events.RecipeUpdated.Types.CategoryType.Builder categoryTypeBuilder, 
            Events.RecipeUpdated.Types.CategoryType.Types.Category.Builder mainCategoryBuilder, 
            LinkedList<Category> categoryLinkedList, 
            List<Category> categoryList
            )
        {
            var childCategory = categoryList.FirstOrDefault(x => x.ExternalId == categoryTypeExternalId);
            categoryLinkedList.AddFirst(childCategory);
            OrchestrateHelper.BuildCategoryObject(categoryList, childCategory.ParentExternalId, categoryLinkedList);

            var categoryLastAdded = categoryLinkedList.Last();
            categoryLinkedList.RemoveLast();
            OrchestrateHelper.BuildCategoryTree(categoryLinkedList, categoryLastAdded);

            if (categoryLastAdded != null)
            {
                BuildCategory(mainCategoryBuilder, categoryLastAdded);

                categoryTypeBuilder.AddMainCategories(mainCategoryBuilder);
           }
        }

        private static void BuildCategory(
            Events.RecipeUpdated.Types.CategoryType.Types.Category.Builder categoryBuilder, 
            Category item
            )
        {
            categoryBuilder.SetExternalId(item.ExternalId)
                           .SetCategoryName(item.Name)
                           .SetParentExternalId(item.ParentExternalId);

            if(item.SubCategories !=null && item.SubCategories.Count > 0)
            {
                foreach(var subItem in item.SubCategories)
                {
                    var subCategoryBuilder = Events.RecipeUpdated.Types.CategoryType.Types.Category.CreateBuilder();

                    BuildCategory(subCategoryBuilder, subItem);

                    categoryBuilder.AddSubCategories(subCategoryBuilder);
                }
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
                var productPartId = int.Parse(reader[0].ToString());
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

        private List<RecipeIngredient> GetIngredients(IDataReader reader)
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
                    RecipeId = Id
                };
                ingredientList.Add(recipeIngredient);
            }

            return ingredientList;
        }
    }
}