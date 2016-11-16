using System;
using System.ServiceProcess;
using log4net;

namespace StarChef.Listener
{
    public class WindowsServiceClass : ServiceBase
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(WindowsServiceClass));

        public WindowsServiceClass() { }

        protected override void OnStart(string[] args)
        {
            // Set the application ID that is reported on every log message
            log4net.GlobalContext.Properties["ApplicationId"] = "StarChef.Listener";
            ConfigObjectMapping.Init();

            Logger.Info("Starting StarChef.Listener service");

            try
            {
                MessageBusController.Start();
            }
            catch(Exception ex)
            {
                Logger.Info("Failed to start StarChef.Listener service due to unexpected error", ex);
                throw;
            }
        }

        protected override void OnStop()
        {
            Logger.Info("Stopping StarChef.Listener service");

            try
            {
                MessageBusController.Stop();
            }
            catch (Exception ex)
            {
                Logger.Info("Failed to stop StarChef.Listener service due to unexpected error", ex);
                throw;
            }
        }
    }
}
