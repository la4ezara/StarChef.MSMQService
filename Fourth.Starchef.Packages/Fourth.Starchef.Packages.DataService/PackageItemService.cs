#region usings

using System.Collections.Generic;
using System.Data;
using Fourth.Starchef.Packages.Data;
using Fourth.Starchef.Packages.Model;

#endregion

namespace Fourth.Starchef.Packages.DataService
{
    public class PackageItemService
    {
        public void UpdateConverted(ICollection<AuxDocument> convertedDocuments, Config config)
        {
            foreach (AuxDocument convertedDocument in convertedDocuments)
            {
                using (Dal dal = new Dal(config.ConnString))
                {
                    IDataParameter[] param =
                    {
                        dal.GetParameter("@document_id", convertedDocument.Id),
                        dal.GetParameter("@converted_file_path", convertedDocument.ConvertedFile)
                    };
                    dal.ExecuteSql("sc_package_update_file_converted", param, CommandType.StoredProcedure);
                }
            }
        }
    }
}