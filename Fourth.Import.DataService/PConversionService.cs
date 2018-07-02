using System.Data;
using Fourth.Import.Data;

namespace Fourth.Import.DataService
{
    public class PConversionService : DalBase
    {
        public PConversionService(string connectionString)
            : base(ProviderType.Sql, connectionString)
        {
        }

        public bool ValidatePConversionUnit(string unit1, string unit2)
        {
            IDataParameter[] parameters = {
                                              GetParameter("@unit1",unit1),
                                              GetParameter("@unit2",unit2)
                                          };
            using (IDbConnection conn = GetConnection())
            {
                conn.Open();
                using (IDataReader dr = GetReader(conn, "sc_import_pconversion_unit_validation", parameters, CommandType.StoredProcedure))
                {
                    if (dr.Read())
                    {
                        return dr.GetInt32(0) > 0 ? true : false;
                    }
                }
            }
            return false;
        }
    }
}