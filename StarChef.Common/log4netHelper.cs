using log4net.Appender;
using log4net.Repository.Hierarchy;
using System;
using System.Linq;

namespace StarChef.Common
{
    public static class log4netHelper
    {
        public static void ConfigureAdoAppenderCommandText(string commandText)
        {
            Hierarchy hier = log4net.LogManager.GetRepository() as Hierarchy;

            if (hier != null)
            {
                // Get ADONetAppender
                var adoAppender =
                    (AdoNetAppender)hier.GetAppenders().Where(
                        appender => appender.Name.Equals("ADONetAppender", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                if (adoAppender != null)
                {
                    adoAppender.CommandText = commandText;

                    //refresh settings of appender
                    adoAppender.ActivateOptions();
                }
            }
        }
    }
}
