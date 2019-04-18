using System;
using Xunit;
using StarChef.Common.Hierarchy;
using System.Collections.Generic;
using StarChef.Common.Model;
using Fourth.StarChef.Invariables;

namespace StarChef.Common.Tests
{
    public class ProductForestTests
    {
        [Fact]
        public void ConstructorTest() {
            List<Product> products = new List<Product>();
            products.Add(new Product() { ScopeId = 3 });

            List<ProductPart> parts = new List<ProductPart>();

            var forest = new ProductForest(products, parts);
            Assert.Single(forest.PrivateProducts);
        }

        [Fact]
        public void ReplaceForest()
        {
            List<Product> products = new List<Product>();
            products.Add(new Product() { ScopeId = 1 });

            List<ProductPart> parts = new List<ProductPart>();

            var forest = new ProductForest(products, parts);
            Assert.Empty(forest.Forest);
            Dictionary<int, ProductNode> newForest = new Dictionary<int, ProductNode>();
            newForest.Add(1, new ProductNode(1, 1, 1, Constants.ProductType.Ingredient));
            forest.ReAssignForest(newForest);
            Assert.NotEmpty(forest.Forest);
            Assert.True(forest.Forest.ContainsKey(1));
            Assert.Equal(Constants.ProductType.Ingredient, forest.Forest[1].NodeType);
        }

        [Fact]
        public void SimpleForestTest()
        {
            var recipeProductId = 3;
            var emptyRecipeId = 4;

            List<Product> products = new List<Product>();
            products.Add(new Product() { ProductId = 1, Number = 1, Quantity = 1, ProductTypeId = Constants.ProductType.Ingredient, Wastage = 50, ScopeId = 1, UnitId = 1 });
            products.Add(new Product() { ProductId = 2, Number = 2, Quantity = 2, ProductTypeId = Constants.ProductType.Ingredient, Wastage = 25, ScopeId = 1, UnitId = 2 });
            products.Add(new Product() { ProductId = recipeProductId, Number = 1, Quantity = 1, ProductTypeId = Constants.ProductType.Dish, Wastage = 25, ScopeId = 1, UnitId = 2, RecipeTypeId = Constants.RecipeType.Choice });
            products.Add(new Product() { ProductId = emptyRecipeId, Number = 1, Quantity = 1, ProductTypeId = Constants.ProductType.Dish, Wastage = 25, ScopeId = 1, UnitId = 2, RecipeTypeId = Constants.RecipeType.Choice });

            List<ProductPart> parts = new List<ProductPart>();
            parts.Add(new ProductPart() { ProductId = recipeProductId, SubProductId = 1, IsChoise = true, PortionTypeId = Constants.PortionType.AP, ProductTypeId = Constants.ProductType.Ingredient, Quantity = 1, Ratio = 1, UnitId = 1 });
            parts.Add(new ProductPart() { ProductId = recipeProductId, SubProductId = 2, IsChoise = false, PortionTypeId = Constants.PortionType.EP, ProductTypeId = Constants.ProductType.Ingredient, Quantity = 1, Ratio = 1, UnitId = 1 });

            var forest = new ProductForest(products, parts);
            forest.BuildForest();

            Assert.NotEmpty(forest.Forest);
            Assert.Equal(2, forest.Forest.Count);
            Assert.True(forest.Forest.ContainsKey(recipeProductId));
            Assert.NotEmpty(forest.Forest[recipeProductId].Childs);
            Assert.Equal(parts.Count, forest.Forest[recipeProductId].Childs.Count);
        }

