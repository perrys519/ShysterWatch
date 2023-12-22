namespace ShysterWatch
{
    partial class FalseClaimFinderForm
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
            this.btnProcessClaimsWithAi = new System.Windows.Forms.Button();
            this.debugOutput = new System.Windows.Forms.TextBox();
            this.btnCreateReport = new System.Windows.Forms.Button();
            this.btnCreateSample = new System.Windows.Forms.Button();
            this.btnExportData = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnProcessClaimsWithAi
            // 
            this.btnProcessClaimsWithAi.Location = new System.Drawing.Point(12, 25);
            this.btnProcessClaimsWithAi.Name = "btnProcessClaimsWithAi";
            this.btnProcessClaimsWithAi.Size = new System.Drawing.Size(173, 23);
            this.btnProcessClaimsWithAi.TabIndex = 0;
            this.btnProcessClaimsWithAi.Text = "Process with AI ($)";
            this.btnProcessClaimsWithAi.UseVisualStyleBackColor = true;
            this.btnProcessClaimsWithAi.Click += new System.EventHandler(this.BtnProcessClaimsWithAi_Click);
            // 
            // debugOutput
            // 
            this.debugOutput.Location = new System.Drawing.Point(13, 55);
            this.debugOutput.Multiline = true;
            this.debugOutput.Name = "debugOutput";
            this.debugOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.debugOutput.Size = new System.Drawing.Size(486, 330);
            this.debugOutput.TabIndex = 1;
            // 
            // btnCreateReport
            // 
            this.btnCreateReport.Location = new System.Drawing.Point(191, 25);
            this.btnCreateReport.Name = "btnCreateReport";
            this.btnCreateReport.Size = new System.Drawing.Size(129, 23);
            this.btnCreateReport.TabIndex = 2;
            this.btnCreateReport.Text = "Create Stats Report";
            this.btnCreateReport.UseVisualStyleBackColor = true;
            this.btnCreateReport.Click += new System.EventHandler(this.BtnCreateReport_Click);
            // 
            // btnCreateSample
            // 
            this.btnCreateSample.Location = new System.Drawing.Point(13, 391);
            this.btnCreateSample.Name = "btnCreateSample";
            this.btnCreateSample.Size = new System.Drawing.Size(199, 23);
            this.btnCreateSample.TabIndex = 3;
            this.btnCreateSample.Text = "Create Sample For Expert Review";
            this.btnCreateSample.UseVisualStyleBackColor = true;
            this.btnCreateSample.Click += new System.EventHandler(this.BtnCreateSample_Click);
            // 
            // btnExportData
            // 
            this.btnExportData.Location = new System.Drawing.Point(326, 25);
            this.btnExportData.Name = "btnExportData";
            this.btnExportData.Size = new System.Drawing.Size(173, 23);
            this.btnExportData.TabIndex = 4;
            this.btnExportData.Text = "Export Claims to JSON/HTML";
            this.btnExportData.UseVisualStyleBackColor = true;
            this.btnExportData.Click += new System.EventHandler(this.BtnExportData_Click);
            // 
            // FalseClaimFinderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(520, 439);
            this.Controls.Add(this.btnExportData);
            this.Controls.Add(this.btnCreateSample);
            this.Controls.Add(this.btnCreateReport);
            this.Controls.Add(this.debugOutput);
            this.Controls.Add(this.btnProcessClaimsWithAi);
            this.Name = "FalseClaimFinderForm";
            this.Text = "ShysterWatch - False Claims Finder";
            this.Load += new System.EventHandler(this.ComplaintGenerator_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnProcessClaimsWithAi;
        private System.Windows.Forms.TextBox debugOutput;
        private System.Windows.Forms.Button btnCreateReport;
        private System.Windows.Forms.Button btnCreateSample;
        private System.Windows.Forms.Button btnExportData;
    }
}