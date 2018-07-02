using Fourth.Import.Data;
using Fourth.Import.Model;
using System;
using System.Collections.Generic;
using System.Data;

namespace Fourth.Import.DataService
{
    public class MappingColumnService : DalBase
    {
        public MappingColumnService(string connectionString) : base(ProviderType.Sql, connectionString)
        {
        }

        public IList<MappingColumn> Load(string tableName, string importType)
        {
            IDataParameter[] parameters = {
                                              GetParameter("@table_name",tableName),
                                              GetParameter("@import_type",importType)
                                          };

            const string sqlText = @"
                    select icm.template_column_name,icm.column_name,idt.datatype,icm.lookup_table_id,icm.db_setting_id,icm.default_value,icm.is_mandatory,ilp.lookup_type,ignore_for_import
                    from import_column_mapping icm
                    INNER JOIN import_type_column_mapping itcm on icm.mapping_column_id = itcm.mapping_column_id
		            INNER JOIN import_type it on itcm.import_type_id = it.import_type_id and it.version = @import_type
                    INNER JOIN import_table_mapping itm ON itm.mapping_table_id = icm.mapping_table_id
                    INNER JOIN import_datatype idt ON idt.import_datatype_id = icm.datatype_id
                    LEFT JOIN import_lookup_table ilp ON ilp.lookup_id = icm.lookup_table_id
                    where itcm.is_deleted = 0 and itm.table_name = @table_name";

            IList<MappingColumn> mappingColumns = new List<MappingColumn>();
            using (IDbConnection conn = GetConnection())
            {
                conn.Open();
                using (IDataReader dr = GetReader(conn, sqlText, parameters, CommandType.Text))
                {
                    while (dr.Read())
                        mappingColumns.Add(new MappingColumn
                        {
                            TemplateMappingColumn = dr.GetValue<string>("template_column_name"),
                            StarchefColumnName = dr.GetValue<string>("column_name"),
                            Datatype = (Datatype)Enum.Parse(typeof(Datatype), dr.GetValue<string>("datatype"), true),
                            LookupTableId = dr.GetValue<byte?>("lookup_table_id"),
                            ValidationSettingId = dr.GetValue<int?>("db_setting_id"),
                            DefaultValue = dr.GetValue<string>("default_value"),
                            Mandatory = dr.GetValue<bool>("is_mandatory"),
                            LookupType = (LookupType)Enum.Parse(typeof(LookupType), dr.GetValue<string>("lookup_type") ?? "None", true),
                            TemplateColumnOrdinal = -1,
                            IgnoreForImport = dr.GetValue<bool>("ignore_for_import"),
                            IgnoreThisTable = false
                        });
                }
            }
            return mappingColumns;
        }
    }
}