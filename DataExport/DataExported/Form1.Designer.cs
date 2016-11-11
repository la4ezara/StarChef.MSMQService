﻿namespace DataExported
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
            this.btnRetrieve = new System.Windows.Forms.Button();
            this.txtConnectionString = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.bntExport = new System.Windows.Forms.Button();
            this.dgEntityInfo = new System.Windows.Forms.DataGridView();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDatabaseId = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgEntityInfo)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.txtDatabaseId);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.btnRetrieve);
            this.panel1.Controls.Add(this.txtConnectionString);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(733, 64);
            this.panel1.TabIndex = 0;
            // 
            // btnRetrieve
            // 
            this.btnRetrieve.Location = new System.Drawing.Point(630, 27);
            this.btnRetrieve.Name = "btnRetrieve";
            this.btnRetrieve.Size = new System.Drawing.Size(94, 31);
            this.btnRetrieve.TabIndex = 6;
            this.btnRetrieve.Text = "Retrieve Data";
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
            this.label1.Size = new System.Drawing.Size(137, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Database connectionstring:";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.bntExport);
            this.panel2.Controls.Add(this.dgEntityInfo);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 64);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(733, 515);
            this.panel2.TabIndex = 7;
            // 
            // bntExport
            // 
            this.bntExport.Location = new System.Drawing.Point(30, 261);
            this.bntExport.Name = "bntExport";
            this.bntExport.Size = new System.Drawing.Size(75, 23);
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
            this.dgEntityInfo.Location = new System.Drawing.Point(12, 16);
            this.dgEntityInfo.Name = "dgEntityInfo";
            this.dgEntityInfo.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgEntityInfo.Size = new System.Drawing.Size(688, 150);
            this.dgEntityInfo.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Database Id:";
            // 
            // txtDatabaseId
            // 
            this.txtDatabaseId.Location = new System.Drawing.Point(155, 41);
            this.txtDatabaseId.Name = "txtDatabaseId";
            this.txtDatabaseId.Size = new System.Drawing.Size(71, 20);
            this.txtDatabaseId.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(733, 579);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgEntityInfo)).EndInit();
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
        private System.Windows.Forms.TextBox txtDatabaseId;
        private System.Windows.Forms.Label label2;
    }
}

