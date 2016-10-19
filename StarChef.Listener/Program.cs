using System.ServiceProcess;
using log4net.Config;

namespace StarChef.Listener
{
    static class Program
    {
        static void Main()
        {
            //start log4net up
            XmlConfigurator.Configure();

            var servicesToRun = new ServiceBase[]
            {
                new WindowsServiceClass()
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
