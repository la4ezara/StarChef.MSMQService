using System.ComponentModel;
using static Fourth.StarChef.Invariables.Constants;

namespace StarChef.Common.Model
{
    public class Product
    {
        [Description("product_id")]
        public int ProductId { get; set; }
        [Description("number")]
        public decimal Number { get; set; }
        [Description("quantity")]
        public decimal Quantity { get; set; }
        [Description("unit_id")]
        public int UnitId { get; set; }
        [Description("product_type_id")]
        public ProductType ProductTypeId { get; set; }
        [Description("scope_id")]
        public int ScopeId { get; set; }

        [Description("recipe_type_id")]
        public RecipeType? RecipeTypeId { get; set; }
        [Description("wastage")]
        public decimal? Wastage { get; set; }
    }
}
