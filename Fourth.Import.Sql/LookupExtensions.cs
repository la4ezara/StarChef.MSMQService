using System.Collections.Generic;
using Fourth.Import.Exceptions;
using Fourth.Import.Model;

namespace Fourth.Import.Sql
{
    public static class LookupExtensions
    {
        public static string ReplacementFor(this HashSet<LookupValue> lookupValues, MappingColumn column, string importValue,ProcessedImportRow processedImportRow)
        {
            if (string.IsNullOrWhiteSpace(importValue)) 
                return "";
            
            foreach (LookupValue lookupValue in lookupValues)
            {
                if (lookupValue.Lookup == importValue)
                    return lookupValue.ReplacementId.ToString();
            }
            processedImportRow.AddException(new ImportDataException
                                                {
                                                    ExceptionType = ExceptionType.LookupMissing, 
                                                    TemplateColumnName = column.TemplateColumnName,
                                                    IsValid = false,
                                                    TemplateMappingColumn = column.TemplateMappingColumn
                                                });
            return "";
        }
    }
}