using System.ComponentModel;

namespace StarChef.Common.Model
{
    public class IngredientAlternate
    {
        [Description("product_id")]
        public int ProductId { get; set; }
        [Description("alternate_product_id")]
        public int AlternateProductId { get; set; }
        [Description("alternate_rank")]
        public int AlternateRank { get; set; }
        
    }
}
