using StarChef.Common.Model;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static Fourth.StarChef.Invariables.Constants;

namespace StarChef.Common.Hierarchy
{
    public class ProductForest
    {
        private readonly List<Product> _products;
        private readonly Dictionary<int, Product> _products_dict;
        private readonly List<ProductPart> _parts;
        public readonly Dictionary<int, Product> PrivateProducts;

        private readonly Dictionary<int, ProductNode> _forest;

        public Dictionary<int, ProductNode> Forest => _forest;

        public ProductForest(List<Product> products, List<ProductPart> parts)
        {
            _products = products;
            _products_dict = products.ToDictionary(key => key.ProductId, value => value);
            PrivateProducts = products.Where(p => p.ScopeId > 1).ToDictionary(key => key.ProductId, value => value);
            _parts = parts;
            _forest = new Dictionary<int, ProductNode>();
        }

        public void BuildForest()
        {
            _forest.Clear();
            AddNonBlankRecipes();
            //add recipes which are empty
            AddBlankRecipes();
        }

        private void AddBlankRecipes()
        {
            foreach (Product p in _products.Where(p => p.ProductTypeId == ProductType.Dish))
            {
                var product = _products_dict[p.ProductId];
                ProductNode node = new ProductNode(product.ProductId, product.UnitId, product.Quantity, product.ProductTypeId);
                if (product.ProductTypeId == ProductType.Dish)
                {
                    node.RecipeKind = product.RecipeTypeId;
                }
                if (!_forest.ContainsKey(node.ProductId))
                {
                    _forest.Add(node.ProductId, node);
                }
            }
        }

        private void AddNonBlankRecipes()
        {
            Dictionary<int, List<ProductPart>> product_parts_dict = _parts.GroupBy(g => g.ProductId).ToDictionary(key => key.Key, value => value.ToList());

            foreach (int productId in product_parts_dict.Keys)
            {
                var childs = product_parts_dict[productId];
                var product = _products_dict[productId];
                ProductNode node = new ProductNode(productId, product.UnitId, product.Quantity, product.ProductTypeId);

                if (product.ProductTypeId == ProductType.Dish)
                {
                    node.RecipeKind = product.RecipeTypeId;
                }

                if (!_forest.ContainsKey(productId))
                {
                    AddRecipeChild(node, product_parts_dict, childs);
                    _forest.Add(node.ProductId, node);
                }
            }
        }

        private void AddRecipeChild(ProductNode parent, Dictionary<int, List<ProductPart>> allParts, List<ProductPart> childParts)
        {
            foreach (var part in childParts)
            {
                ProductNode node = new ProductNode(part.SubProductId, part.UnitId.HasValue  ? part.UnitId.Value: 0, part.Quantity.HasValue ? part.Quantity.Value : 0, part.ProductTypeId, part.Ratio, part.PortionTypeId);
                if (!part.Quantity.HasValue || !part.UnitId.HasValue) {
                    node.IsBroken = true;
                }

                if (part.ProductTypeId == ProductType.Dish)
                {
                    node.RecipeKind = _products_dict[part.SubProductId].RecipeTypeId;
                }

                node.IsChoise = part.IsChoise;

                parent.Childs.Add(node);

                if (part.ProductTypeId != ProductType.Ingredient)
                {
                    if (_forest.ContainsKey(node.ProductId))
                    {
                        var existingItemStructure = _forest[node.ProductId];
                        //add existings 
                        node.Childs.AddRange(existingItemStructure.Childs);
                    }
                    else
                    {
                        List<ProductPart> childs = new List<ProductPart>();
                        if (allParts.ContainsKey(node.ProductId))
                        {
                            childs = allParts[node.ProductId];
                        }

                        var product = _products_dict[node.ProductId];
                        ProductNode fNode = new ProductNode(product.ProductId, product.UnitId, product.Quantity, product.ProductTypeId);
                        //build non existing chain
                        AddRecipeChild(fNode, allParts, childs);
                        node.Childs.AddRange(fNode.Childs);
                        //build child structure
                        _forest.Add(fNode.ProductId, node);
                    }
                }
            }
        }

