using System.Collections.Generic;
using System.Linq;
using Fourth.Import.Exceptions;
using Fourth.Import.Model;

namespace Fourth.Import.Sql
{
    public static class ExceptionsExtension
    {
  
        public static ProcessedImportRow AddException(this ProcessedImportRow processedImportRow,ImportDataException importDataException)
        {
            if (importDataException == null)
                return null;
            var listExceptions = from ex in processedImportRow.ImportDataExceptions
                                 where ex.TemplateMappingColumn == importDataException.TemplateMappingColumn
                                 select ex;

            if (listExceptions.Count() == 0)
            {
                processedImportRow.ImportDataExceptions.Add(new ImportDataException
                                                                {
                                                                    ExceptionType = importDataException.ExceptionType,
                                                                    TemplateColumnName =
                                                                        importDataException.TemplateColumnName,
                                                                    TemplateMappingColumn =
                                                                        importDataException.TemplateMappingColumn,
                                                                    MessageParam1 = importDataException.MessageParam1,
                                                                    MessageParam2 = importDataException.MessageParam2
                                                                });
            }
            if (importDataException.IsValid == false)
                processedImportRow.IsValid = false;
            return processedImportRow;
        }

    }
}