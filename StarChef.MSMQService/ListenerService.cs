using System;
using System.ComponentModel;
using System.ServiceProcess;
using System.Threading;
using System.Collections;
using log4net;
using StarChef.MSMQService.Configuration;
using Autofac;
using log4net.Config;
using StarChef.Common;
using IContainer = Autofac.IContainer;

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
	    private IAppConfiguration _appConfiguration;
	    private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private Timer _timer;
        private IContainer _container;

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private Container _components;

	    public static Hashtable GlobalUpdateTimeStamps;
        public static Hashtable ActiveTaskDatabaseIDs;

	    static ListenerService()
	    {
            // Start log4net up
            XmlConfigurator.Configure();
            log4netHelper.ConfigureAdoAppenderCommandText(Constant.CONFIG_LOG4NET_ADO_APPENDER_COMMAND);
        }

        public ListenerService()
		{
            InitializeComponent();
		}

	    /// <summary> 
	    /// Required method for Designer support - do not modify 
	    /// the contents of this method with the code editor.
	    /// </summary>
	    private void InitializeComponent()
	    {
	        _components = new Container();
	        ServiceName = "StarChef.MSMQService";
	    }

	    static void Main()
	    {
            _logger.Info("Initializing...");

            var servicesToRun = new ServiceBase[] { new ListenerService() };

            _logger.Info("Running...");

            Run(servicesToRun);
	    }

	    private void ServiceTask(object state)
	    {
	        try
	        {
	            var listener = _container.Resolve<IListener>();
	            listener.ExecuteAsync().Wait();
	        }
	        catch (AggregateException e)
	        {
	            _logger.Error(e.GetBaseException());
	        }
	        catch (Exception e)
	        {
	            _logger.Error(e);
	        }
	    }

	    // The main entry point for the process


	    /// <summary>
	    /// Set things in motion so your service can do its work.
	    /// </summary>
	    protected override void OnStart(string[] args)
	    {
	        _logger.Info("Service is started.");

	        var builder = new ContainerBuilder();
	        builder.RegisterModule<DependencyConfiguration>();
	        _container = builder.Build();

	        _appConfiguration = _container.Resolve<IAppConfiguration>();
	        var maxThreadCount = _appConfiguration.MsmqThreadCount;
	        ThreadPool.SetMaxThreads(maxThreadCount, 0);
	        GlobalUpdateTimeStamps = new Hashtable();
	        ActiveTaskDatabaseIDs = new Hashtable();

	        var periodSetting = _appConfiguration.Interval;
	        var period = TimeSpan.FromMilliseconds(periodSetting);

            _logger.DebugFormat("Service is configured to run each {0} ms", periodSetting);
            _timer = new Timer(ServiceTask, null, TimeSpan.Zero, period);
	    }

	    /// <summary>
        /// Stop this service.
        /// </summary>
        protected override void OnStop()
		{
			_logger.Info("Service is stopped.");
        }

	    protected override void OnContinue()
	    {
            _logger.Info("Service is continued.");
	    }

	    protected override void OnPause()
	    {
            _logger.Info("Service is paused.");
        }

	    protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
	    {
            _logger.InfoFormat("Power event is occurred: {0}.", powerStatus);
            return true;
        }

	    protected override void OnShutdown()
	    {
            _logger.Info("Service is shutdown.");
        }

	    /// <summary>
	    /// Clean up any resources being used.
	    /// </summary>
	    protected override void Dispose( bool disposing )
	    {
	        if( disposing )
	        {
	            _components?.Dispose();
	        }
	        base.Dispose( disposing );
	    }
	}
}
