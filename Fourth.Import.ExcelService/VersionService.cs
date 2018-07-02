using System.Data;
using System.Data.OleDb;
using Fourth.Import.Data;

namespace Fourth.Import.ExcelService
{
    public class VersionService : DalBase
    {
        public VersionService(string connectionString) : base(ProviderType.OleDb, connectionString)
        {
        }
        public string TemplateVersion(string sheetName, string range)
        {
            string sqlText = string.Format("SELECT * FROM [{0}{1}]", sheetName, range);
            try
            {
                using (IDbConnection conn = GetConnection())
                {
                    conn.Open();
                    using (IDataReader dr = GetReader(conn, sqlText, CommandType.Text))
                    {
                        return dr.Read() ? dr.GetString(0) : "INVALID";
                    }
                }
            }
            catch (OleDbException)
            {
                Dispose();
                throw;
            }
        }
    }
}