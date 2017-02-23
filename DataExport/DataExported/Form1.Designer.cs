namespace DataExported
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.ddlCustomers = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnRetrieve = new System.Windows.Forms.Button();
            this.txtConnectionString = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.bntExport = new System.Windows.Forms.Button();
            this.dgEntityInfo = new System.Windows.Forms.DataGridView();
            this.btnRetrieveExportDetails = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.txtSkip = new System.Windows.Forms.TextBox();
            this.txtTake = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgEntityInfo)).BeginInit();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnRetrieveExportDetails);
            this.panel1.Controls.Add(this.ddlCustomers);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.btnRetrieve);
            this.panel1.Controls.Add(this.txtConnectionString);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(938, 136);
            this.panel1.TabIndex = 0;
            // 
            // ddlCustomers
            // 
            this.ddlCustomers.FormattingEnabled = true;
            this.ddlCustomers.Location = new System.Drawing.Point(155, 67);
            this.ddlCustomers.Name = "ddlCustomers";
            this.ddlCustomers.Size = new System.Drawing.Size(121, 21);
            this.ddlCustomers.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Customers:";
            // 
            // btnRetrieve
            // 
            this.btnRetrieve.Location = new System.Drawing.Point(155, 30);
            this.btnRetrieve.Name = "btnRetrieve";
            this.btnRetrieve.Size = new System.Drawing.Size(177, 31);
            this.btnRetrieve.TabIndex = 6;
            this.btnRetrieve.Text = "Populate Customers";
            this.btnRetrieve.UseVisualStyleBackColor = true;
            // 
            // txtConnectionString
            // 
            this.txtConnectionString.Location = new System.Drawing.Point(155, 6);
            this.txtConnectionString.Name = "txtConnectionString";
            this.txtConnectionString.Size = new System.Drawing.Size(569, 20);
            this.txtConnectionString.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(136, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Login DB Connection string";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.panel4);
            this.panel2.Controls.Add(this.panel5);
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 136);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(938, 593);
            this.panel2.TabIndex = 7;
            // 
            // bntExport
            // 
            this.bntExport.Location = new System.Drawing.Point(369, 6);
            this.bntExport.Name = "bntExport";
            this.bntExport.Size = new System.Drawing.Size(177, 31);
            this.bntExport.TabIndex = 1;
            this.bntExport.Text = "Export";
            this.bntExport.UseVisualStyleBackColor = true;
            this.bntExport.Click += new System.EventHandler(this.bntExport_Click_1);
            // 
            // dgEntityInfo
            // 
            this.dgEntityInfo.AllowUserToAddRows = false;
            this.dgEntityInfo.AllowUserToDeleteRows = false;
            this.dgEntityInfo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgEntityInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgEntityInfo.Location = new System.Drawing.Point(0, 0);
            this.dgEntityInfo.Name = "dgEntityInfo";
            this.dgEntityInfo.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgEntityInfo.Size = new System.Drawing.Size(938, 357);
            this.dgEntityInfo.TabIndex = 0;
            // 
            // btnRetrieveExportDetails
            // 
            this.btnRetrieveExportDetails.Location = new System.Drawing.Point(155, 94);
            this.btnRetrieveExportDetails.Name = "btnRetrieveExportDetails";
            this.btnRetrieveExportDetails.Size = new System.Drawing.Size(177, 31);
            this.btnRetrieveExportDetails.TabIndex = 9;
            this.btnRetrieveExportDetails.Text = "Retrieve Export Details";
            this.btnRetrieveExportDetails.UseVisualStyleBackColor = true;
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(0, 0);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(938, 172);
            this.txtLog.TabIndex = 2;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.txtLog);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(0, 421);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(938, 172);
            this.panel3.TabIndex = 3;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.dgEntityInfo);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(938, 357);
            this.panel4.TabIndex = 4;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.txtTake);
            this.panel5.Controls.Add(this.label4);
            this.panel5.Controls.Add(this.txtSkip);
            this.panel5.Controls.Add(this.label3);
            this.panel5.Controls.Add(this.bntExport);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel5.Location = new System.Drawing.Point(0, 357);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(938, 64);
            this.panel5.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Skip:";
            // 
            // txtSkip
            // 
            this.txtSkip.Location = new System.Drawing.Point(59, 6);
            this.txtSkip.Name = "txtSkip";
            this.txtSkip.Size = new System.Drawing.Size(129, 20);
            this.txtSkip.TabIndex = 9;
            // 
            // txtTake
            // 
            this.txtTake.Location = new System.Drawing.Point(59, 32);
            this.txtTake.Name = "txtTake";
            this.txtTake.Size = new System.Drawing.Size(129, 20);
            this.txtTake.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 32);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Take:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(938, 729);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgEntityInfo)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox txtConnectionString;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnRetrieve;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.DataGridView dgEntityInfo;
        private System.Windows.Forms.Button bntExport;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox ddlCustomers;
        private System.Windows.Forms.Button btnRetrieveExportDetails;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TextBox txtTake;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtSkip;
        private System.Windows.Forms.Label label3;
    }
}

