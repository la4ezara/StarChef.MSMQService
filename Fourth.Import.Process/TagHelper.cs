using System;
using System.Linq;
using Fourth.Import.Exceptions;
using Fourth.Import.Model;
using Fourth.Import.Sql;
using Fourth.Import.DataService;

namespace Fourth.Import.Process
{
    public static class TagHelper
    {
        public static string TagDelegate(MappingColumn column, ProcessedImportRow processedImportRow)
        {
            string mainCategory = processedImportRow.Row[column.TemplateColumnOrdinal - 3] == DBNull.Value ? "" : processedImportRow.Row[column.TemplateColumnOrdinal - 3].ToString().Trim();
            string subCategory = processedImportRow.Row[column.TemplateColumnOrdinal - 2] == DBNull.Value ? "" : processedImportRow.Row[column.TemplateColumnOrdinal - 2].ToString().Trim();
            string subCategory1 = processedImportRow.Row[column.TemplateColumnOrdinal - 1] == DBNull.Value ? "" : processedImportRow.Row[column.TemplateColumnOrdinal - 1].ToString().Trim();
            string subCategory2 = processedImportRow.Row[column.TemplateColumnOrdinal] == DBNull.Value ? "" : processedImportRow.Row[column.TemplateColumnOrdinal].ToString().Trim();

            //main category and all sub category is empty, we can continue with the import without categories
            if (string.IsNullOrWhiteSpace(mainCategory) && string.IsNullOrWhiteSpace(subCategory) && string.IsNullOrWhiteSpace(subCategory1) && string.IsNullOrWhiteSpace(subCategory2))
            {
                column.IgnoreThisTable = true;
                return "";
            }

            //only main Category is available
            if (!string.IsNullOrWhiteSpace(mainCategory) && string.IsNullOrWhiteSpace(subCategory) && string.IsNullOrWhiteSpace(subCategory1) && string.IsNullOrWhiteSpace(subCategory2))
            {
                LookupValue categoryLookup = column.LookupValues.Where(tag => tag.Lookup == mainCategory.ToUpperInvariant()).FirstOrDefault();

                if (categoryLookup != null)
                {
                    return categoryLookup.ReplacementId.ToString();
                }
                processedImportRow.AddException(new ImportDataException { ExceptionType = ExceptionType.CategoryIsNotValid, TemplateMappingColumn = column.TemplateMappingColumn, TemplateColumnName = column.TemplateColumnName, IsValid = false });
                return "";
            }

            //We have  main category and subcategory level 2
            if (!string.IsNullOrWhiteSpace(mainCategory) && !string.IsNullOrWhiteSpace(subCategory) && string.IsNullOrWhiteSpace(subCategory1) && string.IsNullOrWhiteSpace(subCategory2))
            {
                string category = string.Format("{0}||{1}", mainCategory, subCategory).ToUpperInvariant();

                LookupValue categoryLookup = column.LookupValues.Where(tag => tag.Lookup == category).FirstOrDefault();

                if (categoryLookup != null)
                {
                    return categoryLookup.ReplacementId.ToString();
                }
                processedImportRow.AddException(new ImportDataException { ExceptionType = ExceptionType.CategoryIsNotValid, TemplateMappingColumn = column.TemplateMappingColumn, TemplateColumnName = column.TemplateColumnName, IsValid = false });
                return "";
            }

            //We have  main category, subcategory level 2, and subcategory level 3
            if (!string.IsNullOrWhiteSpace(mainCategory) && !string.IsNullOrWhiteSpace(subCategory) && !string.IsNullOrWhiteSpace(subCategory1) && string.IsNullOrWhiteSpace(subCategory2))
            {
                string category = string.Format("{0}||{1}||{2}", mainCategory, subCategory, subCategory1).ToUpperInvariant();

                LookupValue categoryLookup = column.LookupValues.Where(tag => tag.Lookup == category).FirstOrDefault();

                if (categoryLookup != null)
                {
                    return categoryLookup.ReplacementId.ToString();
                }
                processedImportRow.AddException(new ImportDataException { ExceptionType = ExceptionType.CategoryIsNotValid, TemplateMappingColumn = column.TemplateMappingColumn, TemplateColumnName = column.TemplateColumnName, IsValid = false });
                return "";
            }

            //We have  main category, subcategory level 2, subcategory level 3, and subcategory level 4
            if (!string.IsNullOrWhiteSpace(mainCategory) && !string.IsNullOrWhiteSpace(subCategory) && !string.IsNullOrWhiteSpace(subCategory1) && !string.IsNullOrWhiteSpace(subCategory2))
            {
                string category = string.Format("{0}||{1}||{2}||{3}", mainCategory, subCategory, subCategory1,subCategory2).ToUpperInvariant();

                LookupValue categoryLookup = column.LookupValues.Where(tag => tag.Lookup == category).FirstOrDefault();

                if (categoryLookup != null)
                {
                    return categoryLookup.ReplacementId.ToString();
                }
                processedImportRow.AddException(new ImportDataException { ExceptionType = ExceptionType.CategoryIsNotValid, TemplateMappingColumn = column.TemplateMappingColumn, TemplateColumnName = column.TemplateColumnName, IsValid = false });
                return "";
            }

            //All other conditions are invalid hierarchy
            processedImportRow.AddException(new ImportDataException { ExceptionType = ExceptionType.CategoryIsNotValid, TemplateMappingColumn = column.TemplateMappingColumn, TemplateColumnName = column.TemplateColumnName, IsValid = false });
            return "";

        }


