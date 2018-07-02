using System;
using System.Collections.Generic;
using Fourth.Import.Common;
using Fourth.Import.DataService;
using Fourth.Import.ExcelService;
using Fourth.Import.Model;
using System.Linq;

namespace Fourth.Import.Mapping
{
    public static class Column
    {
        private static Config _config;

        public static IList<MappingTable> Columns(this IList<MappingTable> mappingTable, Config config)
        {
            _config = config;
            return mappingTable.Add();
        }

        public static IList<MappingTable> Add(this IList<MappingTable> mappingTable)
        {
            foreach (MappingTable table in mappingTable)
            {
                table.MappingColumns =
                    AddTableColumns(_config, table.TableName)
                        .LookupValues(_config)
                        .Rules(_config)
                        .AddTemplateHeaderDetails(_config)
                        .AddTemplateHeaderName(_config);
                                
            }

            return mappingTable;
        }

        private static IList<MappingColumn> AddTableColumns(Config config, string tableName)
        {
            using (var mcService = new MappingColumnService(config.TargetConnectionString))
            {
                return mcService.Load(tableName, config.ImportType);
            }
        }


        private static IList<MappingColumn> AddTemplateHeaderDetails(this IList<MappingColumn> mappingColumns, Config config)
        {
            IDictionary<string, int> columnOrdinal= new HeaderService(config.ExcelImportConnectionString).Ordinal(config.ExcelImportSheetName, config.TemplateHeaderColumn);
            foreach (MappingColumn mappingColumn in mappingColumns)
            {   
                int ordinal;
                if (mappingColumn.TemplateMappingColumn != null && columnOrdinal.TryGetValue(mappingColumn.TemplateMappingColumn, out ordinal))
                    mappingColumn.TemplateColumnOrdinal = ordinal;
            }
            return mappingColumns;
        }

        private static IList<MappingColumn> AddTemplateHeaderName(this IList<MappingColumn> mappingColumns, Config config)
        {
            IDictionary<int, string> columnFieldName = new HeaderService(config.ExcelImportConnectionString).FieldName(config.ExcelImportSheetName, config.TemplateHeaderName);

            foreach (MappingColumn mappingColumn in mappingColumns.Where(o=>o.TemplateColumnOrdinal>=0))
                mappingColumn.TemplateColumnName = columnFieldName[mappingColumn.TemplateColumnOrdinal];
            
            return mappingColumns;
        }
    }
}