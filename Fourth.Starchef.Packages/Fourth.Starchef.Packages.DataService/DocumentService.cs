#region usings

using System.Data;
using Fourth.Starchef.Packages.Data;
using Fourth.Starchef.Packages.Model;

#endregion

namespace Fourth.Starchef.Packages.DataService
{
    public class DocumentService
    {
        public bool AddPackgeToDocuments(Config config, string name, string notes, string fileName, string fileSize,int logId,int packageId)
        {
            using (Dal dal = new Dal(config.ConnString))
            {
                IDataParameter[] param =
                {
                    dal.GetParameter("@name", name),
                    dal.GetParameter("@file_name", fileName),
                    dal.GetParameter("@notes", notes),
                    dal.GetParameter("@created_by", config.UserId),
                    dal.GetParameter("@file_size", fileSize),
                    dal.GetParameter("@log_id", logId),
                    dal.GetParameter("@package_id",packageId)
                };
                
                return dal.ExecuteSql("sc_add_package_to_document", param, CommandType.StoredProcedure) > 0;
            }
        }
    }
}