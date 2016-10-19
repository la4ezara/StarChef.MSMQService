using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.ServiceProcess;
using System.Xml;

namespace StarChef.Listener
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public string StartServiceIfServerNameEndsWith { get; set; }

        public string ServiceName { get; set; }

        public string DisplayName { get; set; }

        public ServiceAccount ServiceAccount { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public bool MustStartServiceByDefault
        {
            get
            {
                var machineName = Environment.MachineName;
                var startService = false;
                if (!string.IsNullOrEmpty(machineName))
                    machineName = machineName.Trim();
                if (!string.IsNullOrEmpty(machineName) && !string.IsNullOrEmpty(StartServiceIfServerNameEndsWith))
                    startService = machineName.EndsWith(StartServiceIfServerNameEndsWith);
                return startService;
            }
        }

        public ProjectInstaller()
        {
            try
            {
                //uncomment the following line for debugging
                //Debugger.Launch();

                LoadServiceProperties();

                InitializeComponent();

                //Override the service name and display name (so that we can install several copies of the service on the same machine, which was needed for the Yesterday and Training portals).
                foreach (Installer installer in Installers)
                {
                    var installer1 = installer as ServiceInstaller;
                    if (installer1 == null)
                        continue;

                    var serviceInstaller = installer1;
                    serviceInstaller.DisplayName = DisplayName;
                    serviceInstaller.ServiceName = ServiceName;
                }

                //Set the account type and credentials to the service process installer
                serviceProcessInstaller1.Account = ServiceAccount;
                serviceProcessInstaller1.Username = UserName;
                serviceProcessInstaller1.Password = Password;

                //Set the startup type depending of the server name.
                //Please check also the method SchedulerServiceInstaller_AfterInstall right below.
                schedulerServiceInstaller.StartType = MustStartServiceByDefault ? ServiceStartMode.Automatic : ServiceStartMode.Manual;
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        private string GetInstallationTarget()
        {
            var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var folder = location.Replace(System.Reflection.Assembly.GetExecutingAssembly().ManifestModule.Name, "");
            return folder;
        }

        public void LoadServiceProperties()
        {
            var configFile = GetInstallationTarget() + "WindowsServiceCredentials.config";
            
            using (var reader = XmlReader.Create(configFile))
            {
                while (reader.Read())
                {
                    if (!reader.IsStartElement())
                        continue;

                    switch (reader.Name)
                    {
                        case "UserName":
                            if (reader.Read()) UserName = reader.Value;
                            break;
                        case "Password":
                            if (reader.Read()) Password = reader.Value;
                            break;
                        case "StartServiceIfServerNameEndsWith":
                            if (reader.Read()) StartServiceIfServerNameEndsWith = reader.Value;
                            break;
                        case "ServiceAccount":
                            if (reader.Read())
                            {
                                switch (reader.Value)
                                {
                                    case "LocalService":
                                        ServiceAccount = ServiceAccount.LocalService;
                                        break;
                                    case "NetworkService":
                                        ServiceAccount = ServiceAccount.NetworkService;
                                        break;
                                    case "LocalSystem":
                                        ServiceAccount = ServiceAccount.LocalSystem;
                                        break;
                                    case "User":
                                        ServiceAccount = ServiceAccount.User;
                                        break;
                                }
                            }
                            break;
                        case "ServiceName":
                            if (reader.Read()) ServiceName = reader.Value;
                            break;
                        case "DisplayName":
                            if (reader.Read()) DisplayName = reader.Value;
                            break;
                    }
                }
            }
        }

        private void serviceProcessInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {
        }
        
        private void SchedulerServiceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            try
            {
                if (MustStartServiceByDefault) StartTheService();
            }
            catch (Exception ex)
            {
                //If the service fails to start we do not want to revert the installation, so we just log the error
                WriteMessageLog("Unexpected error after installing JobSchedulerWindowsService.", EventLogEntryType.Error, ex);
            }
        }

        private void StartTheService()
        {
            try
            {
                var controller = new ServiceController
                {
                    MachineName = ".",
                    ServiceName = schedulerServiceInstaller.ServiceName
                };
                controller.Start();
            }
            catch (Exception ex)
            {
                WriteMessageLog("Unexpected error while trying to start JobSchedulerWindowsService by the fist time after its installation.", EventLogEntryType.Error, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Try to write a message in the Windows Event Log. If it fails, try to write it in a plain text file.
        /// </summary>
        /// <param name="message">Message describing the entry</param>
        /// <param name="type">Type of entry (error, information. warning, etc)</param>
        /// <param name="ex">The exception raisen</param>
        /// <returns>Returns whether the message could be written in the log or not</returns>
        private void WriteMessageLog(string message, EventLogEntryType type, Exception ex)
        {
            try
            {
                WriteEventLog(message, type, ex);
            }
            catch
            {
                try
                {
                    WriteLogFile(message, type, ex);
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// Write an entry in the Windows Event Log
        /// </summary>
        /// <param name="message">Message describing the entry</param>
        /// <param name="type">Type of entry (error, information. warning, etc)</param>
        /// <param name="ex">The exception raisen</param>
        private void WriteEventLog(string message, EventLogEntryType type, Exception ex)
        {
            const string source = "Starchef Message Listner Installer";
            const string log = "Application";
            if (!EventLog.SourceExists(source)) EventLog.CreateEventSource(source, log);
            EventLog.WriteEntry(source, GenerateDefaultLogEntry(message, ex), type, 0);
        }

        /// <summary>
        /// Write a log entry in a plain text file
        /// </summary>
        /// <param name="message">Message describing the entry</param>
        /// <param name="type">Type of entry (error, information. warning, etc)</param>
        /// <param name="ex">The exception raisen</param>
        private void WriteLogFile(string message, EventLogEntryType type, Exception ex)
        {
            const string newDashedLine = "=============================================================";
            var logEntry = Environment.NewLine + newDashedLine + Environment.NewLine +
                                DateTime.UtcNow + Environment.NewLine +
                                GenerateDefaultLogEntry(message, ex) + Environment.NewLine + Environment.NewLine;
            System.IO.File.AppendAllText(GetInstallationTarget() + "StarchefMessageListnerInstallation.log", logEntry);
        }

        private static string GenerateDefaultLogEntry(string message, Exception ex)
        {
            var logEntry =
                "Error message: " + message + Environment.NewLine +
                "Exception message: " + ex.Message + Environment.NewLine +
                "Stack trace: " + Environment.NewLine + ex.StackTrace + Environment.NewLine;
            if (ex.InnerException != null)
                logEntry += "Inner exception message: " + ex.InnerException.Message;
            return logEntry;
        }
    }
}
