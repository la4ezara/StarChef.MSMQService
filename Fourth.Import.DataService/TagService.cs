using System.Data;
using Fourth.Import.Data;

namespace Fourth.Import.DataService
{
    public class TagService : DalBase
    {
        public TagService(string connectionString)
            : base(ProviderType.Sql, connectionString)
        {
        }


        public bool IsRequired()
        {
            using (IDataReader dr = GetReader("sc_import_category_validation", CommandType.StoredProcedure))
            {
                if (dr.Read())
                {
                    return dr.GetBoolean(0);
                }
            }
            return false;
        }

    }
}