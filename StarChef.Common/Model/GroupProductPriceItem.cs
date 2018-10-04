using System.ComponentModel;

namespace StarChef.Common.Model
{
    /// <summary>
    /// Represent #tmp_db_product_calc
    /// </summary>
    public class GroupProductPriceItem
    {
        [Description("product_id")]
        public int ProductId { get; set; }
        [Description("group_id")]
        public int GroupId { get; set; }
        [Description("product_price")]
        public decimal Price { get; set; }
    }
}
