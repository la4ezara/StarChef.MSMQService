using System.Collections.Generic;

namespace Fourth.Import.Model
{
    public class MappingTable
    {
        public string TableName { get; set; }
        public int ProcessingOrder { get; set; }
        public string StartQueryWith { get; set; }
        public string EndQueryWith { get; set; }
        public bool GenerateQuery { get; set; }
        public DmlType DmlType { get; set; }
        public IList<MappingColumn> MappingColumns { get; set; }
    }
}