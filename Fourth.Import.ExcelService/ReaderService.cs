using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Fourth.Import.Data;

namespace Fourth.Import.ExcelService
{
    public class ReaderService : DalBase
    {
        public ReaderService(string excelConnectionString) : base(ProviderType.OleDb, excelConnectionString)
        {}

        public DataTable Load(string sheetName, int skipRows)
        {
            string sqlText = string.Format("SELECT * FROM [{0}]", sheetName);

            DataTable dataTable = new DataTable("importData");
            using (IDbConnection conn = GetConnection())
            {
                conn.Open();
                using (IDataReader dr = GetReader(conn, sqlText, CommandType.Text))
                {
                    dataTable.Load(dr);
                }
            }

            return Skip(dataTable, skipRows);
        }

        private static DataTable Skip(DataTable dataTable, int skipRows)
        {
            int skippedRows = 0;
            while (skippedRows != skipRows && dataTable.Rows.Count > 0)
            {
                skippedRows++;
                dataTable.Rows.RemoveAt(0);
            }
            return dataTable;
        }
    }
}