using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;

namespace StarChef.MSMQService
{
	/// <summary>
	/// Summary description for ProjectInstaller.
	/// </summary>
	[RunInstaller(true)]
	public class ProjectInstaller : System.Configuration.Install.Installer
	{
		private System.ServiceProcess.ServiceProcessInstaller MSMQService_Listener_Installer;
		private System.ServiceProcess.ServiceInstaller MSMQService_Listener_svcInstaller;
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
			this.MSMQService_Listener_Installer = new System.ServiceProcess.ServiceProcessInstaller();
			this.MSMQService_Listener_svcInstaller = new System.ServiceProcess.ServiceInstaller();

			this.MSMQService_Listener_Installer.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
			this.MSMQService_Listener_svcInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;

			// 
			// MSMQService_Listener_Installer
			// 
			this.MSMQService_Listener_Installer.Password = null;
			this.MSMQService_Listener_Installer.Username = null;
			// 
			// MSMQService_Listener_svcInstaller
			// 
			this.MSMQService_Listener_svcInstaller.ServiceName = "StarChef.MSMQService";
			this.MSMQService_Listener_svcInstaller.DisplayName = "StarChef MSMQ Service";
		    this.RetrieveServiceName();
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
																					  this.MSMQService_Listener_Installer,
																					  this.MSMQService_Listener_svcInstaller});

		}

        public override void Install(IDictionary stateSaver)
        {
            this.RetrieveServiceName();
            base.Install(stateSaver);
        }

        public override void Uninstall(IDictionary savedState)
        {
            this.RetrieveServiceName();
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
                    this.MSMQService_Listener_svcInstaller.ServiceName = serviceName;
                    this.MSMQService_Listener_svcInstaller.DisplayName = serviceName.Replace(".", "");
                }
            }
        }

        #endregion
    }
}
