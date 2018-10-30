using StarChef.Common.Model;
using System.Collections.Generic;
using static Fourth.StarChef.Invariables.Constants;
using System.Linq;

namespace StarChef.Common.Hierarchy
{
    public class ProductNode
    {
        public readonly List<ProductNode> Childs;
        public readonly int ProductId;
        public readonly decimal Quantity;
        public readonly int UnitId;
        public readonly decimal Ratio;
        public readonly PortionType? Portion;
        public readonly ProductType NodeType;
        public RecipeType? RecipeKind;
        public bool IsChoise;
        public decimal? EpPrice { get; set; }
        public decimal? ApPrice { get; set; }

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
            Childs = new List<ProductNode>();
        }

        public decimal? GetPrice(Dictionary<int, decimal> priceStorage, Dictionary<int, Product> products, HashSet<int> accessList)
        {
            if (!accessList.Contains(ProductId))
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

                                //check access of child product 
                                if (!accessList.Contains(child.ProductId))
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
                                        //check access of child product 
                                        if (!accessList.Contains(child.ProductId))
                                        {
                                            total = null;
                                            break;
                                        }
                                        if (RecipeKind != RecipeType.Choice || child.IsChoise)
                                        {
                                            var baseIngredientPrice = priceStorage[child.ProductId];

                                            if (child.Portion == PortionType.EP)
                                            {
                                                decimal ingredientEpPrice = baseIngredientPrice;
                                                if (product.Wastage.HasValue && product.Wastage <= 100)
                                                {
                                                    ingredientEpPrice = baseIngredientPrice * 100.0m / (100.0m - product.Wastage.Value);
                                                }

                                                var epPrice = ingredientEpPrice * child.Quantity;
                                                price = epPrice / convertion;
                                            }
                                            else
                                            {
                                                var apPrice = baseIngredientPrice * child.Quantity;
                                                price = apPrice / convertion;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var recipePrice = child.GetPrice(priceStorage, products, accessList);
                                        if (recipePrice.HasValue)
                                        {
                                            if (RecipeKind != RecipeType.Choice || child.IsChoise)
                                            {
                                                price = recipePrice.Value;

                                                var apPrice = price * child.Quantity;
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