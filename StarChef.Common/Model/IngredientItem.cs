using System.ComponentModel;

namespace StarChef.Common.Model
{
    /// <summary>
    /// Represent tmp_ingredient. All required cached data from ingredient table
    /// </summary>
    public class IngredientItem
    {
        [Description("wastage")]
        public decimal Wastage { get; set; }
        [Description("product_id")]
        public int ProductId { get; set; }
        [Description("ingredient_id")]
        public int IngredientId { get; set; }
    }
}
