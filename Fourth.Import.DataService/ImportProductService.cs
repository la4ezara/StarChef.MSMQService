using System;
using System.Data;
using Fourth.Import.Data;

namespace Fourth.Import.DataService
{
    public class ImportProductService : DalBase
    {
        public ImportProductService(string connectionString) : base(ProviderType.Sql, connectionString)
        {
        }

        public void ValidateStockInventory(int productId)
        {
            IDataParameter[] parameters = {
                                              GetParameter("@product_id",productId),
                                              GetParameter("@product_type_id",1)
                                          };
            var query = "sc_fnb_export_validate_product";
            var res = base.ExecuteSql(query, parameters, CommandType.StoredProcedure);
        }

        
        public bool Valid(string productId)
        {
            int id = 0;
            if (!int.TryParse(productId, out id)) return false;

            IDataParameter[] parameters = {
                                              GetParameter("@product_id",productId)
                                          };

            const string sqlText = @"select 1 from ingredient
                            where product_id = @product_id";
            using (IDbConnection conn = GetConnection())
            {
                conn.Open();

                var res = ExecuteSql<int>(conn, sqlText, parameters, CommandType.Text);
                if (res > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Valid(string supplierName, string supplierCode)
        {
            IDataParameter[] parameters = {
                                              GetParameter("@supplier_name",supplierName),
                                              GetParameter("@supplier_code",supplierCode)
                                          };

            const string sqlText = @"select 1 from ingredient i 
                            LEFT JOIN supplier s ON s.supplier_id = i.supplier_id
                            where s.supplier_name = @supplier_name AND i.supplier_code = @supplier_code";
            using (IDbConnection conn = GetConnection())
            {
                conn.Open();

                var res = ExecuteSql<int>(conn, sqlText, parameters, CommandType.Text);
                if (res > 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}