        /// <summary>
        /// Calcualte all prices for set of groups
        /// </summary>
        /// <param name="groupPrices"></param>
        /// <returns></returns>
        public Dictionary<int, Dictionary<int, decimal>> CalculatePrice(List<GroupProducts> groupPrices)
        {
            Dictionary<int, Dictionary<int, decimal>> allPrices = new Dictionary<int, Dictionary<int, decimal>>();
            ConcurrentDictionary<int, decimal> privatePrices = new ConcurrentDictionary<int, decimal>();
            var groupedGroupPrices = groupPrices.GroupBy(g => g.GroupId).OrderByDescending(g => g.Key).ToList();
            //possible to execure in parallel
            for (int i = 0; i < groupedGroupPrices.Count; i++)
            {

                var groups = groupedGroupPrices[i].ToList();
                var groupCalculatedPrices = CalculatePrice(groups, privatePrices);
                if (groupCalculatedPrices.Any())
                {
                    allPrices.Add(groupedGroupPrices[i].Key, groupCalculatedPrices);
                }
            }
            allPrices.Add(0, privatePrices.ToDictionary(k => k.Key, v => v.Value));
            
            return allPrices;
        }

        /// <summary>
        /// Calculate single GroupPrices
        /// </summary>
        /// <param name="groups"></param>
        /// <param name="privatePrices"></param>
        /// <returns></returns>
        public Dictionary<int, decimal> CalculatePrice(List<GroupProducts> groups, ConcurrentDictionary<int, decimal> privatePrices)
        {
            HashSet<int> accessList = new HashSet<int>(groups.Select(p => p.ProductId).Distinct());

            //get already existing prices
            Dictionary<int, decimal> groupCalculatedPrices = groups.Where(p => p.Price.HasValue && p.Price >= 0).ToDictionary(key => key.ProductId, value => value.Price.Value);

            var keys = _forest.Keys.ToList();

            for (var y = 0; y < keys.Count; y++)
            {
                _forest[keys[y]].GetPrice(groupCalculatedPrices, _products_dict, accessList);
            }

            ///calculate items for producrs which are in results but they are private
            ///remove them from list and add them to private list
            foreach (int privateProductId in PrivateProducts.Keys)
            {
                if (groupCalculatedPrices.ContainsKey(privateProductId))
                {
                    if (!privatePrices.ContainsKey(privateProductId))
                    {
                        privatePrices.TryAdd(privateProductId, groupCalculatedPrices[privateProductId]);
                    }

                    groupCalculatedPrices.Remove(privateProductId);
                }
            }
            return groupCalculatedPrices;
        }

        public void ReAssignForest(Dictionary<int, ProductNode> newNodes)
        {
            _forest.Clear();
            foreach (var node in newNodes)
            {
                _forest.Add(node.Key, node.Value);
            }
        }

        public Dictionary<int, ProductNode> GetAffectedCuts(int productId) {
            Dictionary<int, ProductNode> cuts = new Dictionary<int, ProductNode>();
            foreach (var node in _forest.Values)
            {
                GetAffectedCuts(productId, node, cuts);
            }

            return cuts;
        }

        private bool GetAffectedCuts(int productId, ProductNode node, Dictionary<int, ProductNode> cuts)
        {
            if (node.ProductId == productId)
            {
                if (!cuts.ContainsKey(productId))
                {
                    cuts.Add(productId, node);
                }
                return true;
            }
            else
            {
                if (node.Childs.Any()) {
                    foreach (var child in node.Childs) {
                        var result = GetAffectedCuts(productId, child, cuts);
                        if (result) {

                            if (!cuts.ContainsKey(node.ProductId))
                            {
                                cuts.Add(node.ProductId, node);
                            }
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
