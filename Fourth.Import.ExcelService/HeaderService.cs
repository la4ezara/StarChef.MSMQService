using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Fourth.Import.Data;

namespace Fourth.Import.ExcelService
{
    public class HeaderService: DalBase
    {
        public HeaderService(string connectionString) : base(ProviderType.OleDb, connectionString)
        {}

        public IDictionary<string,int> Ordinal(string sheetName,string range)
        {
            string sqlText = string.Format("SELECT * FROM [{0}{1}]",sheetName,range);

            Dictionary<string, int> templateOrdinals = new Dictionary<string, int>();
            using (IDbConnection conn = GetConnection())
            {
                conn.Open();
                using (IDataReader dr = GetReader(conn,sqlText, CommandType.Text))
                {
                    if (dr.Read())
                    {
                        //Note: there should be an easier way to do this, try Enumerable.Range and ToDictionary

                        var values = new Object[dr.FieldCount];
                        int fieldCount = dr.GetValues(values);
                        for (int i = 0; i < fieldCount; i++)
                        {
                            if (!string.IsNullOrEmpty(values[i].ToString()))
                                if (!templateOrdinals.ContainsKey(values[i].ToString()))
                                    templateOrdinals.Add(values[i].ToString(), i);
                        }
                    }
                }
            }
            return templateOrdinals;
        }



        public IDictionary<int,string> FieldName(string sheetName, string range)
        {
            string sqlText = string.Format("SELECT * FROM [{0}{1}]", sheetName, range);

            Dictionary<int, string> templateFieldNames = new Dictionary<int, string>();
            using (IDbConnection conn = GetConnection())
            {
                conn.Open();
                using (IDataReader dr = GetReader(conn, sqlText, CommandType.Text))
                {
                    if (dr.Read())
                    {
                        //Note: there should be an easier way to do this, try Enumerable.Range and ToDictionary

                        var values = new Object[dr.FieldCount];
                        int fieldCount = dr.GetValues(values);
                        for (int i = 0; i < fieldCount; i++)
                        {
                            templateFieldNames.Add(i, values[i].ToString());
                        }
                    }
                }
            }
            return templateFieldNames;
        }


    }
}