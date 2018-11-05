using System.ComponentModel;
using static Fourth.StarChef.Invariables.Constants;

namespace StarChef.Common.Model
{
    public class ProductPart
    {
        [Description("product_id")]
        public int ProductId { get; set; }
        [Description("sub_product_id")]
        public int SubProductId { get; set; }
        [Description("quantity")]
        public decimal? Quantity { get; set; }
        [Description("unit_id")]
        public int? UnitId { get; set; }
        [Description("is_choice")]
        public bool IsChoise { get; set; }
        [Description("portion_type_id")]
        public PortionType PortionTypeId { get; set; }

        [Description("product_type_id")]
        public ProductType ProductTypeId { get; set; }

        [Description("ratio")]
        public decimal Ratio { get; set; }
    }
}
