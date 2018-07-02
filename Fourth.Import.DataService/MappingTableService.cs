using Fourth.Import.Data;
using Fourth.Import.Model;
using System.Collections.Generic;
using System.Data;

namespace Fourth.Import.DataService
{
    public class MappingTableService : DalBase
    {
        public MappingTableService(string connectionString) : base(ProviderType.Sql, connectionString)
        {
        }


        public IList<MappingTable> Load(string importType)
        {
            IDataParameter[] parameters = {
                                              GetParameter("@import_type",importType)
                                          };

            const string sqlText = @"
                    select table_name,process_order,start_with_query,end_with_query,generate_query,ISNULL(ittm.dml_type_id,0) as dml_type_id from import_table_mapping itm
                    INNER JOIN import_type_table_mapping ittm ON itm.mapping_table_id = ittm.mapping_table_id
                    INNER JOIN import_type it ON it.import_type_id = ittm.import_type_id
                    where ittm.is_deleted = 0 and it.version=@import_type";

            IList<MappingTable> mappingTables = new List<MappingTable>();
            using (IDbConnection conn = GetConnection())
            {
                conn.Open();
                using (IDataReader dr = GetReader(conn, sqlText, parameters, CommandType.Text))
                {
                    while (dr.Read())
                        mappingTables.Add(new MappingTable
                        {
                            TableName = dr.GetValue<string>("table_name"),
                            ProcessingOrder = dr.GetValue<int>("process_order"),
                            StartQueryWith = dr.GetValue<string>("start_with_query"),
                            EndQueryWith = dr.GetValue<string>("end_with_query"),
                            GenerateQuery = dr.GetValue<bool>("generate_query"),
                            DmlType = (DmlType)dr.GetValue<byte>("dml_type_id")
                        });
                }
            }
            return mappingTables;
        }
    }
}