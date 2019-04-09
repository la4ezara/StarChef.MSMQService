using System;
using System.ComponentModel;

namespace StarChef.Common.Model
{
    public class MsmqLog
    {
        [Description("log_id")]
        public int LogId { get; set; }
        [Description("calc_type")]
        public string CalcType { get; set; }
        [Description("group_id")]
        public int GroupId { get; set; }
        [Description("pset_id")]
        public int PsetId { get; set; }
        [Description("pband_id")]
        public int PbandId { get; set; }
        [Description("unit_id")]
        public int UnitId { get; set; }
        [Description("update_start_time")]
        public DateTime? StartTime { get; set; }
        [Description("update_end_time")]
        public DateTime? EndTime { get; set; }
    }
}