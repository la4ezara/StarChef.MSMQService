using System.ComponentModel;
using static Fourth.StarChef.Invariables.Constants;

namespace StarChef.Common.Model
{
    /// <summary>
    /// Represent #tmp_product
    /// </summary>
    public class ProductItem
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
    }
}
