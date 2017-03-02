using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Configuration;
//necessary for using a timer
using System.Timers;
using System.Messaging;
using System.Threading;
using System.Collections;
using log4net;

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

        /// <summary> The log4net Logger instance. </summary>
        private static readonly ILog Logger =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const long _tickPeriod = 1000; // 1 second!

		private System.Timers.Timer _timer;
		private bool _IsStarted;
		private string _QueuePath;
	    private EventLog log = new EventLog();

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

        private readonly int _maxThreadCount;
        public static ManualResetEvent[] resetEvents;
        public static Hashtable GlobalUpdateTimeStamps;
        public static Hashtable ActiveTaskDatabaseIDs;

		public ListenerSVC()
		{
			// This call is required by the Windows.Forms Component Designer.
			InitializeComponent();
            
            _maxThreadCount = Int32.Parse(ConfigurationSettings.AppSettings.Get("MSMQThreadCount"));
            ThreadPool.SetMaxThreads(_maxThreadCount, 0);
            resetEvents = new ManualResetEvent[_maxThreadCount];
            GlobalUpdateTimeStamps = new Hashtable();
            ActiveTaskDatabaseIDs = new Hashtable();

			//Code to start the timer "tick", which runs the actual MSMQ listener.
			_timer = new System.Timers.Timer {Interval = _tickPeriod};
		    _timer.Elapsed += TimerTick;
		    log.Source = "StarChef-ListenerSVC";
		}

		private void TimerTick(object sender, ElapsedEventArgs e) 
		{
			if (_IsStarted)
			{
			    _timer.Stop();
			    
			    Listener iListener = new Listener(_QueuePath);
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
                    Logger.Error(mqe);
                }
            }
            return ret;
        }

		// The main entry point for the process
		static void Main()
		{
		    // More than one user Service may run within the same process. To add
			// another service to this process, change the following line to
			// create a second service object. For example,
			//
			//   ServicesToRun = new System.ServiceProcess.ServiceBase[] {new ListenerSVC(), new MySecondUserService()};
			//
		    ServiceBase[] ServicesToRun = new ServiceBase[] { new ListenerSVC() };

		    Run(ServicesToRun);
		}

	    /// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			ServiceName = "StarChef.MSMQService";
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
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
			_IsStarted = true;	
			
			_QueuePath = ConfigurationSettings.AppSettings.Get("StarChef.MSMQ.Queue");
			
		}
 
		/// <summary>
		/// Stop this service.
		/// </summary>
		protected override void OnStop()
		{
			_IsStarted = false;

			// TODO: Add code here to perform any tear-down necessary to stop your service.
		}
	}
}
