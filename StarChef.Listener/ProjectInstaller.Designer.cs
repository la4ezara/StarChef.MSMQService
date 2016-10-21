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

        private System.ServiceProcess.ServiceProcessInstaller listenerProcessInstaller;
        private System.ServiceProcess.ServiceInstaller listenerServiceInstaller;

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listenerProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.listenerServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // listenerProcessInstaller
            // 
            this.listenerProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.listenerProcessInstaller.Password = null;
            this.listenerProcessInstaller.Username = null;
            // 
            // listenerServiceInstaller
            // 
            this.listenerServiceInstaller.Description = "Service responsible for listening for arrival of messages";
            this.listenerServiceInstaller.DisplayName = "Starchef Message Listner";
            this.listenerServiceInstaller.ServiceName = "StarchefMessageListner";
            this.listenerServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            this.listenerServiceInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.listenerServiceInstaller_AfterInstall);
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.listenerProcessInstaller,
            this.listenerServiceInstaller});

        }

        #endregion
    }
}