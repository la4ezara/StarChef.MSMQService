using System.Collections;
using System.ComponentModel;

namespace StarChef.SqlQueue.Service
{
	/// <summary>
	/// Summary description for ProjectInstaller.
	/// </summary>
	[RunInstaller(true)]
	public class ProjectInstaller : System.Configuration.Install.Installer
	{
		private System.ServiceProcess.ServiceProcessInstaller SqlQueueService_Listener_Installer;
		private System.ServiceProcess.ServiceInstaller SqlQueueService_Listener_svcInstaller;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ProjectInstaller()
		{
			// This call is required by the Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}


		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SqlQueueService_Listener_Installer = new System.ServiceProcess.ServiceProcessInstaller();
			this.SqlQueueService_Listener_svcInstaller = new System.ServiceProcess.ServiceInstaller();

			this.SqlQueueService_Listener_Installer.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
			this.SqlQueueService_Listener_svcInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;

			// 
			// MSMQService_Listener_Installer
			// 
			this.SqlQueueService_Listener_Installer.Password = null;
			this.SqlQueueService_Listener_Installer.Username = null;
			// 
			// MSMQService_Listener_svcInstaller
			// 
			this.SqlQueueService_Listener_svcInstaller.ServiceName = "StarChef.MSMQService";
			this.SqlQueueService_Listener_svcInstaller.DisplayName = "StarChef MSMQ Service";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
																					  this.SqlQueueService_Listener_Installer,
																					  this.SqlQueueService_Listener_svcInstaller});

		}

        public override void Install(IDictionary stateSaver)
        {
            RetrieveServiceName();
            base.Install(stateSaver);
        }

        public override void Uninstall(IDictionary savedState)
        {
            RetrieveServiceName();
            base.Uninstall(savedState);
        }


        private void RetrieveServiceName()
        {
            var key = "serviceName";
            if (Context.Parameters.ContainsKey(key))
            {
                var serviceName = Context.Parameters[key];
                if (!string.IsNullOrEmpty(serviceName))
                {
                    this.SqlQueueService_Listener_svcInstaller.ServiceName = serviceName;
                    this.SqlQueueService_Listener_svcInstaller.DisplayName = serviceName;
                }
            }
        }

        #endregion
    }
}
