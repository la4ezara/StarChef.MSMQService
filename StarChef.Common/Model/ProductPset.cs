using System.ComponentModel;

namespace StarChef.Common.Model
{
    public class ProductPset
    {
        [Description("product_id")]
        public int ProductId { get; set; }
        [Description("set_id")]
        public int SetId { get; set; }
    }
}
