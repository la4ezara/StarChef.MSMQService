using System.ComponentModel;

namespace StarChef.Common.Model
{
    public class GroupSets
    {
        [Description("group_id")]
        public int GroupId { get; set; }
        [Description("set_id")]
        public int SetId { get; set; }
        [Description("set_entity_id")]
        public int SetEntityId { get; set; }
        [Description("group_set_link_id")]
        public int GroupSetLinkId { get; set; }
        [Description("rank")]
        public int Rank { get; set; }
        [Description("pband_id")]
        public int? PbandId { get; set; }
    }
}
