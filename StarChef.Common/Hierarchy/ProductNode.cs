using StarChef.Common.Model;
using System.Collections.Generic;
using static Fourth.StarChef.Invariables.Constants;
using System.Linq;

namespace StarChef.Common.Hierarchy
{
    public class ProductNode
    {
        private readonly List<ProductNode> _childs;
        public readonly int ProductId;
        public readonly decimal Quantity;
        public readonly int UnitId;
        public readonly decimal Ratio;
        public readonly PortionType? Portion;
        public readonly ProductType NodeType;
        public RecipeType? RecipeKind { get; set; }
        public bool IsChoise { get; set; }
        public bool IsBroken { get; set; }

        public List<ProductNode> Childs => _childs;

        public ProductNode(int productId, int unitId, decimal quantity, ProductType productType) : this(productId, unitId, quantity, productType, 1, PortionType.NotSet)
        {

        }

        public ProductNode(int productId, int unitId, decimal quantity, ProductType productType, decimal ratio, PortionType? portionType)
        {
            ProductId = productId;
            UnitId = unitId;
            Quantity = quantity;
            NodeType = productType;
            Ratio = ratio;
            Portion = portionType;
            _childs = new List<ProductNode>();
        }

        

        public decimal? GetPrice(Dictionary<int, decimal> priceStorage, Dictionary<int, Product> products, HashSet<int> accessList)
        {
            if (!accessList.Contains(ProductId) || IsBroken)
            {
                return null;
            }

            if (priceStorage.ContainsKey(ProductId))
            {
                return priceStorage[ProductId];
            }
            else
            {
                //all ingredients has values 
                //need to calculate prices for recipes

                if (NodeType != ProductType.Ingredient)
                {
                    decimal? total = 0;

                    //skip recipe type option
                    if (RecipeKind.HasValue)
                    {
                        //if we do not have any selection
                        if (RecipeKind == RecipeType.Choice && !Childs.Any(c => c.IsChoise) && Childs.Any())
                        {
                            total = null;
                        }
                        else
                        {
                            for (int i = 0; i < Childs.Count; i++)
                            {
                                var child = Childs[i];
                                decimal price = 0;

                                //need to check if child is ingredient is any of its alternates listed in access list
                                //this case is used when feature Restrict by Supplier access is enable
                                //check access of child product 
                                if (!accessList.Contains(child.ProductId) || child.IsBroken)
                                {
                                    total = null;
                                    break;
                                }

                                if (RecipeKind != RecipeType.Option)
                                {
                                    //for batch && standard recipe or for choise where is selected
                                    var product = products[child.ProductId];
                                    var convertion = product.Quantity * product.Number * child.Ratio;

                                    if (child.NodeType == ProductType.Ingredient)
                                    {
                                        if (RecipeKind != RecipeType.Choice || child.IsChoise)
                                        {
                                            //need to check if child is ingredient is any of its alternates listed in price storage
                                            //this case is used when feature Restrict by Supplier access is enable
                                            var baseIngredientPrice = priceStorage[child.ProductId];
                                            decimal ingredientEpPrice = baseIngredientPrice;
                                            if (child.Portion == PortionType.EP && product.Wastage.HasValue && product.Wastage <= 100 && product.Wastage > 0)
                                            {
                                                ingredientEpPrice = baseIngredientPrice * 100.0m / (100.0m - product.Wastage.Value);
                                            }

                                            var apPrice = ingredientEpPrice * child.Quantity;
                                            price = apPrice / convertion;
                                        }
                                    }
                                    else
                                    {
                                        var recipePrice = child.GetPrice(priceStorage, products, accessList);
                                        if (recipePrice.HasValue)
                                        {
                                            if (RecipeKind != RecipeType.Choice || child.IsChoise)
                                            {
                                                var apPrice = recipePrice.Value * child.Quantity;
                                                price = apPrice / convertion;
                                            }
                                            //add non ingredient price
                                        }
                                        else
                                        {
                                            total = null;
                                            break;
                                        }
                                    }

                                    total += price;
                                }
                                else
                                {
                                    var recipePrice = child.GetPrice(priceStorage, products, accessList);
                                    if (!recipePrice.HasValue)
                                    {
                                        total = null;
                                        break;
                                    }
                                    else
                                    {
                                        price = 0;
                                    }

                                    total += price;
                                }
                            }
                        }
                    }

                    if (total.HasValue)
                    {
                        priceStorage.Add(ProductId, total.Value);
                    }
                    //add final price to storage
                    return total;

                }
                return 0;
            }
        }
    }
}