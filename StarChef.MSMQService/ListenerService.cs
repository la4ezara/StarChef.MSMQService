using System;
using System.ServiceProcess;

namespace StarChef.MSMQService
{
    /// <summary>
    /// ListenerSVC -- StarChef.MSMQService.exe
    /// 
    /// Deployment
    /// 1) Copy the StarChef.MSMQService.exe together with StarChef.MSMQService.exe.config and StarChef.Data.dll file to the 
    ///    desired destination
    ///	2) Edit the StarChef.MSMQService.exe.config, property StarChef_QueueName to reflect the name of the queue that 
    ///	   is to be listened
    ///	3) Run C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\installutil StarChef.MSMQService.exe		
    /// 
    /// </summary>
    public class ListenerService : ServiceBase
	{
        private ServiceRunner _serviceRunner;

		public ListenerService()
		{
            _serviceRunner = new ServiceRunner();
            InitializeComponent();
		}

	    /// <summary> 
	    /// Required method for Designer support - do not modify 
	    /// the contents of this method with the code editor.
	    /// </summary>
	    private void InitializeComponent()
	    {
            ServiceName = "StarChef.MSMQService";
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
                //UpdateMessage msg = new UpdateMessage() { DatabaseID = 6, DSN= "Data Source = 10.10.10.109\\devtest; User ID = sl_web_user; Password = reddevil; Initial Catalog = SCNET_demo_qa", Action = (int)Fourth.StarChef.Invariables.Constants.MessageActionType.UpdatedProductCost, SubAction = (int)Fourth.StarChef.Invariables.Constants.MessageSubActionType.Update, ProductID = 12345 };
                //for (int i = 0; i < 50000; i++)
                //{
                //    MSMQHelper.Send(msg, runner.Configuration.NormalQueueName, runner.Configuration.PoisonQueueName);
                //}

                runner.Start();
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

	    //protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
	    //{
     //       _logger.InfoFormat("Power event is occurred: {0}.", powerStatus);
     //       return true;
     //   }

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
