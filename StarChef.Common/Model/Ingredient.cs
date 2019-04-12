using System.ComponentModel;

namespace StarChef.Common.Model
{
    public class Ingredient
    {
        [Description("product_id")]
        public int ProductId { get; set; }
        [Description("master_rank_order")]
        public int RankOrder { get; set; }
        [Description("master_ref_no")]
        public int RefNumber { get; set; }
        [Description("master_ref_rev")]
        public int RefRevision { get; set; }
    }
}
