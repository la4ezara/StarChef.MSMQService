using System.Data;
using System.Data.OleDb;
using Fourth.Import.Data;

namespace Fourth.Import.ExcelService
{
    public class ImportSheetService : DalBase
    {
        public ImportSheetService(string connectionString)
            : base(ProviderType.OleDb, connectionString)
        {
        }
        public string ImportSheet()
        {
            DataTable dt;
            var res = string.Empty;

            using (IDbConnection conn = GetConnection())
            {
                var oleDbConn = conn as OleDbConnection;
                if (oleDbConn != null)
                {
                    oleDbConn.Open();
                    dt = oleDbConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    if (dt != null && dt.Rows.Count > 0 && dt.Columns.Contains("TABLE_NAME"))
                    {
                        res = dt.Rows[0]["TABLE_NAME"].ToString();
                    }
                }
            }

            return res;
        }
    }
}