        [Fact]
        public void SimpleForestTestBroken()
        {
            var recipeProductId = 3;
            var emptyRecipeId = 4;

            List<Product> products = new List<Product>();
            products.Add(new Product() { ProductId = 1, Number = 1, Quantity = 1, ProductTypeId = Constants.ProductType.Ingredient, Wastage = 50, ScopeId = 1, UnitId = 1 });
            products.Add(new Product() { ProductId = 2, Number = 2, Quantity = 2, ProductTypeId = Constants.ProductType.Ingredient, Wastage = 25, ScopeId = 1, UnitId = 2 });
            products.Add(new Product() { ProductId = recipeProductId, Number = 1, Quantity = 1, ProductTypeId = Constants.ProductType.Dish, Wastage = 25, ScopeId = 1, UnitId = 2, RecipeTypeId = Constants.RecipeType.Choice });
            products.Add(new Product() { ProductId = emptyRecipeId, Number = 1, Quantity = 1, ProductTypeId = Constants.ProductType.Dish, Wastage = 25, ScopeId = 1, UnitId = 2, RecipeTypeId = Constants.RecipeType.Choice });

            List<ProductPart> parts = new List<ProductPart>();
            parts.Add(new ProductPart() { ProductId = recipeProductId, SubProductId = 1, IsChoise = true, PortionTypeId = Constants.PortionType.AP, ProductTypeId = Constants.ProductType.Ingredient, Ratio = 1, UnitId = 1 });
            parts.Add(new ProductPart() { ProductId = recipeProductId, SubProductId = 2, IsChoise = false, PortionTypeId = Constants.PortionType.EP, ProductTypeId = Constants.ProductType.Ingredient, Quantity = 1, Ratio = 1, UnitId = 1 });

            var forest = new ProductForest(products, parts);
            forest.BuildForest();

            Assert.NotEmpty(forest.Forest);
            Assert.Equal(2, forest.Forest.Count);
            Assert.True(forest.Forest.ContainsKey(recipeProductId));
            Assert.NotEmpty(forest.Forest[recipeProductId].Childs);
            Assert.Equal(parts.Count, forest.Forest[recipeProductId].Childs.Count);
        }

        [Fact]
        public void NestedSimpleForestTest()
        {
            var recipeProductId = 3;
            var nestedRecipeId = 4;

            List<Product> products = new List<Product>();
            products.Add(new Product() { ProductId = 1, Number = 1, Quantity = 1, ProductTypeId = Constants.ProductType.Ingredient, Wastage = 50, ScopeId = 1, UnitId = 1 });
            products.Add(new Product() { ProductId = 2, Number = 2, Quantity = 2, ProductTypeId = Constants.ProductType.Ingredient, Wastage = 25, ScopeId = 1, UnitId = 2 });
            products.Add(new Product() { ProductId = recipeProductId, Number = 1, Quantity = 1, ProductTypeId = Constants.ProductType.Dish, Wastage = 25, ScopeId = 1, UnitId = 2, RecipeTypeId = Constants.RecipeType.Choice });
            products.Add(new Product() { ProductId = nestedRecipeId, Number = 1, Quantity = 1, ProductTypeId = Constants.ProductType.Dish, Wastage = 25, ScopeId = 1, UnitId = 2, RecipeTypeId = Constants.RecipeType.Choice });

            List<ProductPart> parts = new List<ProductPart>();
            parts.Add(new ProductPart() { ProductId = nestedRecipeId, SubProductId = 1, IsChoise = true, PortionTypeId = Constants.PortionType.AP, ProductTypeId = Constants.ProductType.Ingredient, Quantity = 1, Ratio = 1, UnitId = 1 });
            parts.Add(new ProductPart() { ProductId = nestedRecipeId, SubProductId = 2, IsChoise = false, PortionTypeId = Constants.PortionType.EP, ProductTypeId = Constants.ProductType.Ingredient, Quantity = 1, Ratio = 1, UnitId = 1 });
            parts.Add(new ProductPart() { ProductId = recipeProductId, SubProductId = nestedRecipeId, IsChoise = false, PortionTypeId = Constants.PortionType.EP, ProductTypeId = Constants.ProductType.Dish, Quantity = 1, Ratio = 1, UnitId = 1 });

            var forest = new ProductForest(products, parts);
            forest.BuildForest();

            Assert.NotEmpty(forest.Forest);
            Assert.Equal(2, forest.Forest.Count);
            Assert.True(forest.Forest.ContainsKey(nestedRecipeId));
            Assert.True(forest.Forest.ContainsKey(recipeProductId));
            Assert.NotEmpty(forest.Forest[nestedRecipeId].Childs);
        }

