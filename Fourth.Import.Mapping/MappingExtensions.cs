using System;
using System.Collections.Generic;
using System.Linq;
using Fourth.Import.Model;

namespace Fourth.Import.Mapping
{
    public static class MappingExtensions
    {
        public static int? ColumnOrdinalOf(this IList<MappingTable> mappingTables, string tableName, string columnName)
        {

            MappingColumn column = mappingTables.Where(t => t.TableName.ToUpperInvariant() == (tableName!=null ? tableName.ToUpperInvariant() : string.Empty))
                .Select(mappingTable => mappingTable.MappingColumns.Where(i => i.StarchefColumnName.ToUpperInvariant() == columnName.ToUpperInvariant()).FirstOrDefault()).FirstOrDefault();

            if (column != null)
                return column.TemplateColumnOrdinal;
            return null;
        }

        public static int? ColumnOrdinalOf(this MappingTable mappingTable, string columnName)
        {
            MappingColumn column = mappingTable.MappingColumns.Where(c => c.StarchefColumnName.ToUpperInvariant() == columnName.ToUpperInvariant()).FirstOrDefault();

            if (column != null)
                return column.TemplateColumnOrdinal;
            return null;
        }


        public static string ColumnNameOf(this MappingTable mappingTable, string columnName)
        {
            MappingColumn column = mappingTable.MappingColumns.Where(c => c.StarchefColumnName.ToUpperInvariant() == columnName.ToUpperInvariant()).FirstOrDefault();

            return column != null ? column.TemplateColumnName : null;
        }

        public static string MappingNameOf(this MappingTable mappingTable, string columnName)
        {
            MappingColumn column = mappingTable.MappingColumns.Where(c => c.StarchefColumnName.ToUpperInvariant() == columnName.ToUpperInvariant()).FirstOrDefault();

            return column != null ? column.TemplateMappingColumn : null;
        }

        public static List<MappingColumn> GetTemplateColumns(IList<MappingTable> mappingTables)
        {
            var mappingColumns = new List<MappingColumn>();
            foreach (var mappingTable in mappingTables)
            {
                mappingColumns.AddRange(from column in mappingTable.MappingColumns
                                        where column.TemplateColumnName != null
                                        select column);
            }
            return mappingColumns;
        }

        public static int GetMinColumnOrdinal(this IList<MappingTable> mappingTables)
        {
            int startingCell = int.MaxValue;
            foreach (MappingTable mappingTable in mappingTables)
            {
                if (mappingTable.MappingColumns.Count > 0)
                {
                    int tempStartingCell = mappingTable.MappingColumns
                        .Where(col => col.TemplateColumnOrdinal >= 0)
                        .Min(col => col.TemplateColumnOrdinal);

                    if (tempStartingCell < startingCell)
                        startingCell = tempStartingCell;
                }
            }
            
            return startingCell;
        }
       
    }
}