        public static string TagDelegateTwoLevel(MappingColumn column, ProcessedImportRow processedImportRow)
        {
            string mainCategory = processedImportRow.Row[column.TemplateColumnOrdinal - 1] == DBNull.Value ? "" : processedImportRow.Row[column.TemplateColumnOrdinal - 1].ToString().Trim();
            string subCategory = processedImportRow.Row[column.TemplateColumnOrdinal] == DBNull.Value ? "" : processedImportRow.Row[column.TemplateColumnOrdinal].ToString().Trim();

            //main category and all sub category is empty, we can continue with the import without categories
            if (string.IsNullOrWhiteSpace(mainCategory) && string.IsNullOrWhiteSpace(subCategory))
            {
                column.IgnoreThisTable = true;
                return "";
            }

            //only main Category is available
            if (!string.IsNullOrWhiteSpace(mainCategory) && string.IsNullOrWhiteSpace(subCategory))
            {
                LookupValue categoryLookup = column.LookupValues.Where(tag => tag.Lookup == mainCategory.ToUpperInvariant()).FirstOrDefault();

                if (categoryLookup != null)
                {
                    return categoryLookup.ReplacementId.ToString();
                }
                processedImportRow.AddException(new ImportDataException { ExceptionType = ExceptionType.CategoryIsNotValid, TemplateMappingColumn = column.TemplateMappingColumn, TemplateColumnName = column.TemplateColumnName, IsValid = false });
                return "";
            }

            //We have  main category and subcategory level 2
            if (!string.IsNullOrWhiteSpace(mainCategory) && !string.IsNullOrWhiteSpace(subCategory))
            {
                string category = string.Format("{0}||{1}", mainCategory, subCategory).ToUpperInvariant();

                LookupValue categoryLookup = column.LookupValues.Where(tag => tag.Lookup == category).FirstOrDefault();

                if (categoryLookup != null)
                {
                    return categoryLookup.ReplacementId.ToString();
                }
                processedImportRow.AddException(new ImportDataException { ExceptionType = ExceptionType.CategoryIsNotValid, TemplateMappingColumn = column.TemplateMappingColumn, TemplateColumnName = column.TemplateColumnName, IsValid = false });
                return "";
            }

            //All other conditions are invalid hierarchy
            processedImportRow.AddException(new ImportDataException { ExceptionType = ExceptionType.CategoryIsNotValid, TemplateMappingColumn = column.TemplateMappingColumn, TemplateColumnName = column.TemplateColumnName, IsValid = false });
            return "";

        }

        
    }
}