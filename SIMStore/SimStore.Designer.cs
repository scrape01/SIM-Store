namespace SIMStore
{
    partial class SimStore
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
            this.cmbCardReaders = new System.Windows.Forms.ComboBox();
            this.lblReader = new System.Windows.Forms.Label();
            this.btnUploadFileToSIM = new System.Windows.Forms.Button();
            this.btnDownloadFileFromSIM = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cmbCardReaders
            // 
            this.cmbCardReaders.FormattingEnabled = true;
            this.cmbCardReaders.Location = new System.Drawing.Point(179, 17);
            this.cmbCardReaders.Name = "cmbCardReaders";
            this.cmbCardReaders.Size = new System.Drawing.Size(458, 28);
            this.cmbCardReaders.TabIndex = 0;
            this.cmbCardReaders.SelectedIndexChanged += new System.EventHandler(this.CmbCardReaders_SelectedIndexChanged);
            // 
            // lblReader
            // 
            this.lblReader.AutoSize = true;
            this.lblReader.Location = new System.Drawing.Point(12, 20);
            this.lblReader.Name = "lblReader";
            this.lblReader.Size = new System.Drawing.Size(100, 20);
            this.lblReader.TabIndex = 1;
            this.lblReader.Text = "Card Reader";
            // 
            // btnUploadFileToSIM
            // 
            this.btnUploadFileToSIM.Location = new System.Drawing.Point(179, 127);
            this.btnUploadFileToSIM.Name = "btnUploadFileToSIM";
            this.btnUploadFileToSIM.Size = new System.Drawing.Size(226, 56);
            this.btnUploadFileToSIM.TabIndex = 2;
            this.btnUploadFileToSIM.Text = "Upload File to SIM";
            this.btnUploadFileToSIM.UseVisualStyleBackColor = true;
            this.btnUploadFileToSIM.Click += new System.EventHandler(this.BtnUploadFileToSIM_Click);
            // 
            // btnDownloadFileFromSIM
            // 
            this.btnDownloadFileFromSIM.Location = new System.Drawing.Point(411, 127);
            this.btnDownloadFileFromSIM.Name = "btnDownloadFileFromSIM";
            this.btnDownloadFileFromSIM.Size = new System.Drawing.Size(226, 56);
            this.btnDownloadFileFromSIM.TabIndex = 3;
            this.btnDownloadFileFromSIM.Text = "Download File from SIM";
            this.btnDownloadFileFromSIM.UseVisualStyleBackColor = true;
            this.btnDownloadFileFromSIM.Click += new System.EventHandler(this.BtnDownloadFileFromSIM_Click);
            // 
            // SimStore
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(661, 209);
            this.Controls.Add(this.btnDownloadFileFromSIM);
            this.Controls.Add(this.btnUploadFileToSIM);
            this.Controls.Add(this.lblReader);
            this.Controls.Add(this.cmbCardReaders);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SimStore";
            this.Text = "SIM Store";
            this.Load += new System.EventHandler(this.SimStore_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbCardReaders;
        private System.Windows.Forms.Label lblReader;
        private System.Windows.Forms.Button btnUploadFileToSIM;
        private System.Windows.Forms.Button btnDownloadFileFromSIM;
    }
}