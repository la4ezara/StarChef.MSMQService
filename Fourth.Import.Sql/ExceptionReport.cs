using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Fourth.Import.Exceptions;
using Fourth.Import.Model;

namespace Fourth.Import.Sql
{
    public class ExceptionReport
    {
        private const string TableName = "ExceptionReport";
        private const string Column1 = "Row Number";
        private const string Column2 = "Ingredient Name (In Exception)";
        private const string Column3 = "Error Field";
        private const string Column4 = "Error Description";

        public DataTable Generate(List<MappingColumn> mappingColumns, string displayFileName, List<ProcessedImportRow> exceptionsRows, List<ExceptionMessage> exceptionMessages, string importType, out IList<int> dataRows)
        {
            if (exceptionsRows.Count > 0)
            {
                DataTable exceptionReport = GenerateExceptionTable(mappingColumns, displayFileName, importType);
                IList<int> tempDataRows = new List<int>();
                foreach (var processedImportRow in exceptionsRows)
                {
                    bool isHeaderRowAdded = false;
                    foreach (var importDataException in processedImportRow.ImportDataExceptions)
                    {
                        string exceptionMessage = GetExceptionMessage(importDataException, exceptionMessages);
                        DataRow dr = exceptionReport.NewRow();
                        dr[Column1] = processedImportRow.RowIndex + 1;
                        dr[Column2] = processedImportRow.IngredientName;
                        dr[Column3] = importDataException.TemplateColumnName;
                        dr[Column4] = exceptionMessage;
                        if (!isHeaderRowAdded)
                        {
                            tempDataRows.Add(exceptionReport.Rows.Count);
                            foreach (var mappingColumn in mappingColumns)
                            {
                                dr[mappingColumn.TemplateColumnName] =
                                    processedImportRow.Row[mappingColumn.TemplateColumnOrdinal];
                            }
                            isHeaderRowAdded = true;
                        }
                        exceptionReport.Rows.Add(dr);
                    }
                }
                dataRows = tempDataRows;
                return exceptionReport;
            }
            dataRows = null;
            return null;
        }


        public DataTable Generate(DataTable exceptionReport, string displayFileName, string importType)
        {
            DataTable result = GenerateExceptionTable(displayFileName, importType);
            
            foreach (DataRow row in exceptionReport.Rows)
            {
                DataRow dr = result.NewRow();
                dr[Column1] = row[0];
                dr[Column2] = row[4];
                dr[Column3] = row[3];
                dr[Column4] = row[2];

                result.Rows.Add(dr);
            }

            return result;
        }

        private static string GetExceptionMessage(ImportDataException importDataException, IEnumerable<ExceptionMessage> exceptionMessages)
        {
            string message = string.Empty;
            List<string> list = (exceptionMessages.Where(ex => ex.MappingName == importDataException.TemplateMappingColumn).Select(ex => ex.Message)).ToList();

            switch (importDataException.ExceptionType)
            {
                case ExceptionType.DbSettingIsMandatory:
                case ExceptionType.MandatoryInTable:
                    list = (exceptionMessages.Where(ex => ex.MappingName.ToUpperInvariant() == "DBSETTING" &&
                                        ex.ValidationName.ToUpperInvariant() == "ISMANDATORY").Select(ex => ex.Message)).ToList();
                    break;
                case ExceptionType.DbSettingMaxLength:
                    list = (exceptionMessages.Where(ex => ex.MappingName.ToUpperInvariant() == "DBSETTING" &&
                                        ex.ValidationName.ToUpperInvariant() == "LENGTH").Select(ex => ex.Message)).ToList();
                    break;
                case ExceptionType.DbSettingRegEx:
                    list = (exceptionMessages.Where(ex => ex.MappingName == importDataException.TemplateMappingColumn &&
                                        ex.ValidationName.ToUpperInvariant() == "REGEX").Select(ex => ex.Message)).ToList();
                    break;
                case ExceptionType.DbSettingMaximumValue:
                case ExceptionType.DbSettingMinimumValue:
                    list = (exceptionMessages.Where(ex => ex.MappingName.ToUpperInvariant() == "DBSETTING" &&
                                        ex.ValidationName.ToUpperInvariant() == "RANGE").Select(ex => ex.Message)).ToList();
                    break;
                case ExceptionType.DuplicateDistributorCode:
                case ExceptionType.DuplicateSupplierCode:
                    list = (exceptionMessages.Where(ex => ex.MappingName == importDataException.TemplateMappingColumn).Select(ex => ex.Message)).ToList();
                    break;
                case ExceptionType.LookupMissing:
                    list = (exceptionMessages.Where(ex => ex.MappingName == importDataException.TemplateMappingColumn &&
                                        ex.ValidationName.ToUpperInvariant() == "LOOKUP").Select(ex => ex.Message)).ToList();
                    break;
                case ExceptionType.CategoryIsNotValid:
                    list = (exceptionMessages.Where(ex => ex.MappingName == importDataException.TemplateMappingColumn &&
                                        ex.ValidationName.ToUpperInvariant() == "DATABASE").Select(ex => ex.Message)).ToList();
                    break;
                case ExceptionType.InvalidFutureDate:
                    list = (exceptionMessages.Where(ex => ex.MappingName == importDataException.TemplateMappingColumn &&
                                        ex.ValidationName.ToUpperInvariant() == "DATABASE").Select(ex => ex.Message)).ToList();
                    break;
                case ExceptionType.InvalidDateFormat:
                case ExceptionType.CategoryRequired:
                    list = (exceptionMessages.Where(ex => ex.MappingName == importDataException.TemplateMappingColumn &&
                                        ex.ValidationName.ToUpperInvariant() == "ISMANDATORY").Select(ex => ex.Message)).ToList();
                    break;
            }

            if (list.Count > 0)
            {
                message = list[0];

                message = string.Format(message, importDataException.TemplateColumnName,
                                    importDataException.MessageParam1, importDataException.MessageParam2);
            }
            return message;
        }

