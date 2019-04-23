using Fourth.Orchestration.Model.Menus;
using StarChef.Common;
using StarChef.Orchestrate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Decimal = Fourth.Orchestration.Model.Common.Decimal;

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
            using (var reader = dbManager.ExecuteReaderMultiResultset(connectionString, "sc_event_recipe_nutritions", new SqlParameter("@entity_id", entityId)))
            {
                if (reader.Read())
                {
                    builder
                        .SetCustomerCanonicalId(cust.ExternalId)
                        .SetRecipeId(reader.GetValueOrDefault<string>("product_guid"));

                    List<RecipeNutrition> nutritions = new List<RecipeNutrition>();

                    if (reader.NextResult())
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
                var nutrition = new RecipeNutrition
                {

                    Id = reader.GetValueOrDefault<int>("nutrient_id"),
                    Name = reader.GetValueOrDefault<string>("nutrient_name"),
                    NutrientPerHundredGram = reader.GetValueOrDefault<Decimal>("nutrient_percent"),
                    NutrientPerPortion = reader.GetValueOrDefault<Decimal>("nutrient_portion"),
                    NutrientReferenceIntake = reader.GetValueOrDefault<Decimal>("nutrient_ds_percent"),
                    NutrientDescription = reader.GetValueOrDefault<string>("nutrient_desc"),
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