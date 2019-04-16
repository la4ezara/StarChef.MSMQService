using System;
using Xunit;
using StarChef.Common.Hierarchy;
using System.Collections.Generic;
using Fourth.StarChef.Invariables;
using StarChef.Common.Model;

namespace StarChef.Common.Tests
{
    public class ProductNodeTests
    {
        [Fact]
        public void ConstructorTestOne()
        {
            ProductNode node = new ProductNode(1,1,1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient);

            Assert.Equal(1, node.ProductId);
            Assert.Equal(1, node.Quantity);
            Assert.Equal(1, node.UnitId);
            Assert.Equal(Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, node.NodeType);
        }


        [Fact]
        public void ConstructorTestTwo()
        {
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);

            Assert.Equal(1, node.ProductId);
            Assert.Equal(1, node.Quantity);
            Assert.Equal(1, node.UnitId);
            Assert.Equal(2, node.Ratio);
            Assert.Equal(Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, node.NodeType);
            Assert.Equal(Fourth.StarChef.Invariables.Constants.PortionType.AP, node.Portion);
        }

        [Fact]
        public void PriceRecalcBaseNoAccess()
        {
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            HashSet<int> accessList = new HashSet<int>();

            var result = node.GetPrice(priceStorate, forest, accessList);
            Assert.Null(result);
        }

        [Fact]
        public void PriceRecalcBaseIsBroken()
        {
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            HashSet<int> accessList = new HashSet<int>();
            node.IsBroken = true;
            var result = node.GetPrice(priceStorate, forest, accessList);
            Assert.Null(result);
        }

        [Fact]
        public void PriceRecalcBaseAlreadyCalculatedPrice()
        {
            decimal expectedPrice = 2.546m;
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            priceStorate.Add(node.ProductId, expectedPrice);
            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            var result = node.GetPrice(priceStorate, forest, accessList);
            Assert.NotNull(result);
            Assert.Equal(expectedPrice, result);
        }

        [Fact]
        public void PriceRecalcBaseIngredientZeroPrice()
        {
            decimal expectedPrice = 0m;
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            var result = node.GetPrice(priceStorate, forest, accessList);
            Assert.NotNull(result);
            Assert.Equal(expectedPrice, result);
            Assert.Empty(priceStorate);
        }

