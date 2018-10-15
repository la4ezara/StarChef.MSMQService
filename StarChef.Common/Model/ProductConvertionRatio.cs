using System.Collections.Generic;
using System.ComponentModel;

namespace StarChef.Common.Model
{
    public class ProductConvertionRatio : IEqualityComparer<ProductConvertionRatio>
    {
        [Description("Product_id")]
        public int ProductId { get; set; }
        [Description("Source_unit_id")]
        public short SourceUnitId { get; set; }
        [Description("Target_unit_id")]
        public short TargetUnitId { get; set; }
        [Description("Ratio")]
        public decimal Ratio { get; set; }

        public bool Equals(ProductConvertionRatio x, ProductConvertionRatio y)
        {
            if (x.ProductId.Equals(y.ProductId) && x.SourceUnitId.Equals(y.SourceUnitId)
                && x.TargetUnitId.Equals(y.TargetUnitId)
                && x.Ratio.Equals(y.Ratio))
            {
                return true;
            }

            return false;
        }

        public int GetHashCode(ProductConvertionRatio obj)
        {
            return obj.ProductId.GetHashCode() ^ obj.SourceUnitId.GetHashCode()
                ^ obj.TargetUnitId.GetHashCode() ^ obj.Ratio.GetHashCode();
        }
    }
}
