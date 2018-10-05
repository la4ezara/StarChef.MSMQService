using System.ComponentModel;

namespace StarChef.Common.Model
{
    public class ProductConvertionRatio
    {
        [Description("Product_id")]
        public int ProductId { get; set; }
        [Description("Source_unit_id")]
        public short SourceUnitId { get; set; }
        [Description("Target_unit_id")]
        public short TargetUnitId { get; set; }
        [Description("Ratio")]
        public decimal Ratio { get; set; }

    }
}
