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

namespace StarChef.Orchestrate
{
    public class RecipeUpdatedSetter : IEventSetter<Events.RecipeUpdated.Builder>
    {
        internal virtual IDataReader ExecuteDbCommand(string connectionString, int entityId)
        {
            var dbManager = new DatabaseManager();
            return dbManager.ExecuteReaderMultiResultset(connectionString, "sc_event_recipe", new SqlParameter("@entity_id", entityId));
        }

        public bool SetForUpdate(Events.RecipeUpdated.Builder builder, string connectionString, int entityId, int databaseId)
        {
            if (builder == null) return false;

            var cust = new Customer(databaseId);
            var reader = ExecuteDbCommand(connectionString, entityId);
            
            if (reader.Read())
            {
                var changeTypeSc = reader["ChangeType"].ToString();
                var changeType = Events.ChangeType.UPDATE;
                if (changeTypeSc == "Archive")
                {
                    changeType = Events.ChangeType.ARCHIVE;
                }

                builder.SetCustomerId(cust.ExternalId)
                    .SetCustomerName(cust.Name)
                    .SetExternalId(reader[0].ToString())
                    .SetRecipeName(reader[1].ToString())
                    .SetRecipeType(OrchestrateHelper.MapRecipeType(reader[2].ToString()))
                    .SetUnitSizeQuantity(reader.GetValueOrDefault<double>(3))
                    .SetPortionSizeUnitCode(reader[4].ToString())
                    .SetUnitSizePackDescription(reader[5].ToString())
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
                    .SetModifiedDate(Fourth.Orchestration.Model.UnixDateTime.FromDateTime(reader.GetValueOrDefault<DateTime>(23)))
                    .SetPortionSizeQuantity(reader.GetValueOrDefault<double>("PortionSizeQuantity"))
                    .SetPortionSizeUom(reader.GetValueOrDefault<string>("PortionSizeUom") ?? string.Empty)
                    .SetChangeType(changeType);

                var barcodes = ReadBarcodes(reader);
                if (barcodes.Any())
                {
                    builder.AddRangeBarcode(barcodes);
                }
                    
            }

            //Ingredients
            var ingredientList = new List<RecipeIngredient>();
            if (reader.NextResult())
            {
                ingredientList = ReadIngredients(reader, entityId);
            }

            //Kitchenarea
            var kitchenAreaLookup = new Dictionary<int, List<KitchenArea>>();
            if (reader.NextResult())
            {
                List<KitchenArea> kitchenAreas;
                reader.ReadKitchenAreas(out kitchenAreas);

                kitchenAreaLookup = BuildKitchenAreas(kitchenAreas);
            }

            //Build Ingredients and KitchenArea
            BuildIngredients(builder, ingredientList, kitchenAreaLookup);

            //Groups
            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    var groupBuilder = Events.RecipeUpdated.Types.RecipeGroup.CreateBuilder();
                    var externalId = reader.GetValueOrDefault<Guid>("group_guid").ToString();
                    var groupName = reader.GetValueOrDefault<string>("group_name");
                    var groupPrice = reader.GetValueOrDefault<decimal>("product_price");
                    groupBuilder.SetExternalId(externalId)
                        .SetGroupName(groupName)
                        .SetPrice(Fourth.Orchestration.Model.Common.Decimal.BuildFromDecimal(groupPrice));
                    builder.AddGroups(groupBuilder);
                }
            }

            var categoryTypes = new List<CategoryType>();
            List<Category> categories;
            
            //read category types and categories
            if (reader.NextResult())
                reader.ReadCategories(out categoryTypes, out categories);
            if (categoryTypes.Count > 0)
            {
                Func<dynamic> createCategoryType = () => Events.RecipeUpdated.Types.CategoryType.CreateBuilder();
                Func<dynamic> createCategory = () => Events.RecipeUpdated.Types.CategoryType.Types.Category.CreateBuilder();
                BuilderHelpers.BuildCategoryTypes(builder, createCategoryType, createCategory, categoryTypes);
            }

