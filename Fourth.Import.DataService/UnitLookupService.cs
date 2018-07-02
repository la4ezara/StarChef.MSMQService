using Fourth.Import.Data;
using Fourth.Import.Model;
using System.Collections.Generic;
using System.Data;

namespace Fourth.Import.DataService
{
    public class UnitLookupService : LookupService
    {
        public UnitLookupService(string connectionString) : base(connectionString)
        {}

        public new HashSet<LookupValue> Add(byte lookupId)
        {
            HashSet<LookupValue> lookupValues = new HashSet<LookupValue>();
            using (IDbConnection conn = GetConnection())
            {
                conn.Open();
                using (IDataReader dr = GetReader(conn, GetLookupSql(lookupId), CommandType.Text))
                {
                    while (dr.Read())
                    {
                        //Add unit names
                        lookupValues.Add(new LookupValue()
                        {
                            ReplacementId = dr.GetValue<int>("ReplacementId"),
                            Lookup = dr.GetValue<string>(1).ToUpperInvariant()
                        }
                            );

                        //Add unit codes
                        lookupValues.Add(new LookupValue()
                        {
                            ReplacementId = dr.GetValue<int>("ReplacementId"),
                            Lookup = dr.GetValue<string>(2).ToUpperInvariant()
                        }
                            );
                    }
                }
            }
            return lookupValues;
        }
    }
}