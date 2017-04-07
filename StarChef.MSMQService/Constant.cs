using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarChef.MSMQService
{
    public static class Constant
    {
        public const string CONFIG_LOG4NET_ADO_APPENDER_COMMAND =
            "INSERT INTO msmq_log ([OrganizationId],[Date],[Thread],[Level],[Logger],[Message],[Exception]) " +
            "VALUES (@organization_id, @log_date, @thread, @log_level, @logger, @message, @exception)";
    }
}
