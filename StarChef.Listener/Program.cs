using System.ServiceProcess;
using log4net.Config;
using StarChef.Common;

namespace StarChef.Listener
{
    static class Program
    {
        static void Main()
        {
            //start log4net up
            XmlConfigurator.Configure();

            log4netHelper.ConfigureAdoAppenderCommandText(Constant.CONFIG_LOG4NET_ADO_APPENDER_COMMAND);

            var servicesToRun = new ServiceBase[]
            {
                new WindowsServiceClass()
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
