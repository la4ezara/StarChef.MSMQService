using System.ComponentModel;

namespace StarChef.Engine.IntegrationTests.Model
{
    public class PriceRecalculationRequest
    {
        [Description("product_id")]
        public int ProductId { get; set; }

        [Description("group_id")]
        public int GroupId { get; set; }

        [Description("unit_id")]
        public int UnitId { get; set; }

        [Description("pset_id")]
        public int PsetId { get; set; }

        [Description("pband_id")]
        public int PbandId { get; set; }

        public override string ToString()
        {
            return ($"ProductId: {ProductId}, GroupId: {GroupId}, Pband: {PbandId}, Pset: {PsetId}, Unit:{UnitId}");
        }
    }
}
