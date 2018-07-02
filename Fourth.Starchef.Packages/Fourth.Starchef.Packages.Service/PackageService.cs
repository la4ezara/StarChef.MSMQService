#region usings

using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Timers;
using Fourth.Starchef.Packages.DataService;
using Fourth.Starchef.Packages.Model;

#endregion

namespace Fourth.Starchef.Packages.Service
{
    public partial class PackageService : ServiceBase
    {
        private readonly Config _config;
        private readonly Timer _timer;

        public PackageService()
        {
            _config = new ConfigrationManager().Load();

            _timer = new Timer
            {
                Interval = new TimeSpan(0, _config.PollIntervalMinutes, 0).TotalMilliseconds,
                Enabled = true
            };
            _timer.Elapsed += OnElapsed;

            InitializeComponent();
        }

        private void OnElapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            MsmqListener msmqListener = new MsmqListener();
            UpdateMessage updateMessage = msmqListener.PollQueue(_config.PackageQueuePath);

            if (updateMessage != null)
            {
                _config.UserId = updateMessage.UserId;
                _config.ConnString = updateMessage.DSN;
                Utils.SetUserDetails(_config);
                Utils.SetDocumentPath(_config);

                try
                {
                    Manager.Package package = new Manager.Package();

                    package.Create(_config, updateMessage.ProductID);

                }
                catch (Exception ex)
                {
                    AddLog(ex.Message);
                    _timer.Start();
                    //swallow exception so that the service does not crash
                }
            }
            _timer.Start();
        }

        protected override void OnStart(string[] args)
        {
            AddLog("Service started");
        }

        protected override void OnStop()
        {
            AddLog("Service stopped");
        }

        private static void AddLog(string message)
        {
            EventLog eventLog = new EventLog
            {
                Source = "Fourth.Starchef.Packages"
            };
            eventLog.WriteEntry(message);
        }
    }
}