        [Fact]
        public void NestedComplexForestTest()
        {
            var recipeProductId = 3;
            var nestedRecipeId = 4;
            var secondRecipeId = 5;

            List<Product> products = new List<Product>();
            products.Add(new Product() { ProductId = 1, Number = 1, Quantity = 1, ProductTypeId = Constants.ProductType.Ingredient, Wastage = 50, ScopeId = 1, UnitId = 1 });
            products.Add(new Product() { ProductId = 2, Number = 2, Quantity = 2, ProductTypeId = Constants.ProductType.Ingredient, Wastage = 25, ScopeId = 1, UnitId = 2 });
            products.Add(new Product() { ProductId = recipeProductId, Number = 1, Quantity = 1, ProductTypeId = Constants.ProductType.Dish, Wastage = 25, ScopeId = 1, UnitId = 2, RecipeTypeId = Constants.RecipeType.Choice });
            products.Add(new Product() { ProductId = nestedRecipeId, Number = 1, Quantity = 1, ProductTypeId = Constants.ProductType.Dish, Wastage = 25, ScopeId = 1, UnitId = 2, RecipeTypeId = Constants.RecipeType.Choice });
            products.Add(new Product() { ProductId = secondRecipeId, Number = 1, Quantity = 1, ProductTypeId = Constants.ProductType.Dish, Wastage = 25, ScopeId = 1, UnitId = 2, RecipeTypeId = Constants.RecipeType.Option });

            List<ProductPart> parts = new List<ProductPart>();
            parts.Add(new ProductPart() { ProductId = secondRecipeId, SubProductId = nestedRecipeId, IsChoise = false, PortionTypeId = Constants.PortionType.EP, ProductTypeId = Constants.ProductType.Dish, Quantity = 1, Ratio = 1, UnitId = 1 });
            parts.Add(new ProductPart() { ProductId = nestedRecipeId, SubProductId = 1, IsChoise = true, PortionTypeId = Constants.PortionType.AP, ProductTypeId = Constants.ProductType.Ingredient, Quantity = 1, Ratio = 1, UnitId = 1 });
            parts.Add(new ProductPart() { ProductId = nestedRecipeId, SubProductId = 2, IsChoise = false, PortionTypeId = Constants.PortionType.EP, ProductTypeId = Constants.ProductType.Ingredient, Quantity = 1, Ratio = 1, UnitId = 1 });
            parts.Add(new ProductPart() { ProductId = recipeProductId, SubProductId = nestedRecipeId, IsChoise = false, PortionTypeId = Constants.PortionType.EP, ProductTypeId = Constants.ProductType.Dish, Quantity = 1, Ratio = 1, UnitId = 1 });

            var forest = new ProductForest(products, parts);
            forest.BuildForest();

            Assert.NotEmpty(forest.Forest);
            Assert.Equal(3, forest.Forest.Count);
            Assert.True(forest.Forest.ContainsKey(nestedRecipeId));
            Assert.True(forest.Forest.ContainsKey(recipeProductId));
            Assert.NotEmpty(forest.Forest[nestedRecipeId].Childs);
        }

