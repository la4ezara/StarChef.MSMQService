using System.Data;
using Fourth.Import.Data;

namespace Fourth.Import.DataService
{
    public class SuppDistService : DalBase
    {
        public SuppDistService(string connectionString) : base(ProviderType.Sql, connectionString)
        {
        }


        public bool SupplierCodeUsedForProduct(string supplierName,string supplierCode)
        {
            IDataParameter[] parameters = {
                                              GetParameter("@supplier_name",supplierName),
                                              GetParameter("@supplier_code",supplierCode)
                                          };
            using (IDbConnection conn = GetConnection())
            {
                conn.Open();
                using (IDataReader dr = GetReader(conn, "sc_import_supplier_code_validation", parameters, CommandType.StoredProcedure))
                {
                    if (dr.Read())
                    {
                        return dr.GetInt32(0) > 0 ? true : false;
                    }
                }
            }
            return false;
        }

        public bool DistributorCodeUsedForProduct(string distributorName, string distributorCode)
        {
            IDataParameter[] parameters = {
                                              GetParameter("@distributor_name",distributorName),
                                              GetParameter("@distributor_code",distributorCode)
                                          };
            using (IDbConnection conn = GetConnection())
            {
                conn.Open();
                using (IDataReader dr = GetReader(conn, "sc_import_distributor_code_validation", parameters, CommandType.StoredProcedure))
                {
                    if (dr.Read())
                    {
                        return dr.GetInt32(0) > 0 ? true : false;
                    }
                }
            }
            return true;
        }

        public DataTable GetAllSupplierProducts(string fileName)
        {
            DataTable result = new DataTable();

            IDataParameter[] parameters = {
                                              GetParameter("@file_name",fileName)
                                          };

            using (IDbConnection conn = GetConnection())
            {
                conn.Open();
                using (IDataReader dr = GetReader(conn, "sc_import_get_supplier_products", parameters, CommandType.StoredProcedure))
                {
                    result.Load(dr);
                }
            }
            return result;
        }
    }
}