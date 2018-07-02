using System;
using System.Data;
using Fourth.Import.Data;
using Fourth.Import.Model;

namespace Fourth.Import.DataService
{
    public class OperationTypeService: DalBase
    {
        public OperationTypeService(string connectionString)
            : base(ProviderType.Sql, connectionString)
        {
        }

        public ImportType Get(string templateVersion)
        {
            IDataParameter[] parameters = {GetParameter("@template_version", templateVersion)};

            string sqlQuery = "Select import_operation, prerequisite_query from import_type where version=@template_version";
            using (IDbConnection conn = GetConnection())
            {
                conn.Open();
                using (var dr = GetReader(conn, sqlQuery, parameters, CommandType.Text))
                {
                    if (dr.Read())
                    {
                        var importOperation = (ImportOperation)Enum.Parse(typeof(ImportOperation), dr.GetValue<string>("import_operation"), true);
                        var query = dr.GetValue<string>("prerequisite_query");
                        return new ImportType { Operation = importOperation, PrerequisiteQuery = query };
                    }
                }
            }
            return new ImportType {Operation = ImportOperation.Invalid};
        }

        public int Exec(string prerequisiteQuery)
        {
            return ExecuteSql(prerequisiteQuery, CommandType.Text);
        }
    }
}