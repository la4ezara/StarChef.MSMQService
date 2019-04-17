using System;
using Xunit;
using StarChef.Common.Hierarchy;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Text;
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
            StringBuilder sbErrors = new StringBuilder();
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            HashSet<int> accessList = new HashSet<int>();

            var result = node.GetPrice(priceStorate, forest, accessList, sbErrors);
            Assert.Null(result);
            Assert.Equal(0, sbErrors.Length);
        }

        [Fact]
        public void PriceRecalcBaseIsBroken()
        {
            StringBuilder sbErrors = new StringBuilder();
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            HashSet<int> accessList = new HashSet<int>();
            node.IsBroken = true;
            var result = node.GetPrice(priceStorate, forest, accessList, sbErrors);
            Assert.Null(result);
            Assert.Equal(0, sbErrors.Length);
        }

        [Fact]
        public void PriceRecalcBaseAlreadyCalculatedPrice()
        {
            StringBuilder sbErrors = new StringBuilder();
            decimal expectedPrice = 2.546m;
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            priceStorate.Add(node.ProductId, expectedPrice);
            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            var result = node.GetPrice(priceStorate, forest, accessList, sbErrors);
            Assert.NotNull(result);
            Assert.Equal(expectedPrice, result);
            Assert.Equal(0, sbErrors.Length);
        }

        [Fact]
        public void PriceRecalcBaseIngredientZeroPrice()
        {
            StringBuilder sbErrors = new StringBuilder();
            decimal expectedPrice = 0m;
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            var result = node.GetPrice(priceStorate, forest, accessList, sbErrors);
            Assert.NotNull(result);
            Assert.Equal(expectedPrice, result);
            Assert.Empty(priceStorate);
            Assert.Equal(0, sbErrors.Length);
        }

        [Fact]
        public void PriceRecalcBaseRecipeMissingType()
        {
            StringBuilder sbErrors = new StringBuilder();
            decimal expectedPrice = 0m;
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            var result = node.GetPrice(priceStorate, forest, accessList, sbErrors);
            Assert.NotNull(result);
            Assert.Equal(expectedPrice, result);
            Assert.NotEmpty(priceStorate);
            Assert.True(priceStorate.ContainsKey(node.ProductId));
            Assert.Equal(expectedPrice, priceStorate[node.ProductId]);
            Assert.Equal(0, sbErrors.Length);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseNoChilds()
        {
            StringBuilder sbErrors = new StringBuilder();
            decimal expectedPrice = 0m;
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            node.RecipeKind = Fourth.StarChef.Invariables.Constants.RecipeType.Choice;
            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            var result = node.GetPrice(priceStorate, forest, accessList, sbErrors);
            Assert.NotNull(result);
            Assert.Equal(expectedPrice, result);
            Assert.NotEmpty(priceStorate);
            Assert.True(priceStorate.ContainsKey(node.ProductId));
            Assert.Equal(expectedPrice, priceStorate[node.ProductId]);
            Assert.Equal(0, sbErrors.Length);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseChildNoDefault()
        {
            StringBuilder sbErrors = new StringBuilder();
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            node.RecipeKind = Fourth.StarChef.Invariables.Constants.RecipeType.Choice;
            node.Childs.Add(new ProductNode(2, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Ingredient));

            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = node.ProductId, Wastage = 0, ScopeId = 1 });
            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            var result = node.GetPrice(priceStorate, forest, accessList, sbErrors);
            Assert.Null(result);
            Assert.Equal(0, sbErrors.Length);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseChildNoAccess()
        {
            StringBuilder sbErrors = new StringBuilder();
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
            var result = node.GetPrice(priceStorate, forest, accessList, sbErrors);
            Assert.Null(result);
            Assert.Equal(0, sbErrors.Length);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseChildBroken()
        {
            StringBuilder sbErrors = new StringBuilder();
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
            var result = node.GetPrice(priceStorate, forest, accessList, sbErrors);
            Assert.Null(result);
            Assert.Equal(0, sbErrors.Length);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseThrowProductException()
        {
            StringBuilder sbErrors = new StringBuilder();
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

            Exception ex = Assert.Throws<KeyNotFoundException>(() => node.GetPrice(priceStorate, forest, accessList, sbErrors));
            Assert.NotNull(ex);
            Assert.NotEqual(0, sbErrors.Length);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseThrowPriceStorageException()
        {
            StringBuilder sbErrors = new StringBuilder();
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

            Exception ex = Assert.Throws<KeyNotFoundException>(() => node.GetPrice(priceStorate, forest, accessList, sbErrors));
            Assert.NotNull(ex);
            Assert.NotEqual(0, sbErrors.Length);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseChildAp()
        {
            StringBuilder sbErrors = new StringBuilder();
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

            var result = node.GetPrice(priceStorate, forest, accessList, sbErrors);
            Assert.NotNull(result);
            Assert.Equal(expectedPrice, result);
            Assert.NotEmpty(priceStorate);
            Assert.True(priceStorate.ContainsKey(node.ProductId));
            Assert.Equal(expectedPrice, priceStorate[node.ProductId]);
            Assert.Equal(0, sbErrors.Length);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseChildEp()
        {
            StringBuilder sbErrors = new StringBuilder();
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

            var result = node.GetPrice(priceStorate, forest, accessList, sbErrors);
            Assert.NotNull(result);
            Assert.Equal(expectedPrice, result);
            Assert.NotEmpty(priceStorate);
            Assert.True(priceStorate.ContainsKey(node.ProductId));
            Assert.Equal(expectedPrice, priceStorate[node.ProductId]);
            Assert.Equal(0, sbErrors.Length);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseChildEpInvalidWastage()
        {
            StringBuilder sbErrors = new StringBuilder();
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

            var result = node.GetPrice(priceStorate, forest, accessList, sbErrors);
            Assert.NotNull(result);
            Assert.Equal(expectedPrice, result);
            Assert.NotEmpty(priceStorate);
            Assert.True(priceStorate.ContainsKey(node.ProductId));
            Assert.Equal(expectedPrice, priceStorate[node.ProductId]);
            Assert.Equal(0, sbErrors.Length);

            forest[childNode.ProductId].Wastage = 110;
            sbErrors.Clear();
            result = node.GetPrice(priceStorate, forest, accessList, sbErrors);
            Assert.NotNull(result);
            Assert.Equal(expectedPrice, result);
            Assert.NotEmpty(priceStorate);
            Assert.True(priceStorate.ContainsKey(node.ProductId));
            Assert.Equal(expectedPrice, priceStorate[node.ProductId]);
            Assert.Equal(0, sbErrors.Length);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseChildRecipeNoAccess()
        {
            StringBuilder sbErrors = new StringBuilder();
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

            var result = node.GetPrice(priceStorate, forest, accessList, sbErrors);
            Assert.Null(result);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseChildRecipe()
        {
            StringBuilder sbErrors = new StringBuilder();
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

            var result = node.GetPrice(priceStorate, forest, accessList, sbErrors);
            Assert.NotNull(result);
            Assert.Equal(expectedPrice, result);
            Assert.NotEmpty(priceStorate);
            Assert.True(priceStorate.ContainsKey(node.ProductId));
            Assert.Equal(expectedPrice, priceStorate[node.ProductId]);
            Assert.Equal(0, sbErrors.Length);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseChildRecipeBroken()
        {
            StringBuilder sbErrors = new StringBuilder();
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

            var result = node.GetPrice(priceStorate, forest, accessList, sbErrors);
            Assert.Null(result);
            Assert.Equal(0, sbErrors.Length);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseChildRecipeOption()
        {
            StringBuilder sbErrors = new StringBuilder();
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

            var result = node.GetPrice(priceStorate, forest, accessList, sbErrors);
            Assert.NotNull(result);
            Assert.Equal(expectedPrice, result);
            Assert.NotEmpty(priceStorate);
            Assert.True(priceStorate.ContainsKey(node.ProductId));
            Assert.Equal(expectedPrice, priceStorate[node.ProductId]);
            Assert.Equal(0, sbErrors.Length);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseChildRecipeOptionBroken()
        {
            StringBuilder sbErrors = new StringBuilder();
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

            var result = node.GetPrice(priceStorate, forest, accessList, sbErrors);
            Assert.Null(result);
            Assert.Equal(0, sbErrors.Length);
        }

        [Fact]
        public void PriceRecalcBaseRecipeChoiseOption()
        {
            StringBuilder sbErrors = new StringBuilder();
            decimal expectedPrice = 0m;
            ProductNode node = new ProductNode(1, 1, 1, Fourth.StarChef.Invariables.Constants.ProductType.Dish, 2, Fourth.StarChef.Invariables.Constants.PortionType.AP);
            node.RecipeKind = Fourth.StarChef.Invariables.Constants.RecipeType.Option;
            Dictionary<int, decimal> priceStorate = new Dictionary<int, decimal>();
            Dictionary<int, Product> forest = new Dictionary<int, Product>();
            forest.Add(node.ProductId, new Product() { ProductId = 1, Wastage = 0, ScopeId = 1 });
            HashSet<int> accessList = new HashSet<int>();
            accessList.Add(node.ProductId);
            var result = node.GetPrice(priceStorate, forest, accessList, sbErrors);
            Assert.NotNull(result);
            Assert.Equal(expectedPrice, result);
            Assert.NotEmpty(priceStorate);
            Assert.True(priceStorate.ContainsKey(node.ProductId));
            Assert.Equal(expectedPrice, priceStorate[node.ProductId]);
            Assert.Equal(0, sbErrors.Length);
        }

        [Fact]
        public void PriceRecalcBaseRecipe_RestrictBySupplier()
        {
            StringBuilder sbErrors = new StringBuilder();
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
            
            var result = node.GetPrice(priceStorate, forest, accessList,true, alternates, sbErrors);
            //expected price of node recipe is 2
            Assert.Equal(2,result);
            Assert.Equal(0, sbErrors.Length);
        }

        [Fact]
        public void PriceRecalcBaseRecipe_RestrictBySupplierWithMainIngredient()
        {
            StringBuilder sbErrors = new StringBuilder();
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

            var result = node.GetPrice(priceStorate, forest, accessList, true, alternates, sbErrors);

            //expected price of node recipe is 2
            Assert.Equal(1.25m,result);
            Assert.Equal(0, sbErrors.Length);

            //reset test
            //productid4 is alternate which does not have access -
            sbErrors.Clear();
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

            result = node.GetPrice(priceStorate, forest, accessList, true, alternates, sbErrors);

            //expected price of node recipe is 2
            Assert.Equal(3,result);
            Assert.Equal(0, sbErrors.Length);

            //reset test
            //productid4 is broken
            sbErrors.Clear();
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

            result = node.GetPrice(priceStorate, forest, accessList, true, alternates, sbErrors);
            //expected price not calculated
            Assert.Null(result);
            Assert.Equal(0, sbErrors.Length);

            //reset test
            sbErrors.Clear();
            ingredientB.IsBroken =false;
            accessList.Remove(ingredientB.ProductId);
            result = node.GetPrice(priceStorate, forest, accessList, true, alternates, sbErrors);

            //expected price not calculated
            Assert.Null(result);
            Assert.Equal(0, sbErrors.Length);

            //reset test
            sbErrors.Clear();
            accessList.Add(ingredientB.ProductId);
            //alternates.Add(new IngredientAlternate() { ProductId = 1, AlternateProductId = 2, Ratio = 3});
            result = node.GetPrice(priceStorate, forest, accessList, true, null, sbErrors);

            //expected price not calculated
            Assert.Equal(2, result);
            Assert.Equal(0, sbErrors.Length);

            //reset test
            sbErrors.Clear();
            priceStorate.Remove(node.ProductId);
            priceStorate.Remove(childNode.ProductId);

            //create list with alternates of productid 4 - productid 4 is main ingredient
            alternateOfMainB = new IngredientAlternate() { ProductId = 5, AlternateProductId = 4, AlternateRank = 1, Ratio = 1 };
            alternates.Add(alternateOfMainB);
            //add alternate ProductId = 5 to access list and price
            accessList.Add(alternateOfMainB.ProductId);
            priceStorate.Add(alternateOfMainB.ProductId, 1);
            forest.Add(alternateOfMainB.ProductId, new Product() { ProductId = alternateOfMainB.ProductId, Wastage = 10, ScopeId = 1, Quantity = 1, UnitId = 3, Number = 2 });

            result = node.GetPrice(priceStorate, forest, accessList, true, alternates,sbErrors);

            //expected price of node recipe is 2
            Assert.Equal(2, result);
            Assert.Equal(0, sbErrors.Length);
        }
    }
}