        [Fact]
        public void GetCutTest()
        {
            var recipeProductId = 3;
            var nestedRecipeId = 4;
            var secondRecipeId = 5;

            List<Product> products = new List<Product>();
            products.Add(new Product() { ProductId = 1, Number = 1, Quantity = 1, ProductTypeId = Constants.ProductType.Ingredient, Wastage = 50, ScopeId = 1, UnitId = 1 });
            products.Add(new Product() { ProductId = 2, Number = 2, Quantity = 2, ProductTypeId = Constants.ProductType.Ingredient, Wastage = 25, ScopeId = 1, UnitId = 2 });
            products.Add(new Product() { ProductId = recipeProductId, Number = 1, Quantity = 1, ProductTypeId = Constants.ProductType.Dish, Wastage = 25, ScopeId = 1, UnitId = 2, RecipeTypeId = Constants.RecipeType.Choice });
            products.Add(new Product() { ProductId = nestedRecipeId, Number = 1, Quantity = 1, ProductTypeId = Constants.ProductType.Dish, Wastage = 25, ScopeId = 1, UnitId = 2, RecipeTypeId = Constants.RecipeType.Choice });
            products.Add(new Product() { ProductId = secondRecipeId, Number = 1, Quantity = 1, ProductTypeId = Constants.ProductType.Dish, Wastage = 25, ScopeId = 1, UnitId = 2, RecipeTypeId = Constants.RecipeType.Option });
            products.Add(new Product() { ProductId = 7, Number = 1, Quantity = 1, ProductTypeId = Constants.ProductType.Dish, Wastage = 25, ScopeId = 1, UnitId = 2, RecipeTypeId = Constants.RecipeType.Option });

            List<ProductPart> parts = new List<ProductPart>();
            parts.Add(new ProductPart() { ProductId = secondRecipeId, SubProductId = nestedRecipeId, IsChoise = false, PortionTypeId = Constants.PortionType.EP, ProductTypeId = Constants.ProductType.Dish, Quantity = 1, Ratio = 1, UnitId = 1 });
            parts.Add(new ProductPart() { ProductId = nestedRecipeId, SubProductId = 1, IsChoise = true, PortionTypeId = Constants.PortionType.AP, ProductTypeId = Constants.ProductType.Ingredient, Quantity = 1, Ratio = 1, UnitId = 1 });
            parts.Add(new ProductPart() { ProductId = nestedRecipeId, SubProductId = 2, IsChoise = false, PortionTypeId = Constants.PortionType.EP, ProductTypeId = Constants.ProductType.Ingredient, Quantity = 1, Ratio = 1, UnitId = 1 });
            parts.Add(new ProductPart() { ProductId = recipeProductId, SubProductId = nestedRecipeId, IsChoise = false, PortionTypeId = Constants.PortionType.EP, ProductTypeId = Constants.ProductType.Dish, Quantity = 1, Ratio = 1, UnitId = 1 });

            parts.Add(new ProductPart() { ProductId = 7, SubProductId = 1, IsChoise = true, PortionTypeId = Constants.PortionType.AP, ProductTypeId = Constants.ProductType.Ingredient, Quantity = 1, Ratio = 1, UnitId = 1 });
            parts.Add(new ProductPart() { ProductId = 7, SubProductId = 2, IsChoise = false, PortionTypeId = Constants.PortionType.EP, ProductTypeId = Constants.ProductType.Ingredient, Quantity = 1, Ratio = 1, UnitId = 1 });

            var forest = new ProductForest(products, parts);
            forest.BuildForest();

            Assert.NotEmpty(forest.Forest);
            Assert.Equal(4, forest.Forest.Count);
            Assert.True(forest.Forest.ContainsKey(nestedRecipeId));
            Assert.True(forest.Forest.ContainsKey(recipeProductId));
            Assert.NotEmpty(forest.Forest[nestedRecipeId].Childs);

            var res = forest.GetAffectedCuts(nestedRecipeId);
            Assert.NotEmpty(res);
            Assert.Equal(3, res.Count);
            Assert.True(res.ContainsKey(recipeProductId));
            Assert.Single(res[recipeProductId].Childs);
            Assert.True(res.ContainsKey(nestedRecipeId));
            Assert.Equal(2, res[nestedRecipeId].Childs.Count);
        }

