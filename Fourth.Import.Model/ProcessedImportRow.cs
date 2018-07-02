using System.Collections.Generic;
using System.Data;
using Fourth.Import.Exceptions;

namespace Fourth.Import.Model
{
    public class ProcessedImportRow
    {
        public int RowIndex { get; set; }
        public DataRow Row { get; set; }
        public string IngredientName { get; set; }
        public string IngredientId { get; set; }
        public string DistributorCode { get; set; }
        public string SqlQuery { get; set; }
        public bool IsValid { get; set; }
        public IList<ImportDataException> ImportDataExceptions { get; set; }
    }
}