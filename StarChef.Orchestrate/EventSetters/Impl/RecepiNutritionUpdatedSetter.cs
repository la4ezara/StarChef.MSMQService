using Fourth.Orchestration.Model.Menus;
using StarChef.Common;
using StarChef.Orchestrate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace StarChef.Orchestrate
{
    public class RecepiNutritionUpdatedSetter : IEventSetter<Events.RecipeNutritionUpdated.Builder>
    {
        public bool SetForDelete(Events.RecipeNutritionUpdated.Builder builder, string entityExternalId, int databaseId)
        {
            throw new NotImplementedException();
        }

        public bool SetForUpdate(Events.RecipeNutritionUpdated.Builder builder, string connectionString, int entityId, int databaseId)
        {
            if (builder == null) return false;

            var cust = new Customer(databaseId);
            var dbManager = new DatabaseManager();
            using (var reader = dbManager.ExecuteReaderMultiResultset(connectionString, "sc_event_set", new SqlParameter("@pset_id", entityId)))
            {
                if (reader.Read())
                {
                    byte productType = reader.GetValueOrDefault<byte>("product_type_id");

                    builder
                        .SetCustomerCanonicalId(cust.ExternalId);
                        //.SetRecipeId(entityId)

                        //.SetStatus(Events.EntityState.Valid)
                        //.SetSetName(reader.GetValueOrDefault<string>("pset_name"));

                    builder.

                    List<Product> products = new List<Product>();
                    if (reader.NextResult())
                    {
                        products = GetProducts(reader);
                    }
                    switch (productType)
                    {
                        // Ingredient
                        case 1:
                            BuildIngredients(builder, products);
                            break;
                        // Dish (Recipe)
                        case 2:
                            BuildRecipes(builder, products);
                            break;
                    }
                }
            }

            return true;
        }

        private List<Product> GetProducts(IDataReader reader)
        {
            List<Product> products = new List<Product>();

            while (reader.Read())
            {
                var product = new Product
                {

                    Id = reader.GetValueOrDefault<string>("product_guid"),
                    Name = reader.GetValueOrDefault<string>("product_name")

                };
                products.Add(product);
            }

            return products;
        }

        private static void BuildIngredients(Events.SetUpdated.Builder builder, List<Product> products)
        {
            foreach (var product in products)
            {
                var ingredientBuilder = Events.SetUpdated.Types.Ingredient.CreateBuilder();

                ingredientBuilder
                    .SetExternalId(product.Id)
                    .SetIngredientName(product.Name);

                builder.AddIngredients(ingredientBuilder);
            }
        }

        private static void BuildRecipes(Events.SetUpdated.Builder builder, List<Product> products)
        {
            foreach (var product in products)
            {
                var recipeBuilder = Events.SetUpdated.Types.Recipe.CreateBuilder();

                recipeBuilder
                    .SetExternalId(product.Id)
                    .SetRecipeName(product.Name);

                builder.AddRecipes(recipeBuilder);
            }
        }

        private class Product
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
    }
}