        private static DataTable GenerateExceptionTable(List<MappingColumn> mappingColumns, string displayFileName,string importType)
        {
            DataTable exceptionReport = new DataTable(TableName);
            exceptionReport.Columns.Add(Column1);
            exceptionReport.Columns.Add(Column2);
            exceptionReport.Columns.Add(Column3);
            exceptionReport.Columns.Add(Column4);

            mappingColumns = mappingColumns.OrderBy(col => col.TemplateColumnOrdinal).ToList();

            foreach (var mappingColumn in mappingColumns)
            {
                if (!exceptionReport.Columns.Contains(mappingColumn.TemplateColumnName)) //This is required for internal > pending price update.As there are two fields affected by pending price effective date
                    exceptionReport.Columns.Add(mappingColumn.TemplateColumnName);
            }


            DataRow dr = exceptionReport.NewRow();
            dr[Column1] = "StarChef.NET Exception Report";
            exceptionReport.Rows.Add(dr);

            dr = exceptionReport.NewRow();
            dr[Column1] = "Version:";
            dr[Column2] = importType;
            exceptionReport.Rows.Add(dr);

            dr = exceptionReport.NewRow();
            dr[Column1] = "Original File:";
            dr[Column2] = displayFileName;
            exceptionReport.Rows.Add(dr);

            dr = exceptionReport.NewRow();
            DataRow headerRow = exceptionReport.NewRow();
            dr[Column1] = Column1;
            dr[Column2] = Column2;
            dr[Column3] = Column3;
            dr[Column4] = Column4;

            foreach (var mappingColumn in mappingColumns)
            {
                dr[mappingColumn.TemplateColumnName] = mappingColumn.TemplateColumnName;
                headerRow[mappingColumn.TemplateColumnName] = mappingColumn.TemplateMappingColumn;
            }
            exceptionReport.Rows.Add(headerRow);
            exceptionReport.Rows.Add(dr);

            dr = exceptionReport.NewRow();
            dr[Column1] = "To correct failed items, amend data in the rows highlighted in blue, save this spreadsheet to your PC and then upload it to the Ingredient Import/Price Update tool";
            exceptionReport.Rows.Add(dr);

            return exceptionReport;
        }
    
        private static DataTable GenerateExceptionTable(string displayFileName,string importType)
        {
            DataTable exceptionReport = new DataTable(TableName);
            exceptionReport.Columns.Add(Column1);
            exceptionReport.Columns.Add(Column2);
            exceptionReport.Columns.Add(Column3);
            exceptionReport.Columns.Add(Column4);

            


            DataRow dr = exceptionReport.NewRow();
            dr[Column1] = "Intolerance File Exception Report";
            exceptionReport.Rows.Add(dr);

            dr = exceptionReport.NewRow();
            dr[Column1] = "Ingredients in StarChef.NET which were not in the supplier's file";
            exceptionReport.Rows.Add(dr);

            dr = exceptionReport.NewRow();
            exceptionReport.Rows.Add(dr);

            dr = exceptionReport.NewRow();
            dr[Column1] = "Supplier:";
            dr[Column2] = "3663";
            exceptionReport.Rows.Add(dr);

            dr = exceptionReport.NewRow();
            dr[Column1] = "File:";
            dr[Column2] = displayFileName;
            exceptionReport.Rows.Add(dr);

            dr = exceptionReport.NewRow();
            dr[Column1] = "Date Time";
            dr[Column2] = DateTime.Now.ToString("g");
            exceptionReport.Rows.Add(dr);

            dr = exceptionReport.NewRow();
            exceptionReport.Rows.Add(dr);

            dr = exceptionReport.NewRow();
            
            dr[Column1] = "StarChef Key";
            dr[Column2] = "Ingredient Name";
            dr[Column3] = "Supplier Name";
            dr[Column4] = "Supplier Code";

           
            

            exceptionReport.Rows.Add(dr);
            return exceptionReport;
        }
    }
}
