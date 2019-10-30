using System;
using System.ServiceProcess;
using log4net;
using log4net.Config;
using StarChef.Common;

namespace StarChef.Listener
{
    public class WindowsServiceClass : ServiceBase
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public WindowsServiceClass()
        {
            XmlConfigurator.Configure();
            GlobalContext.Properties["component"] = "Fourth.StarChef.Listener";
        }

        static void Main()
        {
            if (!Environment.UserInteractive)
            {
                var servicesToRun = new ServiceBase[] { new WindowsServiceClass() };
                Run(servicesToRun);
            }
            else
            {
                var servicesToRun = new WindowsServiceClass();
                servicesToRun.StartService(null);
                Console.WriteLine("The listener is running - press any key to stop...");
                Console.ReadKey(true);
                servicesToRun.StopService();
            }
        }

        /// <inheritdoc />
        public void StartService(string[] args)
        {
            this.OnStart(args);
        }

        /// <inheritdoc />
        public void StopService()
        {
            this.OnStop();
        }

        protected override void OnStart(string[] args)
        {
            // Set the application ID that is reported on every log message
            GlobalContext.Properties["ApplicationId"] = "StarChef.Listener";
            ConfigObjectMapping.Init();

            _logger.Info("Started");

            try
            {
                MessageBusController.Start();
            }
            catch(Exception ex)
            {
                _logger.Info("Failed to start StarChef.Listener service due to unexpected error", ex);
                throw;
            }
        }

        protected override void OnContinue()
        {
            _logger.Info("Continuing");
        }

        protected override void OnPause()
        {
            _logger.Info("Paused");
        }

        protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
        {
            _logger.Info("Power event: " + powerStatus);
            return true;
        }

        protected override void OnShutdown()
        {
            _logger.Info("Shutdown");
        }

        protected override void OnStop()
        {
            try { MessageBusController.Stop(); }
            catch (Exception ex)
            {
                _logger.Info("Failed to stop StarChef.Listener service due to unexpected error", ex);
                throw;
            }
            finally
            {
                _logger.Info("Stopped");
            }
        }
    }
}