        [Fact]
        public void CalculatePrices()
        {
            var recipeProductId = 3;
            var nestedRecipeId = 4;
            var secondRecipeId = 5;

            List<Product> products = new List<Product>();
            products.Add(new Product() { ProductId = 1, Number = 1, Quantity = 1, ProductTypeId = Constants.ProductType.Ingredient, Wastage = 50, ScopeId = 1, UnitId = 1 });
            products.Add(new Product() { ProductId = 2, Number = 2, Quantity = 2, ProductTypeId = Constants.ProductType.Ingredient, Wastage = 25, ScopeId = 1, UnitId = 2 });
            products.Add(new Product() { ProductId = recipeProductId, Number = 1, Quantity = 1, ProductTypeId = Constants.ProductType.Dish, Wastage = 25, ScopeId = 1, UnitId = 2, RecipeTypeId = Constants.RecipeType.Choice });
            products.Add(new Product() { ProductId = nestedRecipeId, Number = 1, Quantity = 1, ProductTypeId = Constants.ProductType.Dish, Wastage = 25, ScopeId = 1, UnitId = 2, RecipeTypeId = Constants.RecipeType.Choice });
            products.Add(new Product() { ProductId = secondRecipeId, Number = 1, Quantity = 1, ProductTypeId = Constants.ProductType.Dish, Wastage = 25, ScopeId = 1, UnitId = 2, RecipeTypeId = Constants.RecipeType.Option });

            products.Add(new Product() { ProductId = 6, Number = 1, Quantity = 1, ProductTypeId = Constants.ProductType.Ingredient, Wastage = 50, ScopeId = 3, UnitId = 1 });
            products.Add(new Product() { ProductId = 7, Number = 1, Quantity = 1, ProductTypeId = Constants.ProductType.Dish, Wastage = 25, ScopeId = 1, UnitId = 2, RecipeTypeId = Constants.RecipeType.Batch });
            List<ProductPart> parts = new List<ProductPart>();
            parts.Add(new ProductPart() { ProductId = secondRecipeId, SubProductId = nestedRecipeId, IsChoise = false, PortionTypeId = Constants.PortionType.EP, ProductTypeId = Constants.ProductType.Dish, Quantity = 1, Ratio = 1, UnitId = 1 });
            parts.Add(new ProductPart() { ProductId = nestedRecipeId, SubProductId = 1, IsChoise = true, PortionTypeId = Constants.PortionType.AP, ProductTypeId = Constants.ProductType.Ingredient, Quantity = 1, Ratio = 1, UnitId = 1 });
            parts.Add(new ProductPart() { ProductId = nestedRecipeId, SubProductId = 2, IsChoise = false, PortionTypeId = Constants.PortionType.EP, ProductTypeId = Constants.ProductType.Ingredient, Quantity = 1, Ratio = 1, UnitId = 1 });
            parts.Add(new ProductPart() { ProductId = recipeProductId, SubProductId = nestedRecipeId, IsChoise = false, PortionTypeId = Constants.PortionType.EP, ProductTypeId = Constants.ProductType.Dish, Quantity = 1, Ratio = 1, UnitId = 1 });

            var forest = new ProductForest(products, parts);
            forest.BuildForest();

            Assert.NotEmpty(forest.Forest);
            Assert.Equal(4, forest.Forest.Count);
            Assert.True(forest.Forest.ContainsKey(nestedRecipeId));
            Assert.True(forest.Forest.ContainsKey(recipeProductId));
            Assert.NotEmpty(forest.Forest[nestedRecipeId].Childs);

            List<ProductGroupPrice> groupPrices = new List<ProductGroupPrice>();
            groupPrices.Add(new ProductGroupPrice() { GroupId = 1, Price = 1, ProductId = 1 });
            groupPrices.Add(new ProductGroupPrice() { GroupId = 1, Price = 1, ProductId = 2 });
            groupPrices.Add(new ProductGroupPrice() { GroupId = 1, ProductId = 3 });
            groupPrices.Add(new ProductGroupPrice() { GroupId = 1, ProductId = 4 });
            groupPrices.Add(new ProductGroupPrice() { GroupId = 1, ProductId = 5 });
            groupPrices.Add(new ProductGroupPrice() { GroupId = 0, ProductId = 6, Price = 1 });
            groupPrices.Add(new ProductGroupPrice() { ProductId = 7});

            var result = forest.CalculatePrice(groupPrices, false);
            Assert.NotEmpty(result);
            Assert.True(result.ContainsKey(0));
            //private and non group items = 2
            Assert.Equal(2, result[0].Prices.Count);
        }
    }
}
