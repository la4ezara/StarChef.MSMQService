using System;
using System.ServiceProcess;

namespace StarChef.SqlQueue.Service
{
    public class ListenerService : ServiceBase
	{
        private readonly ServiceRunner _serviceRunner;

		public ListenerService()
		{
            _serviceRunner = new ServiceRunner();
            InitializeComponent();
		}

	    private void InitializeComponent()
	    {
            ServiceName = "StarChef.SqlQueue.Service";
	    }

	    static void Main()
	    {
            if (!Environment.UserInteractive)
            {
                var servicesToRun = new ServiceBase[] { new ListenerService() };
                Run(servicesToRun);
            }
            else
            {
                var runner = new ServiceRunner();
                runner.Start();
                Console.WriteLine("Press any key to stop service");
                Console.ReadLine();
                Console.WriteLine("Stopping");
                runner.Stop();
                Console.WriteLine("Stopped");
                Console.ReadLine();
            }
        }

	    /// <summary>
	    /// Set things in motion so your service can do its work.
	    /// </summary>
	    protected override void OnStart(string[] args)
	    {
            _serviceRunner.Start();
	    }

        /// <summary>
        /// Stop this service.
        /// </summary>
        protected override void OnStop()
		{
            _serviceRunner.Stop();
        }

	    protected override void OnContinue()
	    {
            _serviceRunner.Continue();
        }

	    protected override void OnPause()
	    {
            _serviceRunner.Pause();
        }
        
	    protected override void OnShutdown()
	    {
            _serviceRunner.Stop();
            //_logger.Info("Service is shutdown.");
        }

	    /// <summary>
	    /// Clean up any resources being used.
	    /// </summary>
	    protected override void Dispose( bool disposing )
	    {
	        if( disposing )
	        {
	            if (_serviceRunner != null) 
	            {
                    _serviceRunner.Dispose();
	            }
	        }
	        base.Dispose( disposing );
	    }
	}
}
