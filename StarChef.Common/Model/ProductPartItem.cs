using System.ComponentModel;
using static Fourth.StarChef.Invariables.Constants;

namespace StarChef.Common.Model
{
    /// <summary>
    /// Represent #tmp_product_part
    /// </summary>
    public class ProductPartItem
    {
        [Description("product_part_id")]
        public int ProductPartId { get; set; }
        [Description("product_id")]
        public int ProductId { get; set; }
        [Description("sub_product_id")]
        public int SubProductId { get; set; }
        [Description("quantity")]
        public float Quantity { get; set; }
        [Description("unit_id")]
        public int UnitId { get; set; }
        [Description("is_choise")]
        public bool IsChoise { get; set; }
        [Description("portion_type_id")]
        public PortionType PortionTypeId { get; set; }
    }
}
