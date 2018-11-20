using System.ComponentModel;

namespace StarChef.Common.Model
{
    /// <summary>
    /// db_product_calc data. Also used #group_products
    /// </summary>
    public class ProductGroupPrice
    {
        [Description("product_id")]
        public int ProductId { get; set; }
        [Description("group_id")]
        public int? GroupId { get; set; }
        [Description("price")]
        public decimal? Price { get; set; }
    }
}
