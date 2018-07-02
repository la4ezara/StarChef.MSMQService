using System.Collections.Generic;

namespace Fourth.Import.Model
{
    public class MappingColumn
    {
        public string TemplateColumnName { get; set; }
        public string TemplateMappingColumn { get; set; }
        public int TemplateColumnOrdinal { get; set; }
        public string StarchefColumnName { get; set; }
        public Datatype Datatype { get; set; }
        public string DefaultValue { get; set; }
        public bool Mandatory { get; set; }
        public bool IgnoreForImport { get; set; }
        public bool IgnoreThisTable { get; set; }
        public LookupType LookupType { get; set; }
        public byte? LookupTableId { get; set; }
        public HashSet<LookupValue> LookupValues { get; set; }
        public int? ValidationSettingId { get; set; }
        public IList<ValidationRules> ValidationRules { get; set; }
    }
}