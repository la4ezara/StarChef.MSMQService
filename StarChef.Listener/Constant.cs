using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarChef.Listener
{
    public static class Constant
    {
        public const string CONFIG_LOG4NET_ADO_APPENDER_COMMAND =
            "INSERT INTO listener_log ([ExternalId], [InternalId],[DatabaseGuid],[Date],[Thread],[Level],[Logger],[Message],[Exception]) "+
            "VALUES (@external_id, @internal_id, @database_guid, @log_date, @thread, @log_level, @logger, @message, @exception)";
    }
}