            //read intolerances
            if(reader.NextResult())
            {
                if (reader.Read())
                {
                    if (!reader.IsDBNull(1)) builder.SetContainsMilkOrMilkProducts(reader.GetBoolean(1));
                    if (!reader.IsDBNull(2)) builder.SetContainsEggOrEggDerivatives(reader.GetBoolean(2));
                    if (!reader.IsDBNull(3)) builder.SetContainsCerealsThatContainGluten(reader.GetBoolean(3));
                    if (!reader.IsDBNull(4)) builder.SetContainsPeanuts(reader.GetBoolean(4));
                    if (!reader.IsDBNull(5)) builder.SetContainsNutsOrNutTrace(reader.GetBoolean(5));
                    if (!reader.IsDBNull(6)) builder.SetContainsSesameSeedOrSesameSeedProducts(reader.GetBoolean(6));
                    if (!reader.IsDBNull(7)) builder.SetContainsSoya(reader.GetBoolean(7));
                    if (!reader.IsDBNull(8)) builder.SetContainsFishOrFishProducts(reader.GetBoolean(8));
                    if (!reader.IsDBNull(9)) builder.SetContainsCrustaceans(reader.GetBoolean(9));
                    if (!reader.IsDBNull(10)) builder.SetContainsMolluscs(reader.GetBoolean(10));
                    if (!reader.IsDBNull(11)) builder.SetContainsMustardOrMustardProducts(reader.GetBoolean(11));
                    if (!reader.IsDBNull(12)) builder.SetContainsCeleryOrCeleriacProducts(reader.GetBoolean(12));
                    if (!reader.IsDBNull(13)) builder.SetContainsSulphurDioxideOrSulphites(reader.GetBoolean(13));
                    if (!reader.IsDBNull(14)) builder.SetContainsLupinFlourOrLupinProducts(reader.GetBoolean(14));
                    if (!reader.IsDBNull(15)) builder.SetContainsGlutenOrGlutenProducts(reader.GetBoolean(15));
                }
            }

            //read nutritions
            if (reader.NextResult())
            {
                if (reader.Read())
                {
                    if (!reader.IsDBNull(1)) builder.SetEnergyKJperServing(reader.GetDouble(1));
                    if (!reader.IsDBNull(2)) builder.SetEnergyKCalPerServing(reader.GetDouble(2));
                    if (!reader.IsDBNull(3)) builder.SetFatPerServing(reader.GetDouble(3));
                    if (!reader.IsDBNull(4)) builder.SetSaturatedFatPerServing(reader.GetDouble(4));
                    if (!reader.IsDBNull(5)) builder.SetSugarPerServing(reader.GetDouble(5));
                    if (!reader.IsDBNull(6)) builder.SetSaltPerServing(reader.GetDouble(6));
                }
            }

            //Recipe sets
            var recipeSets = new List<RecipeSet>();
            if (reader.NextResult())
            {
                recipeSets = GetRecipeSets(reader);
            }

            if (!recipeSets.Any())
            {
                builder.SetChangeType(Events.ChangeType.ARCHIVE);
            }

            BuildRecipeSets(builder, recipeSets);

            return true;
        }

        private static List<RecipeSet> GetRecipeSets(IDataReader reader)
        {
            List<RecipeSet> recipeSets = new List<RecipeSet>();

            while (reader.Read())
            {
                var recipeSet = new RecipeSet
                {
                    Id = reader.GetValueOrDefault<int>("pset_id"),
                    Name = reader.GetValueOrDefault<string>("pset_name")
                };
                recipeSets.Add(recipeSet);
            }

            return recipeSets;
        }

        private static void BuildRecipeSets(Events.RecipeUpdated.Builder builder, List<RecipeSet> recipeSets)
        {
            foreach (var recipeSet in recipeSets)
            {
                var setBuilder = Events.RecipeUpdated.Types.Set.CreateBuilder();

                setBuilder
                    .SetExternalId(recipeSet.Id)
                    .SetSetName(recipeSet.Name);

                builder.AddSets(setBuilder);
            }
        }

        internal static void BuildIngredients(Events.RecipeUpdated.Builder builder, List<RecipeIngredient> ingredientList, Dictionary<int, List<KitchenArea>> kitchenAreaLookup)
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

        internal static Dictionary<int, List<KitchenArea>> BuildKitchenAreas(List<KitchenArea> kitchenAreas)
        {
            var kitchenAreaLookup = new Dictionary<int, List<KitchenArea>>();
            foreach (var kitchen in kitchenAreas)
            {
                var kitchenAreasList = !kitchenAreaLookup.ContainsKey(kitchen.ProductPartId) ? new List<KitchenArea>() : kitchenAreaLookup[kitchen.ProductPartId];

                kitchenAreasList.Add(kitchen);
                kitchenAreaLookup[kitchen.ProductPartId] = kitchenAreasList;
            }

            return kitchenAreaLookup;
        }

        internal List<RecipeIngredient> ReadIngredients(IDataReader reader, int entityId)
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

        internal List<string> ReadBarcodes(IDataReader reader)
        {
            var barcodeList = new List<string>();

            var barcodesString = reader.GetValueOrDefault<string>("Barcode");

            if(!string.IsNullOrEmpty(barcodesString))
            {
                var separators = new char[] { ' ', '\t', '\r', '\n' };
                var barcodes = barcodesString.Split(separators , StringSplitOptions.RemoveEmptyEntries);

                foreach(var barcode in barcodes)
                {
                    barcodeList.Add(barcode);
                }
            }

            return barcodeList;
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

        public class RecipeIngredient
        {
            public int RecipeId { get; set; }
            public string IngredientExternalId { get; set; }
            public string IngredientName { get; set; }
            public double ProductMeasure { get; set; }
            public string ProductUom { get; set; }
            public int ProductPartId { get; set; }
        }

        private class RecipeSet
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}