using System;
using System.Collections.Generic;
using System.Data;
using Fourth.Import.Exceptions;
using Fourth.Import.Model;

namespace Fourth.Import.Sql
{
    public class GeneratorDelegates
    {
        public GeneratorDelegates()
        {
            InjectSql = new List<Func<MappingTable, DataRow, string>>();
        }
        public Func<string> AuxillaryStartSql { get; set; }
        public Func<string> AuxillaryEndSql { get; set; }
        public IList<Func<MappingTable, DataRow, ImportDataException>> TableValidations { get; set; }
        public IList<Func<MappingTable,DataRow,string>> InjectSql { get; set; }
        public Func<MappingColumn, ProcessedImportRow, string> ReplacementForTag { get; set; }
        public Func<MappingColumn, ProcessedImportRow, string> GetSuppQtyUnit { get; set; }
        public Func<MappingColumn, ProcessedImportRow, string> CheckPConversionUnit { get; set; }
    }
}