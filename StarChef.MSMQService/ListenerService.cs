using System;
using System.ComponentModel;
using System.ServiceProcess;
using System.Collections;
using log4net;
using StarChef.MSMQService.Configuration;
using Autofac;
using log4net.Config;
using StarChef.Common;
using IContainer = Autofac.IContainer;
using System.Timers;

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
        private bool _isProcessing;
        private IContainer _container;
        private object _locker = new object();
        private object _lockerProcessing = new object();

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private Container _components;

	    public static Hashtable GlobalUpdateTimeStamps;
        public static Hashtable ActiveTaskDatabaseIDs;

		public ListenerService()
		{
            InitializeComponent();

            // Start log4net up
            XmlConfigurator.Configure();
            log4netHelper.ConfigureAdoAppenderCommandText(Constant.CONFIG_LOG4NET_ADO_APPENDER_COMMAND);
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
	        var servicesToRun = new ServiceBase[] { new ListenerService() };

	        Run(servicesToRun);
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
            System.Threading.ThreadPool.SetMaxThreads(maxThreadCount, 0);
	        GlobalUpdateTimeStamps = new Hashtable();
	        ActiveTaskDatabaseIDs = new Hashtable();

	        var periodSetting = _appConfiguration.Interval;
	        var period = TimeSpan.FromMilliseconds(periodSetting);
	        //_isStarted = true;
            _logger.DebugFormat("Service is configured to run each {0} ms", periodSetting);
            _timer = new Timer(period.TotalMilliseconds);
            _timer.Elapsed += TimerElapsed;
            this.TimerElapsed(null, null);
            _timer.Start();
            //_timer = new Timer(ServiceTask, null, TimeSpan.Zero, period);
	    }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!_isProcessing)
                {
                    lock (this._locker)
                    {
                        _isProcessing = true;
                        _timer.Stop();
                    }
                    var listener = _container.Resolve<IListener>();
                    listener.ExecuteAsync().Wait();
                }
            }
            catch (AggregateException aEx)
            {
                _logger.Error(aEx.GetBaseException());
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                lock (this._locker)
                {
                    _isProcessing = false;
                    _timer.Start();
                }
            }
        }

        /// <summary>
        /// Stop this service.
        /// </summary>
        protected override void OnStop()
		{
            while (_isProcessing) {
                System.Threading.Thread.Sleep(300);
            }

            lock (this._locker) {
                _timer.Stop();
            }
            
            _logger.Info("Service is stopped.");
        }

	    protected override void OnContinue()
	    {
            lock (this._locker)
            {
                _timer.Start();
            }
            _logger.Info("Service is continued.");
	    }

	    protected override void OnPause()
	    {
            while (_isProcessing)
            {
                System.Threading.Thread.Sleep(300);
            }

            lock (this._locker)
            {
                _timer.Stop();
            }
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
	            if (_components != null) 
	            {
	                _components.Dispose();
	            }
	        }
	        base.Dispose( disposing );
	    }
	}
}
