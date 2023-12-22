namespace ShysterWatch
{
    partial class SpiderDownloaderForm
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
            this.textBoxProgressReport = new System.Windows.Forms.TextBox();
            this.buttonRun = new System.Windows.Forms.Button();
            this.buttonGeneralDebug = new System.Windows.Forms.Button();
            this.buttonScrapeDataSource = new System.Windows.Forms.Button();
            this.checkedListBoxProcessesToRun = new System.Windows.Forms.CheckedListBox();
            this.buttonRunProcesses = new System.Windows.Forms.Button();
            this.checkBoxPauseOutput = new System.Windows.Forms.CheckBox();
            this.buttonStop = new System.Windows.Forms.Button();
            this.buttonSaveAndStop = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxProgressReport
            // 
            this.textBoxProgressReport.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxProgressReport.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxProgressReport.ForeColor = System.Drawing.SystemColors.WindowText;
            this.textBoxProgressReport.Location = new System.Drawing.Point(306, 42);
            this.textBoxProgressReport.Multiline = true;
            this.textBoxProgressReport.Name = "textBoxProgressReport";
            this.textBoxProgressReport.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxProgressReport.Size = new System.Drawing.Size(939, 709);
            this.textBoxProgressReport.TabIndex = 0;
            // 
            // buttonRun
            // 
            this.buttonRun.Location = new System.Drawing.Point(306, 12);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(99, 23);
            this.buttonRun.TabIndex = 1;
            this.buttonRun.Text = "Run Spider";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // buttonGeneralDebug
            // 
            this.buttonGeneralDebug.Location = new System.Drawing.Point(1121, 13);
            this.buttonGeneralDebug.Name = "buttonGeneralDebug";
            this.buttonGeneralDebug.Size = new System.Drawing.Size(116, 23);
            this.buttonGeneralDebug.TabIndex = 2;
            this.buttonGeneralDebug.Text = "General Debug";
            this.buttonGeneralDebug.UseVisualStyleBackColor = true;
            this.buttonGeneralDebug.Click += new System.EventHandler(this.buttonGeneralDebug_Click);
            // 
            // buttonScrapeDataSource
            // 
            this.buttonScrapeDataSource.Location = new System.Drawing.Point(906, 5);
            this.buttonScrapeDataSource.Name = "buttonScrapeDataSource";
            this.buttonScrapeDataSource.Size = new System.Drawing.Size(75, 23);
            this.buttonScrapeDataSource.TabIndex = 4;
            this.buttonScrapeDataSource.Text = "Scrape Data Source";
            this.buttonScrapeDataSource.UseVisualStyleBackColor = true;
            this.buttonScrapeDataSource.Click += new System.EventHandler(this.buttonScrapeDataSource_Click);
            // 
            // checkedListBoxProcessesToRun
            // 
            this.checkedListBoxProcessesToRun.CheckOnClick = true;
            this.checkedListBoxProcessesToRun.FormattingEnabled = true;
            this.checkedListBoxProcessesToRun.Location = new System.Drawing.Point(13, 42);
            this.checkedListBoxProcessesToRun.Name = "checkedListBoxProcessesToRun";
            this.checkedListBoxProcessesToRun.Size = new System.Drawing.Size(285, 709);
            this.checkedListBoxProcessesToRun.TabIndex = 5;
            // 
            // buttonRunProcesses
            // 
            this.buttonRunProcesses.Location = new System.Drawing.Point(13, 13);
            this.buttonRunProcesses.Name = "buttonRunProcesses";
            this.buttonRunProcesses.Size = new System.Drawing.Size(75, 23);
            this.buttonRunProcesses.TabIndex = 6;
            this.buttonRunProcesses.Text = "Run";
            this.buttonRunProcesses.UseVisualStyleBackColor = true;
            this.buttonRunProcesses.Click += new System.EventHandler(this.buttonRunProcesses_Click);
            // 
            // checkBoxPauseOutput
            // 
            this.checkBoxPauseOutput.AutoSize = true;
            this.checkBoxPauseOutput.Location = new System.Drawing.Point(740, 13);
            this.checkBoxPauseOutput.Name = "checkBoxPauseOutput";
            this.checkBoxPauseOutput.Size = new System.Drawing.Size(91, 17);
            this.checkBoxPauseOutput.TabIndex = 7;
            this.checkBoxPauseOutput.Text = "Pause Output";
            this.checkBoxPauseOutput.UseVisualStyleBackColor = true;
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(411, 12);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(107, 23);
            this.buttonStop.TabIndex = 8;
            this.buttonStop.Text = "Pause to Save & Carry On";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // buttonSaveAndStop
            // 
            this.buttonSaveAndStop.Location = new System.Drawing.Point(524, 12);
            this.buttonSaveAndStop.Name = "buttonSaveAndStop";
            this.buttonSaveAndStop.Size = new System.Drawing.Size(75, 23);
            this.buttonSaveAndStop.TabIndex = 9;
            this.buttonSaveAndStop.Text = "Save & Stop";
            this.buttonSaveAndStop.UseVisualStyleBackColor = true;
            this.buttonSaveAndStop.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1257, 760);
            this.Controls.Add(this.buttonSaveAndStop);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.checkBoxPauseOutput);
            this.Controls.Add(this.buttonRunProcesses);
            this.Controls.Add(this.checkedListBoxProcessesToRun);
            this.Controls.Add(this.buttonScrapeDataSource);
            this.Controls.Add(this.buttonGeneralDebug);
            this.Controls.Add(this.buttonRun);
            this.Controls.Add(this.textBoxProgressReport);
            this.Name = "Form1";
            this.Text = "ShysterWatch Scraper & Web Site Spider";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxProgressReport;
        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.Button buttonGeneralDebug;
        private System.Windows.Forms.Button buttonScrapeDataSource;
        private System.Windows.Forms.CheckedListBox checkedListBoxProcessesToRun;
        private System.Windows.Forms.Button buttonRunProcesses;
        private System.Windows.Forms.CheckBox checkBoxPauseOutput;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Button buttonSaveAndStop;
    }
}

