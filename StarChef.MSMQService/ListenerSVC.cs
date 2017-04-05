using System;
using System.ComponentModel;
using System.ServiceProcess;
//necessary for using a timer
using System.Timers;
using System.Messaging;
using System.Threading;
using System.Collections;
using log4net;
using StarChef.MSMQService.Configuration;
using StarChef.MSMQService.Configuration.Impl;

namespace StarChef.MSMQService
{
	/// <summary>
	/// ListenerSVC -- StarChef.MSMQService.exe
	/// 
	/// Deployment
	/// 1) Copy the StarChef.MSMQService.exe togetger with StarChef.MSMQService.exe.config and StarChef.Data.dll file to the 
	///    desired destination
	///	2) Edit the StarChef.MSMQService.exe.config, property StarChef_QueueName to reflect the name of the queue that 
	///	   is to be listened
	///	3) Run C:\WINDOWS\Microsoft.NET\Framework\v1.1.4322\installutil StarChef.MSMQService.exe		
	/// 
	/// </summary>
	public class ListenerSVC : ServiceBase
	{
	    private readonly IAppConfiguration _appConfiguration;
	    private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const long TICK_PERIOD = 1000; // 1 second!
		private readonly System.Timers.Timer _timer;
		private bool _isStarted;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container _components = null;

	    public static ManualResetEvent[] ResetEvents;
        public static Hashtable GlobalUpdateTimeStamps;
        public static Hashtable ActiveTaskDatabaseIDs;

		public ListenerSVC(IAppConfiguration appConfiguration)
		{
		    _appConfiguration = appConfiguration;
		    // This call is required by the Windows.Forms Component Designer.
		    InitializeComponent();
            
            var maxThreadCount = _appConfiguration.MsmqThreadCount;
            ThreadPool.SetMaxThreads(maxThreadCount, 0);
            ResetEvents = new ManualResetEvent[maxThreadCount];
            GlobalUpdateTimeStamps = new Hashtable();
            ActiveTaskDatabaseIDs = new Hashtable();

			//Code to start the timer "tick", which runs the actual MSMQ listener.
			_timer = new System.Timers.Timer {Interval = TICK_PERIOD};
		    _timer.Elapsed += TimerTick;
		}

		private void TimerTick(object sender, ElapsedEventArgs e) 
		{
			if (_isStarted)
			{
			    _timer.Stop();
			    
			    var iListener = new Listener(_appConfiguration);
                ThreadPool.QueueUserWorkItem(iListener.Listen);
                
			    _timer.Start();
			}
		}
        
        protected int GetMessageCount(string mqName)
        {
            MessageQueue q = new MessageQueue(mqName);
            int count = 0;
            Cursor cursor = q.CreateCursor();

            Message m = PeekWithoutTimeout(q, cursor, PeekAction.Current);
            if (m != null)
            {
                count = 1;
                while ((PeekWithoutTimeout(q, cursor, PeekAction.Next)) != null)
                {
                    count++;
                }
            }
            return count;
        }

        protected Message PeekWithoutTimeout(MessageQueue q, Cursor cursor, PeekAction action)
        {
            Message ret = null;
            try
            {
                ret = q.Peek(new TimeSpan(1), cursor, action);
            }
            catch (MessageQueueException mqe)
            {
                if (mqe.MessageQueueErrorCode != MessageQueueErrorCode.IOTimeout &&
                    mqe.MessageQueueErrorCode != MessageQueueErrorCode.MessageNotFound)
                {
                    _logger.Error(mqe);
                }
            }
            return ret;
        }

		// The main entry point for the process
		static void Main()
		{
		    var servicesToRun = new ServiceBase[] { new ListenerSVC(new AppConfiguration()) };

		    Run(servicesToRun);
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

		/// <summary>
		/// Set things in motion so your service can do its work.
		/// </summary>
		protected override void OnStart(string[] args)
		{
			//start the timer.
			_timer.Enabled = true;
			_isStarted = true;
			
		}
 
		/// <summary>
		/// Stop this service.
		/// </summary>
		protected override void OnStop()
		{
			_isStarted = false;

			// TODO: Add code here to perform any tear-down necessary to stop your service.
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
	}
}
