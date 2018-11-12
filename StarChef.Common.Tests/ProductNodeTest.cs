using System;
using Xunit;
using StarChef.Common.Hierarchy;
using System.Collections.Generic;
using StarChef.Common.Model;

namespace StarChef.Common.Tests
{
    
    public class ProductNodeTest
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
            decimal expected = 2;
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
            decimal expected = 2;
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
            decimal expectedPrice = 8;
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
            decimal expectedPrice = 2;
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
            decimal expectedPrice = 0;
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
            //accessList.Add(subChildOne.ProductId);
            accessList.Add(subChildTwo.ProductId);

            var result = node.GetPrice(priceStorate, forest, accessList);
            Assert.Null(result);
            //Assert.Equal(expectedPrice, result);
            //Assert.NotEmpty(priceStorate);
            //Assert.True(priceStorate.ContainsKey(node.ProductId));
            //Assert.Equal(expectedPrice, priceStorate[node.ProductId]);
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

    }
}
