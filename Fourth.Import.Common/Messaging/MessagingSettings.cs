using Fourth.Import.Data;
using log4net;
using System;
using System.Configuration;
using System.Data;
using System.Messaging;
using System.Reflection;

namespace Fourth.Import.Common.Messaging
{
    internal class MessagingSettings : DalBase, IMessagingSettings
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const string CONFIG_KEY_MQ = "StarChef.MSMQ.Queue";

        public MessagingSettings(string connectionString) : base(ProviderType.Sql, connectionString)
        {
        }

        public MessagePriority GetMessagePriority()
        {

            return (MessagePriority) Convert.ToInt32(GetSetting("CONFIG_MSMQ_MESSAGE_PRIORITY"));
        }

        public string GetSetting(string settingName)
        {
            string settingValue = string.Empty;
           
                try
                {
                IDataParameter[] parameters =
                {
                        GetParameter("@setting_name", settingName)
                    };
                using (IDbConnection conn = GetConnection())
                {
                    conn.Open();
                    using (var rdr = GetReader(conn, "sc_get_db_setting", parameters, CommandType.StoredProcedure))
                    {
                        if (rdr.Read())
                        {
                            settingValue = DataReaderExtensions.GetValue<string>(rdr, "db_setting_value");
                        }
                    }
                }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
         
            return settingValue;
        }

        public string GetQueueName()
        {
            return GetStringSetting(CONFIG_KEY_MQ);
        }

        private string GetStringSetting(string settingName)
        {
            try { return ConfigurationManager.AppSettings[settingName]; }
            catch (Exception e) {
                _logger.Error(e);
                throw new Exception("StarChef: Cannot find MSMQ configuration. Key " + settingName + " is missing from web.config file.", e);
            }
        }
    }
}