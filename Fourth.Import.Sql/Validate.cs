using System;
using System.Configuration;
using Fourth.Import.Exceptions;
using Fourth.Import.Model;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Data;
using Fourth.Import.Data;

namespace Fourth.Import.Sql
{
    public static class Validate
    {
        public static string ValidateAgainstDbSetting(this string importValue, MappingColumn column, ProcessedImportRow processedImportRow)
        {
            if(column.ValidationRules!=null)
            {
                foreach (var rule in column.ValidationRules)
                {
                    if(string.IsNullOrEmpty(importValue) && (rule.Mandatory || column.Mandatory))
                    {
                        processedImportRow.AddException
                            (new ImportDataException
                                 {
                                     ExceptionType = column.StarchefColumnName == "tag_id" ? ExceptionType.CategoryRequired : ExceptionType.DbSettingIsMandatory,
                                     TemplateColumnName = column.TemplateColumnName,
                                     IsValid = false,
                                     TemplateMappingColumn = column.TemplateMappingColumn
                                 });
                    }

                    if(rule.StringLength < int.MaxValue && rule.StringLength > 0 && importValue.Length > rule.StringLength)
                    {
                        processedImportRow.AddException
                            (new ImportDataException
                            {
                                ExceptionType = ExceptionType.DbSettingMaxLength,
                                TemplateColumnName = column.TemplateColumnName,
                                IsValid = false,
                                TemplateMappingColumn = column.TemplateMappingColumn,
                                MessageParam1 = rule.StringLength.ToString()
                            });
                    }

                    if (rule.MinimumValue!=rule.MaximumValue && !string.IsNullOrEmpty(importValue))
                    {
                        //TODO:candidate for refactoring, perhaps the whole class
                        decimal value;
                        if (decimal.TryParse(importValue, out value))
                        {

                            if (value < rule.MinimumValue)
                            {
                                processedImportRow.AddException
                                    (new ImportDataException
                                         {
                                             ExceptionType = ExceptionType.DbSettingMinimumValue,
                                             TemplateColumnName = column.TemplateColumnName,
                                             IsValid = false,
                                             TemplateMappingColumn = column.TemplateMappingColumn,
                                             MessageParam1 = rule.MinimumValue.ToString(),
                                             MessageParam2 = rule.MaximumValue.ToString()
                                         });
                            }
                            if (value > rule.MaximumValue)
                            {
                                processedImportRow.AddException(new ImportDataException
                                                                    {
                                                                        ExceptionType = ExceptionType.DbSettingMaximumValue,
                                                                        TemplateColumnName = column.TemplateColumnName,
                                                                        IsValid = false,
                                                                        TemplateMappingColumn = column.TemplateMappingColumn,
                                                                        MessageParam1 = rule.MinimumValue.ToString(),
                                                                        MessageParam2 = rule.MaximumValue.ToString()
                                                                    });
                            }
                        }
                        else
                        {
                            processedImportRow.AddException
                                    (new ImportDataException
                                    {
                                        ExceptionType = ExceptionType.DbSettingMinimumValue,
                                        TemplateColumnName = column.TemplateColumnName,
                                        IsValid = false,
                                        TemplateMappingColumn = column.TemplateMappingColumn,
                                        MessageParam1 = rule.MinimumValue.ToString(),
                                        MessageParam2 = rule.MaximumValue.ToString()
                                    });
                        }

                    }

                    if(!string.IsNullOrEmpty(rule.RegEx))
                    {
                        Regex regex = new Regex(rule.RegEx);

                        if (!regex.Match(importValue).Success)
                        {
                            processedImportRow.AddException
                                (new ImportDataException
                                     {
                                         ExceptionType = ExceptionType.DbSettingRegEx,
                                         TemplateColumnName = column.TemplateColumnName,
                                         IsValid = false,
                                         TemplateMappingColumn = column.TemplateMappingColumn,
                                         MessageParam1 = rule.RegEx,
                                     });
                        }
                    }
                }
            }
            

            return importValue;
        }
    }
}