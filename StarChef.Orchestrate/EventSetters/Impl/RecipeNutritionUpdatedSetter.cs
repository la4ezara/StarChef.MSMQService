using Fourth.Orchestration.Model.Menus;
using StarChef.Common;
using StarChef.Orchestrate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Fourth.Orchestration.Model.Common;
using Decimal = Fourth.Orchestration.Model.Common.Decimal;
using Fourth.StarChef.Invariables.Extensions;

namespace StarChef.Orchestrate
{
    public class RecipeNutritionUpdatedSetter : IEventSetter<Events.RecipeNutritionUpdated.Builder>
    {
        public bool SetForDelete(Events.RecipeNutritionUpdated.Builder builder, string entityExternalId, int databaseId)
        {
            throw new NotImplementedException();
        }

        public bool SetForUpdate(Events.RecipeNutritionUpdated.Builder builder, string connectionString, int entityId, int databaseId)
        {
            if (builder == null)
            {
                return false;
            }

            var cust = new Customer(databaseId);
            var dbManager = new DatabaseManager();
            using (var reader = dbManager.ExecuteReaderMultiResultset(connectionString, "sc_event_recipe_nutrient", new SqlParameter("@entity_id", entityId)))
            {
                if(reader.Read())
                {
                    var productGuidAsString = (reader.GetValueOrDefault<Guid>("product_guid")).ToString();
                    builder
                        .SetCustomerCanonicalId(cust.ExternalId)
                        .SetSource(SourceSystemId.STARCHEF)
                        .SetRecipeId(productGuidAsString);


                    List<RecipeNutrition> nutritions = new List<RecipeNutrition>();

                    if(reader.NextResult())
                    {
                        nutritions = GetRecipeNutrition(reader);
                    }

                    BuildRecipeNutritions(builder, nutritions);
                         
                }
            }

            return true;
        }

        private List<RecipeNutrition> GetRecipeNutrition(IDataReader reader)
        {
            List<RecipeNutrition> nutritions = new List<RecipeNutrition>();

            while (reader.Read())
            {
                var nutrientName = reader.GetValueOrDefault<string>("nutrient_name");
                var nutPercent = reader.GetValueOrDefault<decimal>("nutrient_percent");
                var nutPercentPortion = reader.GetValueOrDefault<decimal?>("nutrient_portion") ?? 0;
                var nutRefIntake = reader.GetValueOrDefault<decimal?>("nutrient_ds_percent") ?? 0;
                var nutDescription = reader.GetValueOrDefault<string>("nutrient_desc");

                if (nutrientName == "Sodium")
                {
                    nutPercent *= 2.5m;
                    nutPercentPortion *= 2.5m;
                    nutRefIntake *= 2.5m;
                    nutrientName = "Salt";
                    nutDescription = "Salt(mg)";
                }

                var nutrition = new RecipeNutrition
                {
                    Id = reader.GetValueOrDefault<int>("nutrient_id"),
                    Name = nutrientName,
                    NutrientPerHundredGram = Decimal.BuildFromDecimal(nutPercent),
                    NutrientPerPortion = Decimal.BuildFromDecimal(nutPercentPortion),
                    NutrientReferenceIntake = Decimal.BuildFromDecimal(nutRefIntake),
                    NutrientDescription = nutDescription,
                };

                nutritions.Add(nutrition);
            }

            return nutritions;
        }

        private static void BuildRecipeNutritions(Events.RecipeNutritionUpdated.Builder builder, List<RecipeNutrition> nutritions)
        {
            foreach (var nutrition in nutritions)
            {
                var recipeNutritionsBuilder = Events.RecipeNutritionUpdated.Types.RecipeNutrition.CreateBuilder();

                recipeNutritionsBuilder
                    .SetNutrientId(nutrition.Id)
                    .SetNutrientName(nutrition.Name)
                    .SetNutrientPerHundredGram(nutrition.NutrientPerHundredGram)
                    .SetNutrientPerPortion(nutrition.NutrientPerPortion)
                    .SetReferenceIntake(nutrition.NutrientReferenceIntake)
                    .SetNutrientDescription(nutrition.NutrientDescription);

                builder.AddRecipeNutritions(recipeNutritionsBuilder);
            }
        }

        private class RecipeNutrition
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Decimal NutrientPerHundredGram { get; set; }
            public Decimal NutrientPerPortion { get; set; }
            public Decimal NutrientReferenceIntake { get; set; }
            public string NutrientDescription { get; set; }
        }
    }
}