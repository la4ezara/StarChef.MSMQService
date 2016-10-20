namespace StarChef.Listener
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller1;
        private System.ServiceProcess.ServiceInstaller schedulerServiceInstaller;

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            serviceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
            schedulerServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // serviceProcessInstaller1
            // 
            serviceProcessInstaller1.Password = null;
            serviceProcessInstaller1.Username = null;
            serviceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            serviceProcessInstaller1.AfterInstall += new System.Configuration.Install.InstallEventHandler(serviceProcessInstaller1_AfterInstall);
            // 
            // SchedulerServiceInstaller
            // 
            schedulerServiceInstaller.Description = "Service responsible for listening for arrival of messages";
            schedulerServiceInstaller.DisplayName = "Starchef Message Listner";
            schedulerServiceInstaller.ServiceName = "StarchefMessageListner";
            schedulerServiceInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(SchedulerServiceInstaller_AfterInstall);
            // 
            // ProjectInstaller
            // 
            Installers.AddRange(new System.Configuration.Install.Installer[] {
            serviceProcessInstaller1,
            schedulerServiceInstaller});
        }

        #endregion
    }
}