        [Fact]
        public void PriceRecalcBaseRecipeMissingType()
        {
            decimal expectedPrice = 0m;
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            var result = node.GetPrice(priceStorate, forest, accessList);
            Assert.NotNull(result);
            Assert.Equal(expectedPrice, result);
            Assert.NotEmpty(priceStorate);
            Assert.True(priceStorate.ContainsKey(node.ProductId));
            Assert.Equal(expectedPrice, priceStorate[node.ProductId]);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseNoChilds()
        {
            decimal expectedPrice = 0m;
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            node.RecipeKind = Fourth.StarChef.Invariables.Constants.RecipeType.Choice;
            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            var result = node.GetPrice(priceStorate, forest, accessList);
            Assert.NotNull(result);
            Assert.Equal(expectedPrice, result);
            Assert.NotEmpty(priceStorate);
            Assert.True(priceStorate.ContainsKey(node.ProductId));
            Assert.Equal(expectedPrice, priceStorate[node.ProductId]);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseChildNoDefault()
        {
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            node.RecipeKind = Fourth.StarChef.Invariables.Constants.RecipeType.Choice;
            node.Childs.Add(new ProductNode(2, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient));

            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            var result = node.GetPrice(priceStorate, forest, accessList);
            Assert.Null(result);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseChildNoAccess()
        {
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            node.RecipeKind = Fourth.StarChef.Invariables.Constants.RecipeType.Choice;
            var childNode = new ProductNode(2, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient);
            childNode.IsChoise = true;
            childNode.IsBroken = true;
            node.Childs.Add(childNode);

            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            var result = node.GetPrice(priceStorate, forest, accessList);
            Assert.Null(result);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseChildBroken()
        {
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            node.RecipeKind = Fourth.StarChef.Invariables.Constants.RecipeType.Choice;
            var childNode = new ProductNode(2, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient);
            childNode.IsChoise = true;
            childNode.IsBroken = true;
            node.Childs.Add(childNode);

            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            accessList.Add(childNode.ProductId);
            var result = node.GetPrice(priceStorate, forest, accessList);
            Assert.Null(result);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseThrowProductException()
        {
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            node.RecipeKind = Fourth.StarChef.Invariables.Constants.RecipeType.Choice;
            var childNode = new ProductNode(2, 2, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 1, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            childNode.IsChoise = true;
            node.Childs.Add(childNode);

            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            accessList.Add(childNode.ProductId);

            Exception ex = Assert.Throws<KeyNotFoundException>(() => node.GetPrice(priceStorate, forest, accessList));
            Assert.NotNull(ex);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseThrowPriceStorageException()
        {
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            node.RecipeKind = Fourth.StarChef.Invariables.Constants.RecipeType.Choice;
            var childNode = new ProductNode(2, 2, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 1, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            childNode.IsChoise = true;
            node.Childs.Add(childNode);

            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            forest.Add(childNode.ProductId, new Product() { ProductId = childNode.ProductId, Wastage = 0, ScopeId = 1, Quantity = 1, UnitId = 1 });
            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            accessList.Add(childNode.ProductId);

            Exception ex = Assert.Throws<KeyNotFoundException>(() => node.GetPrice(priceStorate, forest, accessList));
            Assert.NotNull(ex);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseChildAp()
        {
            decimal expectedPrice = 4;
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            node.RecipeKind = Fourth.StarChef.Invariables.Constants.RecipeType.Choice;
            var childNode = new ProductNode(2, 2, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 1, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            childNode.IsChoise = true;
            node.Childs.Add(childNode);

            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            priceStorate.Add(childNode.ProductId, 8);
            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            forest.Add(childNode.ProductId, new Product() { ProductId = childNode.ProductId, Wastage = 0, ScopeId = 1, Quantity = 1, UnitId = 1, Number = 2 });
            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            accessList.Add(childNode.ProductId);

            var result = node.GetPrice(priceStorate, forest, accessList);
            Assert.NotNull(result);
            Assert.Equal(expectedPrice, result);
            Assert.NotEmpty(priceStorate);
            Assert.True(priceStorate.ContainsKey(node.ProductId));
            Assert.Equal(expectedPrice, priceStorate[node.ProductId]);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseChildEp()
        {
            decimal expectedPrice = 8;
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            node.RecipeKind = Fourth.StarChef.Invariables.Constants.RecipeType.Choice;
            var childNode = new ProductNode(2, 2, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 1, Fourth.StarChef.Invariables.Constants.PortionType.EP);
            childNode.IsChoise = true;
            node.Childs.Add(childNode);

            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            priceStorate.Add(childNode.ProductId, 8);
            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            forest.Add(childNode.ProductId, new Product() { ProductId = childNode.ProductId, Wastage = 50, ScopeId = 1, Quantity = 1, UnitId = 1, Number = 2 });
            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            accessList.Add(childNode.ProductId);

            var result = node.GetPrice(priceStorate, forest, accessList);
            Assert.NotNull(result);
            Assert.Equal(expectedPrice, result);
            Assert.NotEmpty(priceStorate);
            Assert.True(priceStorate.ContainsKey(node.ProductId));
            Assert.Equal(expectedPrice, priceStorate[node.ProductId]);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseChildEpInvalidWastage()
        {
            decimal expectedPrice = 4;
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            node.RecipeKind = Fourth.StarChef.Invariables.Constants.RecipeType.Choice;
            var childNode = new ProductNode(2, 2, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 1, Fourth.StarChef.Invariables.Constants.PortionType.EP);
            childNode.IsChoise = true;
            node.Childs.Add(childNode);

            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            priceStorate.Add(childNode.ProductId, 8);
            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            forest.Add(childNode.ProductId, new Product() { ProductId = childNode.ProductId, Wastage = -50, ScopeId = 1, Quantity = 1, UnitId = 1, Number = 2 });
            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            accessList.Add(childNode.ProductId);

            var result = node.GetPrice(priceStorate, forest, accessList);
            Assert.NotNull(result);
            Assert.Equal(expectedPrice, result);
            Assert.NotEmpty(priceStorate);
            Assert.True(priceStorate.ContainsKey(node.ProductId));
            Assert.Equal(expectedPrice, priceStorate[node.ProductId]);

            forest[childNode.ProductId].Wastage = 110;

            result = node.GetPrice(priceStorate, forest, accessList);
            Assert.NotNull(result);
            Assert.Equal(expectedPrice, result);
            Assert.NotEmpty(priceStorate);
            Assert.True(priceStorate.ContainsKey(node.ProductId));
            Assert.Equal(expectedPrice, priceStorate[node.ProductId]);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseChildRecipeNoAccess()
        {
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            node.RecipeKind = Fourth.StarChef.Invariables.Constants.RecipeType.Choice;
            var childNode = new ProductNode(2, 2, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 1, Fourth.StarChef.Invariables.Constants.PortionType.EP);
            childNode.RecipeKind = Fourth.StarChef.Invariables.Constants.RecipeType.Batch;
            childNode.IsChoise = true;

            var subChildOne = new ProductNode(3, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 2, Fourth.StarChef.Invariables.Constants.PortionType.EP);
            var subChildTwo = new ProductNode(4, 2, 2, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 3, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            childNode.Childs.Add(subChildOne);
            childNode.Childs.Add(subChildTwo);

            node.Childs.Add(childNode);

            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            
            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            forest.Add(childNode.ProductId, new Product() { ProductId = childNode.ProductId, Wastage = 50, ScopeId = 1, Quantity = 1, UnitId = 1, Number = 2 });
            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            accessList.Add(childNode.ProductId);

            var result = node.GetPrice(priceStorate, forest, accessList);
            Assert.Null(result);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseChildRecipe()
        {
            decimal expectedPrice = 2;
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            node.RecipeKind = Fourth.StarChef.Invariables.Constants.RecipeType.Choice;
            var childNode = new ProductNode(2, 2, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 1, Fourth.StarChef.Invariables.Constants.PortionType.EP);
            childNode.RecipeKind = Fourth.StarChef.Invariables.Constants.RecipeType.Batch;
            childNode.IsChoise = true;

            var subChildOne = new ProductNode(3, 2, 2, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 2, Fourth.StarChef.Invariables.Constants.PortionType.EP);
            var subChildTwo = new ProductNode(4, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 1, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            childNode.Childs.Add(subChildOne);
            childNode.Childs.Add(subChildTwo);

            node.Childs.Add(childNode);

            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            priceStorate.Add(subChildOne.ProductId, 2);
            priceStorate.Add(subChildTwo.ProductId, 4);

            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            forest.Add(childNode.ProductId, new Product() { ProductId = childNode.ProductId, Wastage = 0, ScopeId = 1, Quantity = 1, UnitId = 1, Number = 2 });
            forest.Add(subChildOne.ProductId, new Product() { ProductId = subChildOne.ProductId, Wastage = 50, ScopeId = 1, Quantity = 1, UnitId = 1, Number = 2 });
            forest.Add(subChildTwo.ProductId, new Product() { ProductId = subChildTwo.ProductId, Wastage = 50, ScopeId = 1, Quantity = 1, UnitId = 1, Number = 2 });

            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            accessList.Add(childNode.ProductId);
            accessList.Add(subChildOne.ProductId);
            accessList.Add(subChildTwo.ProductId);

            var result = node.GetPrice(priceStorate, forest, accessList);
            Assert.NotNull(result);
            Assert.Equal(expectedPrice, result);
            Assert.NotEmpty(priceStorate);
            Assert.True(priceStorate.ContainsKey(node.ProductId));
            Assert.Equal(expectedPrice, priceStorate[node.ProductId]);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseChildRecipeBroken()
        {
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            node.RecipeKind = Fourth.StarChef.Invariables.Constants.RecipeType.Choice;
            var childNode = new ProductNode(2, 2, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 1, Fourth.StarChef.Invariables.Constants.PortionType.EP);
            childNode.RecipeKind = Fourth.StarChef.Invariables.Constants.RecipeType.Choice;
            childNode.IsChoise = true;

            var subChildOne = new ProductNode(3, 2, 2, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 2, Fourth.StarChef.Invariables.Constants.PortionType.EP);
            var subChildTwo = new ProductNode(4, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 1, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            childNode.Childs.Add(subChildOne);
            childNode.Childs.Add(subChildTwo);
            subChildOne.IsBroken = true;
            node.Childs.Add(childNode);

            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            priceStorate.Add(subChildOne.ProductId, 2);
            priceStorate.Add(subChildTwo.ProductId, 4);

            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            forest.Add(childNode.ProductId, new Product() { ProductId = childNode.ProductId, Wastage = 0, ScopeId = 1, Quantity = 1, UnitId = 1, Number = 2 });
            forest.Add(subChildOne.ProductId, new Product() { ProductId = subChildOne.ProductId, Wastage = 50, ScopeId = 1, Quantity = 1, UnitId = 1, Number = 2 });
            forest.Add(subChildTwo.ProductId, new Product() { ProductId = subChildTwo.ProductId, Wastage = 50, ScopeId = 1, Quantity = 1, UnitId = 1, Number = 2 });

            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            accessList.Add(childNode.ProductId);
            accessList.Add(subChildOne.ProductId);
            accessList.Add(subChildTwo.ProductId);

            var result = node.GetPrice(priceStorate, forest, accessList);
            Assert.Null(result);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseChildRecipeOption()
        {
            decimal expectedPrice = 0;
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            node.RecipeKind = Fourth.StarChef.Invariables.Constants.RecipeType.Choice;
            var childNode = new ProductNode(2, 2, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 1, Fourth.StarChef.Invariables.Constants.PortionType.EP);
            childNode.RecipeKind = Fourth.StarChef.Invariables.Constants.RecipeType.Option;
            childNode.IsChoise = true;

            var subChildOne = new ProductNode(3, 2, 2, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 2, Fourth.StarChef.Invariables.Constants.PortionType.EP);
            var subChildTwo = new ProductNode(4, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 1, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            childNode.Childs.Add(subChildOne);
            childNode.Childs.Add(subChildTwo);

            node.Childs.Add(childNode);

            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            priceStorate.Add(subChildOne.ProductId, 2);
            priceStorate.Add(subChildTwo.ProductId, 4);

            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            forest.Add(childNode.ProductId, new Product() { ProductId = childNode.ProductId, Wastage = 0, ScopeId = 1, Quantity = 1, UnitId = 1, Number = 2 });
            forest.Add(subChildOne.ProductId, new Product() { ProductId = subChildOne.ProductId, Wastage = 50, ScopeId = 1, Quantity = 1, UnitId = 1, Number = 2 });
            forest.Add(subChildTwo.ProductId, new Product() { ProductId = subChildTwo.ProductId, Wastage = 50, ScopeId = 1, Quantity = 1, UnitId = 1, Number = 2 });

            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            accessList.Add(childNode.ProductId);
            accessList.Add(subChildOne.ProductId);
            accessList.Add(subChildTwo.ProductId);

            var result = node.GetPrice(priceStorate, forest, accessList);
            Assert.NotNull(result);
            Assert.Equal(expectedPrice, result);
            Assert.NotEmpty(priceStorate);
            Assert.True(priceStorate.ContainsKey(node.ProductId));
            Assert.Equal(expectedPrice, priceStorate[node.ProductId]);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseChildRecipeOptionBroken()
        {
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            node.RecipeKind = Fourth.StarChef.Invariables.Constants.RecipeType.Option;
            var childNode = new ProductNode(2, 2, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 1, Fourth.StarChef.Invariables.Constants.PortionType.EP);
            childNode.RecipeKind = Fourth.StarChef.Invariables.Constants.RecipeType.Option;
            childNode.IsChoise = true;
            
            var subChildOne = new ProductNode(3, 2, 2, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 2, Fourth.StarChef.Invariables.Constants.PortionType.EP);
            subChildOne.IsBroken = true;
            var subChildTwo = new ProductNode(4, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 1, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            childNode.Childs.Add(subChildOne);
            childNode.Childs.Add(subChildTwo);

            node.Childs.Add(childNode);

            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            priceStorate.Add(subChildOne.ProductId, 2);
            priceStorate.Add(subChildTwo.ProductId, 4);

            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            forest.Add(childNode.ProductId, new Product() { ProductId = childNode.ProductId, Wastage = 0, ScopeId = 1, Quantity = 1, UnitId = 1, Number = 2 });
            forest.Add(subChildOne.ProductId, new Product() { ProductId = subChildOne.ProductId, Wastage = 50, ScopeId = 1, Quantity = 1, UnitId = 1, Number = 2 });
            forest.Add(subChildTwo.ProductId, new Product() { ProductId = subChildTwo.ProductId, Wastage = 50, ScopeId = 1, Quantity = 1, UnitId = 1, Number = 2 });

            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            accessList.Add(childNode.ProductId);
            accessList.Add(subChildTwo.ProductId);

            var result = node.GetPrice(priceStorate, forest, accessList);
            Assert.Null(result);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseOption()
        {
            decimal expectedPrice = 0m;
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            node.RecipeKind = Fourth.StarChef.Invariables.Constants.RecipeType.Option;
            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = 1, Wastage = 0, ScopeId = 1 });
            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            var result = node.GetPrice(priceStorate, forest, accessList);
            Assert.NotNull(result);
            Assert.Equal(expectedPrice, result);
            Assert.NotEmpty(priceStorate);
            Assert.True(priceStorate.ContainsKey(node.ProductId));
            Assert.Equal(expectedPrice, priceStorate[node.ProductId]);
        }

        [Fact]
        public void PriceRecalcBaseRecipe_RestrictBySupplier()
        {
            //core scenario where we have access to used ingredient and no list with alternates
            var ingredientA = new ProductNode(3, 2, 2, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 2, Fourth.StarChef.Invariables.Constants.PortionType.EP);
            var ingredientB = new ProductNode(4, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 1, Fourth.StarChef.Invariables.Constants.PortionType.AP);

            var childNode = new ProductNode(2, 2, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 1, Fourth.StarChef.Invariables.Constants.PortionType.EP);
            childNode.RecipeKind = Constants.RecipeType.Batch;
            childNode.Childs.Add(ingredientA);
            childNode.Childs.Add(ingredientB);

            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            node.RecipeKind = Constants.RecipeType.Standard;
            node.Childs.Add(childNode);

            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            priceStorate.Add(ingredientA.ProductId, 2);
            priceStorate.Add(ingredientB.ProductId, 4);

            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            forest.Add(childNode.ProductId, new Product() { ProductId = childNode.ProductId, Wastage = 0, ScopeId = 1, Quantity = 1, UnitId = 1, Number = 2 });
            forest.Add(ingredientA.ProductId, new Product() { ProductId = ingredientA.ProductId, Wastage = 50, ScopeId = 1, Quantity = 1, UnitId = 1, Number = 2 });
            forest.Add(ingredientB.ProductId, new Product() { ProductId = ingredientB.ProductId, Wastage = 50, ScopeId = 1, Quantity = 1, UnitId = 1, Number = 2 });

            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            accessList.Add(childNode.ProductId);
            accessList.Add(ingredientA.ProductId);
            accessList.Add(ingredientB.ProductId);

            List<IngredientAlternate> alternates = new List<IngredientAlternate>();

            var result = node.GetPrice(priceStorate, forest, accessList,true, alternates);
            //expected price of node recipe is 2
            Assert.Equal(result,2);
        }

        [Fact]
        public void PriceRecalcBaseRecipe_RestrictBySupplierWithMainIngredient()
        {
            //core scenario where we have access to used ingredient and no list with alternates
            var ingredientA = new ProductNode(3, 2, 2, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 2, Fourth.StarChef.Invariables.Constants.PortionType.EP);
            var ingredientB = new ProductNode(4, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 1, Fourth.StarChef.Invariables.Constants.PortionType.AP);

            var childNode = new ProductNode(2, 2, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 1, Fourth.StarChef.Invariables.Constants.PortionType.EP);
            childNode.RecipeKind = Constants.RecipeType.Batch;
            childNode.Childs.Add(ingredientA);
            childNode.Childs.Add(ingredientB);

            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            node.RecipeKind = Constants.RecipeType.Standard;
            node.Childs.Add(childNode);

            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            priceStorate.Add(ingredientA.ProductId, 2);
            priceStorate.Add(ingredientB.ProductId, 4);

            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            forest.Add(childNode.ProductId, new Product() { ProductId = childNode.ProductId, Wastage = 0, ScopeId = 1, Quantity = 1, UnitId = 1, Number = 2 });
            forest.Add(ingredientA.ProductId, new Product() { ProductId = ingredientA.ProductId, Wastage = 50, ScopeId = 1, Quantity = 1, UnitId = 1, Number = 2 });
            forest.Add(ingredientB.ProductId, new Product() { ProductId = ingredientB.ProductId, Wastage = 50, ScopeId = 1, Quantity = 1, UnitId = 1, Number = 2 });
            
            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            accessList.Add(childNode.ProductId);
            accessList.Add(ingredientA.ProductId);
            accessList.Add(ingredientB.ProductId);

            List<IngredientAlternate> alternates = new List<IngredientAlternate>();
            //create list with alternates of productid 4 - productid 4 is main ingredient
            var alternateOfMainB = new IngredientAlternate() { ProductId = 4, AlternateProductId = 5, AlternateRank = 1, Ratio = 1 };
            alternates.Add(alternateOfMainB);
            //add alternate ProductId = 5 to access list and price
            accessList.Add(alternateOfMainB.AlternateProductId);
            priceStorate.Add(alternateOfMainB.AlternateProductId, 1);
            forest.Add(alternateOfMainB.AlternateProductId, new Product() { ProductId = alternateOfMainB.AlternateProductId, Wastage = 10, ScopeId = 1, Quantity = 1, UnitId = 3, Number = 2 });

            //remove productId=4 from access list
            accessList.Remove(ingredientB.ProductId);

            var result = node.GetPrice(priceStorate, forest, accessList, true, alternates);

            //expected price of node recipe is 2
            Assert.Equal(result, 1.25m);

            //reset test
            //productid4 is alternate which does not have access -
            priceStorate.Remove(node.ProductId);
            priceStorate.Remove(childNode.ProductId);

            priceStorate.Remove(alternateOfMainB.AlternateProductId);
            accessList.Remove(alternateOfMainB.AlternateProductId);
            forest.Remove(alternateOfMainB.AlternateProductId);
            alternates = new List<IngredientAlternate>();

            var alternateOfB = new IngredientAlternate() { ProductId = 5, AlternateProductId = 4, AlternateRank = 1, Ratio = 1 };
            alternates.Add(alternateOfB);
            //add alternate ProductId = 5 to access list and price
            accessList.Add(alternateOfB.ProductId);
            priceStorate.Add(alternateOfB.ProductId, 1);
            forest.Add(alternateOfB.ProductId, new Product() { ProductId = alternateOfB.ProductId, Wastage = 10, ScopeId = 1, Quantity = 1, UnitId = 3, Number = 2 });

            var alternateOfBAlternate = new IngredientAlternate() { ProductId = 5, AlternateProductId = 6, AlternateRank = 1, Ratio = 0.25m };
            alternates.Add(alternateOfBAlternate);
            //add alternate ProductId = 5 to access list and price
            accessList.Add(alternateOfBAlternate.AlternateProductId);
            priceStorate.Add(alternateOfBAlternate.AlternateProductId, 2);
            forest.Add(alternateOfBAlternate.AlternateProductId, new Product() { ProductId = alternateOfBAlternate.AlternateProductId, Wastage = 10, ScopeId = 1, Quantity = 1, UnitId = 3, Number = 2 });

            result = node.GetPrice(priceStorate, forest, accessList, true, alternates);

            //expected price of node recipe is 2
            Assert.Equal(result, 3);

            //reset test
            //productid4 is broken
            priceStorate.Remove(node.ProductId);
            priceStorate.Remove(childNode.ProductId);

            priceStorate.Remove(alternateOfB.ProductId);
            accessList.Remove(alternateOfB.ProductId);
            forest.Remove(alternateOfB.ProductId);

            priceStorate.Remove(alternateOfBAlternate.AlternateProductId);
            accessList.Remove(alternateOfBAlternate.AlternateProductId);
            forest.Remove(alternateOfBAlternate.AlternateProductId);

            alternates = new List<IngredientAlternate>();
            ingredientB.IsBroken = true;

            result = node.GetPrice(priceStorate, forest, accessList, true, alternates);
            //expected price not calculated
            Assert.Null(result);

            //reset test
            ingredientB.IsBroken =false;
            accessList.Remove(ingredientB.ProductId);
            result = node.GetPrice(priceStorate, forest, accessList, true, alternates);

            //expected price not calculated
            Assert.Null(result);

            //reset test
            accessList.Add(ingredientB.ProductId);
            //alternates.Add(new IngredientAlternate() { ProductId = 1, AlternateProductId = 2, Ratio = 3});
            result = node.GetPrice(priceStorate, forest, accessList, true, null);

            //expected price not calculated
            Assert.Equal(result, 2);

            //reset test
            priceStorate.Remove(node.ProductId);
            priceStorate.Remove(childNode.ProductId);

            //create list with alternates of productid 4 - productid 4 is main ingredient
            alternateOfMainB = new IngredientAlternate() { ProductId = 5, AlternateProductId = 4, AlternateRank = 1, Ratio = 1 };
            alternates.Add(alternateOfMainB);
            //add alternate ProductId = 5 to access list and price
            accessList.Add(alternateOfMainB.ProductId);
            priceStorate.Add(alternateOfMainB.ProductId, 1);
            forest.Add(alternateOfMainB.ProductId, new Product() { ProductId = alternateOfMainB.ProductId, Wastage = 10, ScopeId = 1, Quantity = 1, UnitId = 3, Number = 2 });

            result = node.GetPrice(priceStorate, forest, accessList, true, alternates);

            //expected price of node recipe is 2
            Assert.Equal(result, 2);

        }
    }
}
