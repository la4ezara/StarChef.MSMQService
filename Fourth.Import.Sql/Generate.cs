using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Fourth.Import.Exceptions;
using Fourth.Import.Model;

namespace Fourth.Import.Sql
{
    public class Generate
    {
        public bool Query(IList<MappingTable> mappingTables, ProcessedImportRow processedImportRow, GeneratorDelegates generatorDelegates)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("SET XACT_ABORT ON");
            sb.AppendLine("BEGIN TRAN");
            sb.AppendLine(generatorDelegates.AuxillaryStartSql());

            foreach (MappingTable mappingTable in mappingTables.OrderBy(t => t.ProcessingOrder))
            {   
                
                sb.AppendLine(InsertStringFor(mappingTable, processedImportRow, generatorDelegates));
                if (!string.IsNullOrWhiteSpace(mappingTable.EndQueryWith))
                    sb.AppendLine(mappingTable.EndQueryWith);
            }
            sb.AppendLine(generatorDelegates.AuxillaryEndSql());
            sb.AppendLine("COMMIT");
            processedImportRow.SqlQuery = sb.ToString();
            return true;
        }

        private static string InsertStringFor(MappingTable mappingTable, ProcessedImportRow processedImportRow,GeneratorDelegates generatorDelegates)
        {
            StringBuilder sb = new StringBuilder();
            string columnNames;
            string columnValues;
            Dictionary<string, string> updateColumns;

            foreach (Func<MappingTable, DataRow, ImportDataException> tableValidation in generatorDelegates.TableValidations)
            {
                processedImportRow.AddException(tableValidation(mappingTable, processedImportRow.Row));
            }

            foreach (Func<MappingTable, DataRow, string> injectSql in generatorDelegates.InjectSql)
            {
                sb.AppendLine(injectSql(mappingTable, processedImportRow.Row));
            }

            if (!string.IsNullOrWhiteSpace(mappingTable.StartQueryWith))
                sb.AppendLine(mappingTable.StartQueryWith);

            bool ignoreThisTable = GetTableAttributes(mappingTable, processedImportRow, generatorDelegates, out columnNames, out columnValues, out updateColumns);
            if (!ignoreThisTable && mappingTable.GenerateQuery)
            {
                if (mappingTable.DmlType == DmlType.Insert)
                {
                    sb.AppendFormat("Insert into {0} ({1}) values ({2})", mappingTable.TableName, columnNames,
                                    columnValues);
                }
                else if(mappingTable.DmlType==DmlType.Update)
                {
                    sb.AppendFormat("Update {0} Set ", mappingTable.TableName);

                    foreach (KeyValuePair<string, string> column in updateColumns)
                    {
                        sb.AppendFormat("{0} = {1}", column.Key, column.Value);
                    }
                }
            }
            return sb.ToString();
        }


        private static bool GetTableAttributes(MappingTable mappingTable, ProcessedImportRow processedImportRow,GeneratorDelegates generatorDelegates,out string columnNames, out string columnValues,out Dictionary<string,string> columnNamesValues )
        {
            StringBuilder columnNamesSb = new StringBuilder();
            StringBuilder columnValuesSb = new StringBuilder();
            bool ignoreThisTable=false;
            Dictionary<string,string> updateCollection = new Dictionary<string, string>();


            foreach (MappingColumn column in mappingTable.MappingColumns.Where(i=>!i.IgnoreForImport))
            {
                if (column.TemplateColumnOrdinal == -1 && !string.IsNullOrWhiteSpace(column.TemplateColumnName)) // when a column is missing in Excel
                {
                    if (column.Mandatory)
                        processedImportRow.AddException(new ImportDataException
                                                            {
                                                                ExceptionType = ExceptionType.MandatoryInTable, 
                                                                TemplateColumnName = column.TemplateColumnName,
                                                                IsValid = false,
                                                                TemplateMappingColumn = column.TemplateMappingColumn
                                                            });
                    continue;
                }

                string columnValue = column.TemplateColumnOrdinal == -1 ? column.DefaultValue.WithSqlSyntax(column.Datatype) : GetImportValueOf(column, processedImportRow, generatorDelegates);
                
                if (column.Mandatory && columnValue == "NULL")
                {
                    processedImportRow.AddException(new ImportDataException
                    {
                        ExceptionType = ExceptionType.MandatoryInTable,
                        TemplateColumnName = column.TemplateColumnName,
                        IsValid = false,
                        TemplateMappingColumn = column.TemplateMappingColumn
                    });
                    continue;
                }

                if (columnValue == "INVALID")
                {
                    processedImportRow.AddException(new ImportDataException
                    {
                        ExceptionType = ExceptionType.LookupMissing,
                        TemplateColumnName = column.TemplateColumnName,
                        IsValid = false,
                        TemplateMappingColumn = column.TemplateMappingColumn
                    });
                    continue;
                }

                columnValuesSb.Append(columnValue + ",");
                                     
                if (column.IgnoreThisTable)
                {
                    //Note : not neat, can't think of any eligent solution now
                    ignoreThisTable = column.IgnoreThisTable;
                    column.IgnoreThisTable = false;
                }

                columnNamesSb.Append(column.StarchefColumnName + ",");

                updateCollection.Add(column.StarchefColumnName,columnValue);

            }
            columnNames = columnNamesSb.ToString().TrimEnd(',');
            columnValues = columnValuesSb.ToString().TrimEnd(',');
            columnNamesValues = updateCollection;
            return ignoreThisTable;
        }

        private static string GetImportValueOf(MappingColumn column, ProcessedImportRow processedImportRow,GeneratorDelegates generatorDelegates)
        {
            string value = processedImportRow.Row[column.TemplateColumnOrdinal] == DBNull.Value ? "" : processedImportRow.Row[column.TemplateColumnOrdinal].ToString().Trim();
            
            switch (column.LookupType)
            {
                case LookupType.None:
                    return value
                        .ValidateAgainstDbSetting(column, processedImportRow)
                        .WithSqlSyntax(column.Datatype);
                    
                case LookupType.Table:
                case LookupType.Unit:
                    return column.LookupValues
                            .ReplacementFor(column, value.ToUpperInvariant(), processedImportRow)
                            .ValidateAgainstDbSetting(column, processedImportRow)
                            .WithSqlSyntax(column.Datatype);

                case LookupType.Tag:
                    return generatorDelegates.ReplacementForTag(column, processedImportRow)
                        .ValidateAgainstDbSetting(column, processedImportRow)
                        .WithSqlSyntax(column.Datatype);
             
                default:
                    throw new ArgumentException("Unknown lookup type");
             }
        }
    }
}

