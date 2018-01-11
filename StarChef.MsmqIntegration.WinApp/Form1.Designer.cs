namespace StarChef.MsmqIntegration.WinApp
{
    partial class Form1
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtCount = new System.Windows.Forms.TextBox();
            this.lblCount = new System.Windows.Forms.Label();
            this.cbMessageType = new System.Windows.Forms.ComboBox();
            this.lblMessageType = new System.Windows.Forms.Label();
            this.txtProductId = new System.Windows.Forms.TextBox();
            this.lblProductId = new System.Windows.Forms.Label();
            this.lblProductType = new System.Windows.Forms.Label();
            this.cbProductType = new System.Windows.Forms.ComboBox();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.lblQueueCount = new System.Windows.Forms.Label();
            this.lblQueueCountValue = new System.Windows.Forms.Label();
            this.lblProcessMessageCount = new System.Windows.Forms.Label();
            this.txtStatus = new System.Windows.Forms.TextBox();
            this.btnStartMonitoring = new System.Windows.Forms.Button();
            this.btnStopMonitoring = new System.Windows.Forms.Button();
            this.btnClearQueue = new System.Windows.Forms.Button();
            this.lblDatabaseId = new System.Windows.Forms.Label();
            this.txtDatabaseId = new System.Windows.Forms.TextBox();
            this.lblDSN = new System.Windows.Forms.Label();
            this.txtDSN = new System.Windows.Forms.TextBox();
            this.btnClearConsole = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtCount
            // 
            this.txtCount.Location = new System.Drawing.Point(102, 16);
            this.txtCount.Name = "txtCount";
            this.txtCount.Size = new System.Drawing.Size(121, 20);
            this.txtCount.TabIndex = 0;
            this.txtCount.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtCount_KeyPress);
            // 
            // lblCount
            // 
            this.lblCount.AutoSize = true;
            this.lblCount.Location = new System.Drawing.Point(12, 19);
            this.lblCount.Name = "lblCount";
            this.lblCount.Size = new System.Drawing.Size(84, 13);
            this.lblCount.TabIndex = 1;
            this.lblCount.Text = "Message Count:";
            // 
            // cbMessageType
            // 
            this.cbMessageType.FormattingEnabled = true;
            this.cbMessageType.Location = new System.Drawing.Point(319, 16);
            this.cbMessageType.Name = "cbMessageType";
            this.cbMessageType.Size = new System.Drawing.Size(121, 21);
            this.cbMessageType.TabIndex = 2;
            // 
            // lblMessageType
            // 
            this.lblMessageType.AutoSize = true;
            this.lblMessageType.Location = new System.Drawing.Point(229, 19);
            this.lblMessageType.Name = "lblMessageType";
            this.lblMessageType.Size = new System.Drawing.Size(80, 13);
            this.lblMessageType.TabIndex = 3;
            this.lblMessageType.Text = "Message Type:";
            // 
            // txtProductId
            // 
            this.txtProductId.Location = new System.Drawing.Point(102, 42);
            this.txtProductId.Name = "txtProductId";
            this.txtProductId.Size = new System.Drawing.Size(121, 20);
            this.txtProductId.TabIndex = 4;
            this.txtProductId.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtCount_KeyPress);
            // 
            // lblProductId
            // 
            this.lblProductId.AutoSize = true;
            this.lblProductId.Location = new System.Drawing.Point(12, 45);
            this.lblProductId.Name = "lblProductId";
            this.lblProductId.Size = new System.Drawing.Size(61, 13);
            this.lblProductId.TabIndex = 5;
            this.lblProductId.Text = "Product ID:";
            // 
            // lblProductType
            // 
            this.lblProductType.AutoSize = true;
            this.lblProductType.Location = new System.Drawing.Point(229, 44);
            this.lblProductType.Name = "lblProductType";
            this.lblProductType.Size = new System.Drawing.Size(74, 13);
            this.lblProductType.TabIndex = 6;
            this.lblProductType.Text = "Product Type:";
            // 
            // cbProductType
            // 
            this.cbProductType.FormattingEnabled = true;
            this.cbProductType.Location = new System.Drawing.Point(319, 41);
            this.cbProductType.Name = "cbProductType";
            this.cbProductType.Size = new System.Drawing.Size(121, 21);
            this.cbProductType.TabIndex = 7;
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(102, 210);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(338, 35);
            this.btnGenerate.TabIndex = 8;
            this.btnGenerate.Text = "Generate Messages";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // lblQueueCount
            // 
            this.lblQueueCount.AutoSize = true;
            this.lblQueueCount.Location = new System.Drawing.Point(475, 19);
            this.lblQueueCount.Name = "lblQueueCount";
            this.lblQueueCount.Size = new System.Drawing.Size(73, 13);
            this.lblQueueCount.TabIndex = 9;
            this.lblQueueCount.Text = "Queue Count:";
            // 
            // lblQueueCountValue
            // 
            this.lblQueueCountValue.AutoSize = true;
            this.lblQueueCountValue.Location = new System.Drawing.Point(554, 19);
            this.lblQueueCountValue.Name = "lblQueueCountValue";
            this.lblQueueCountValue.Size = new System.Drawing.Size(107, 13);
            this.lblQueueCountValue.TabIndex = 10;
            this.lblQueueCountValue.Text = "Current Queue Count";
            // 
            // lblProcessMessageCount
            // 
            this.lblProcessMessageCount.AutoSize = true;
            this.lblProcessMessageCount.Location = new System.Drawing.Point(472, 52);
            this.lblProcessMessageCount.Name = "lblProcessMessageCount";
            this.lblProcessMessageCount.Size = new System.Drawing.Size(167, 13);
            this.lblProcessMessageCount.TabIndex = 11;
            this.lblProcessMessageCount.Text = "Process Message per 10 seconds";
            // 
            // txtStatus
            // 
            this.txtStatus.Location = new System.Drawing.Point(475, 68);
            this.txtStatus.Multiline = true;
            this.txtStatus.Name = "txtStatus";
            this.txtStatus.ReadOnly = true;
            this.txtStatus.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtStatus.Size = new System.Drawing.Size(281, 136);
            this.txtStatus.TabIndex = 12;
            // 
            // btnStartMonitoring
            // 
            this.btnStartMonitoring.Location = new System.Drawing.Point(475, 210);
            this.btnStartMonitoring.Name = "btnStartMonitoring";
            this.btnStartMonitoring.Size = new System.Drawing.Size(136, 35);
            this.btnStartMonitoring.TabIndex = 13;
            this.btnStartMonitoring.Text = "Start monitoring";
            this.btnStartMonitoring.UseVisualStyleBackColor = true;
            this.btnStartMonitoring.Click += new System.EventHandler(this.btnStartMonitoring_Click);
            // 
            // btnStopMonitoring
            // 
            this.btnStopMonitoring.Location = new System.Drawing.Point(617, 210);
            this.btnStopMonitoring.Name = "btnStopMonitoring";
            this.btnStopMonitoring.Size = new System.Drawing.Size(139, 35);
            this.btnStopMonitoring.TabIndex = 13;
            this.btnStopMonitoring.Text = "Stop monitoring";
            this.btnStopMonitoring.UseVisualStyleBackColor = true;
            this.btnStopMonitoring.Click += new System.EventHandler(this.btnStopMonitoring_Click);
            // 
            // btnClearQueue
            // 
            this.btnClearQueue.Location = new System.Drawing.Point(667, 14);
            this.btnClearQueue.Name = "btnClearQueue";
            this.btnClearQueue.Size = new System.Drawing.Size(89, 23);
            this.btnClearQueue.TabIndex = 14;
            this.btnClearQueue.Text = "Clear Queue";
            this.btnClearQueue.UseVisualStyleBackColor = true;
            this.btnClearQueue.Click += new System.EventHandler(this.btnClearQueue_Click);
            // 
            // lblDatabaseId
            // 
            this.lblDatabaseId.AutoSize = true;
            this.lblDatabaseId.Location = new System.Drawing.Point(12, 71);
            this.lblDatabaseId.Name = "lblDatabaseId";
            this.lblDatabaseId.Size = new System.Drawing.Size(70, 13);
            this.lblDatabaseId.TabIndex = 6;
            this.lblDatabaseId.Text = "Database ID:";
            // 
            // txtDatabaseId
            // 
            this.txtDatabaseId.Location = new System.Drawing.Point(102, 68);
            this.txtDatabaseId.Name = "txtDatabaseId";
            this.txtDatabaseId.Size = new System.Drawing.Size(121, 20);
            this.txtDatabaseId.TabIndex = 4;
            this.txtDatabaseId.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtCount_KeyPress);
            // 
            // lblDSN
            // 
            this.lblDSN.AutoSize = true;
            this.lblDSN.Location = new System.Drawing.Point(12, 97);
            this.lblDSN.Name = "lblDSN";
            this.lblDSN.Size = new System.Drawing.Size(33, 13);
            this.lblDSN.TabIndex = 6;
            this.lblDSN.Text = "DSN:";
            // 
            // txtDSN
            // 
            this.txtDSN.Location = new System.Drawing.Point(102, 94);
            this.txtDSN.Multiline = true;
            this.txtDSN.Name = "txtDSN";
            this.txtDSN.Size = new System.Drawing.Size(338, 110);
            this.txtDSN.TabIndex = 4;
            // 
            // btnClearConsole
            // 
            this.btnClearConsole.Location = new System.Drawing.Point(667, 40);
            this.btnClearConsole.Name = "btnClearConsole";
            this.btnClearConsole.Size = new System.Drawing.Size(89, 23);
            this.btnClearConsole.TabIndex = 14;
            this.btnClearConsole.Text = "Clear Console";
            this.btnClearConsole.UseVisualStyleBackColor = true;
            this.btnClearConsole.Click += new System.EventHandler(this.btnClearConsole_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(785, 271);
            this.Controls.Add(this.btnClearConsole);
            this.Controls.Add(this.btnClearQueue);
            this.Controls.Add(this.btnStopMonitoring);
            this.Controls.Add(this.btnStartMonitoring);
            this.Controls.Add(this.txtStatus);
            this.Controls.Add(this.lblProcessMessageCount);
            this.Controls.Add(this.lblQueueCountValue);
            this.Controls.Add(this.lblQueueCount);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.cbProductType);
            this.Controls.Add(this.lblDSN);
            this.Controls.Add(this.lblDatabaseId);
            this.Controls.Add(this.lblProductType);
            this.Controls.Add(this.lblProductId);
            this.Controls.Add(this.txtDatabaseId);
            this.Controls.Add(this.txtDSN);
            this.Controls.Add(this.txtProductId);
            this.Controls.Add(this.lblMessageType);
            this.Controls.Add(this.cbMessageType);
            this.Controls.Add(this.lblCount);
            this.Controls.Add(this.txtCount);
            this.Name = "Form1";
            this.Text = "MSMQ Generator and Monitoring Tool";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtCount;
        private System.Windows.Forms.Label lblCount;
        private System.Windows.Forms.ComboBox cbMessageType;
        private System.Windows.Forms.Label lblMessageType;
        private System.Windows.Forms.TextBox txtProductId;
        private System.Windows.Forms.Label lblProductId;
        private System.Windows.Forms.Label lblProductType;
        private System.Windows.Forms.ComboBox cbProductType;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Label lblQueueCount;
        private System.Windows.Forms.Label lblQueueCountValue;
        private System.Windows.Forms.Label lblProcessMessageCount;
        private System.Windows.Forms.TextBox txtStatus;
        private System.Windows.Forms.Button btnStartMonitoring;
        private System.Windows.Forms.Button btnStopMonitoring;
        private System.Windows.Forms.Button btnClearQueue;
        private System.Windows.Forms.Label lblDatabaseId;
        private System.Windows.Forms.TextBox txtDatabaseId;
        private System.Windows.Forms.Label lblDSN;
        private System.Windows.Forms.TextBox txtDSN;
        private System.Windows.Forms.Button btnClearConsole;
    }
}

