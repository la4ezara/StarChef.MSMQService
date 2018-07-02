using Fourth.Import.Data;
using Fourth.Import.Model;
using System.Collections.Generic;
using System.Data;

namespace Fourth.Import.DataService
{
    //TODO: change the tag loading rules based on the main category, category type
    public class TagLookupService : LookupService
    {
        public TagLookupService(string connectionString) : base(connectionString)
        {}

        public int ValidateCostCentreCategory(string costCentreCategoryType, string costCentreMainCategory, string costCentreSubCategory)
        {
            IDataParameter[] parameters = {
                                              GetParameter("@cost_centre_category_type",costCentreCategoryType),
                                              GetParameter("@cost_centre_main_category",costCentreMainCategory),
                                              GetParameter("@cost_centre_sub_category",costCentreSubCategory)
                                          };
            using (IDbConnection conn = GetConnection())
            {
                conn.Open();
                using (IDataReader dr = GetReader(conn, "sc_import_cost_cenre_category_validation", parameters, CommandType.StoredProcedure))
                {
                    if (dr.Read())
                    {
                        return dr.GetInt16(0);
                    }
                }
            }
            return 0;
        }
    }
}