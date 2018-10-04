using System.ComponentModel;
using static Fourth.StarChef.Invariables.Constants;

namespace StarChef.Common.Model
{
    /// <summary>
    /// Represent #tmp_dish. All required cached data from dish table
    /// </summary>
    public class DishItem
    {
        [Description("recipe_type_id")]
        public RecipeType RecipeTypeId { get; set; }
        [Description("product_id")]
        public int ProductId { get; set; }
    }
}
