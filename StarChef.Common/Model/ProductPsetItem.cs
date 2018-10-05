using System.ComponentModel;

namespace StarChef.Common.Model
{
    public class ProductPsetItem
    {
        [Description("product_id")]
        public int ProductId { get; set; }
        [Description("pset_id")]
        public int PsetId { get; set; }
        [Description("group_id")]
        public int GroupId { get; set; }
        [Description("is_choice")]
        public bool IsChoise { get; set; }
    }
}
