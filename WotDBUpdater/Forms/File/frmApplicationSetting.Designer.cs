﻿namespace WotDBUpdater
{
    partial class frmApplicationSetting
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
            this.btnOpenDossierFile = new System.Windows.Forms.Button();
            this.lblDossierFIle = new System.Windows.Forms.Label();
            this.txtDossierFilePath = new System.Windows.Forms.TextBox();
            this.openFileDialogDossierFile = new System.Windows.Forms.OpenFileDialog();
            this.btnSave = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtPlayerName = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOpenDossierFile
            // 
            this.btnOpenDossierFile.Location = new System.Drawing.Point(331, 127);
            this.btnOpenDossierFile.Name = "btnOpenDossierFile";
            this.btnOpenDossierFile.Size = new System.Drawing.Size(110, 25);
            this.btnOpenDossierFile.TabIndex = 5;
            this.btnOpenDossierFile.Text = "Select dossier file";
            this.btnOpenDossierFile.UseVisualStyleBackColor = true;
            this.btnOpenDossierFile.Click += new System.EventHandler(this.btnOpenDossierFile_Click);
            // 
            // lblDossierFIle
            // 
            this.lblDossierFIle.AutoSize = true;
            this.lblDossierFIle.Location = new System.Drawing.Point(15, 69);
            this.lblDossierFIle.Name = "lblDossierFIle";
            this.lblDossierFIle.Size = new System.Drawing.Size(69, 13);
            this.lblDossierFIle.TabIndex = 4;
            this.lblDossierFIle.Text = "Dossier path:";
            // 
            // txtDossierFilePath
            // 
            this.txtDossierFilePath.Location = new System.Drawing.Point(18, 85);
            this.txtDossierFilePath.Multiline = true;
            this.txtDossierFilePath.Name = "txtDossierFilePath";
            this.txtDossierFilePath.Size = new System.Drawing.Size(423, 36);
            this.txtDossierFilePath.TabIndex = 6;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(401, 187);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(67, 25);
            this.btnSave.TabIndex = 7;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Username:";
            // 
            // txtPlayerName
            // 
            this.txtPlayerName.Location = new System.Drawing.Point(18, 36);
            this.txtPlayerName.Name = "txtPlayerName";
            this.txtPlayerName.Size = new System.Drawing.Size(171, 20);
            this.txtPlayerName.TabIndex = 9;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtPlayerName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.txtDossierFilePath);
            this.groupBox1.Controls.Add(this.btnOpenDossierFile);
            this.groupBox1.Controls.Add(this.lblDossierFIle);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(456, 164);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Settings";
            // 
            // frmApplicationSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(487, 224);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmApplicationSetting";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Application settings";
            this.Load += new System.EventHandler(this.frmDossierFileSelect_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnOpenDossierFile;
        private System.Windows.Forms.Label lblDossierFIle;
        private System.Windows.Forms.OpenFileDialog openFileDialogDossierFile;
        private System.Windows.Forms.TextBox txtDossierFilePath;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtPlayerName;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}