using System.Collections.Generic;
using System.Data;
using Fourth.Import.Data;
using Fourth.Import.Model;

namespace Fourth.Import.DataService
{
    public class LookupService : DalBase
    {
        public LookupService(string connectionString) : base(ProviderType.Sql, connectionString)
        {
        }

        protected string GetLookupSql(byte lookupId)
        {
            IDataParameter[] parameters = {
                                              GetParameter("@lookup_id",lookupId)
                                          };
            const string sqlText = @"select replace_with,lookup_column,table_name,condition
                            from import_lookup_table where lookup_id = @lookup_id";
            using (IDbConnection conn = GetConnection())
            {
                conn.Open();
                using (IDataReader dr = GetReader(conn, sqlText, parameters, CommandType.Text))
                {
                    if (dr.Read())
                    {
                        return string.Format("select CAST({0} as INT) as ReplacementId,{1} from {2} {3}",
                                                         dr.GetValue<string>("replace_with"),
                                                         dr.GetValue<string>("lookup_column"),
                                                         dr.GetValue<string>("table_name"),
                                                         dr.GetValue<string>("condition")
                                                     );
                    }
                    return "select 1 ReplacementId,1"; //this should never happen
                }
            }
        }


        public HashSet<LookupValue> Add(byte lookupId)
        {
            HashSet<LookupValue> lookupValues = new HashSet<LookupValue>();
            using (IDbConnection conn = GetConnection())
            {
                conn.Open();
                using (IDataReader dr = GetReader(conn, GetLookupSql(lookupId), CommandType.Text))
                {
                    while (dr.Read())
                    {
                        lookupValues.Add(new LookupValue()
                        {
                            ReplacementId = dr.GetValue<int>("ReplacementId"),
                            Lookup = dr.GetValue<string>(1).ToUpperInvariant()
                        }
                            );
                    }
                }
            }
            return lookupValues;
        }
    }
}