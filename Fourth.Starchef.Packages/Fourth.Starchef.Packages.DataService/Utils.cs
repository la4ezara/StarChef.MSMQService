#region usings

using System.Data;
using Fourth.Starchef.Packages.Data;
using Fourth.Starchef.Packages.Model;

#endregion

namespace Fourth.Starchef.Packages.DataService
{
    public static class Utils
    {
        public static void SetDocumentPath(Config config)
        {
            config.DocumentsFolder = GetDbSettingValue(config.ConnString,"sc_get_db_setting","DOCUMENTS_FILEPATH");
        }

        public static string GetAccountAdministrationMessage(Config config)
        {
            return GetDbSettingValue(config.LoginConnString, "sc_get_login_db_setting", "CONFIG_ADMINISTRATION_MESSAGE");
        }

        private static string GetDbSettingValue(string connString,string procName,string settingName)
        {
            using (Dal dal = new Dal(connString))
            {
                IDataParameter[] param =
                {
                    dal.GetParameter("@setting_name", settingName)
                };

                return dal.ExecuteScalar<string>(procName, param, CommandType.StoredProcedure);
            }
        }

        public static void SetUserDetails(Config config)
        {   
            using (Dal dal = new Dal(config.ConnString))
            {
                IDataParameter[] param =
                {
                    dal.GetParameter("@m_userId", config.UserId),
                    dal.GetParameter("@getugroupdetail", false)
                };

                IDataReader dr = dal.GetReader("sc_admin_get_user_detail", param, CommandType.StoredProcedure);

                if (!dr.Read()) return;
                config.UserEmailAddress = dr.GetDrValue<string>(1);
                config.UserFirstName = dr.GetDrValue<string>(3);
                config.UserLastName = dr.GetDrValue<string>(4);
            }
        }
    }
}