namespace ShysterWatch
{
    partial class SearchForm
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
            this.components = new System.ComponentModel.Container();
            this.buttonRunSearch = new System.Windows.Forms.Button();
            this.textBoxSearchText = new System.Windows.Forms.TextBox();
            this.webBrowserResults = new System.Windows.Forms.WebBrowser();
            this.comboBoxPageSelect = new System.Windows.Forms.ComboBox();
            this.buttonNextPage = new System.Windows.Forms.Button();
            this.buttonPreviousPage = new System.Windows.Forms.Button();
            this.checkBoxExport = new System.Windows.Forms.CheckBox();
            this.comboBoxSavedSearches = new System.Windows.Forms.ComboBox();
            this.buttonSaveSearch = new System.Windows.Forms.Button();
            this.checkBoxHideIgnoredResults = new System.Windows.Forms.CheckBox();
            this.dateTimePickerEffectiveDate = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.downloaderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.termAnalyserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autoComplaintGeneratorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.listBoxCategories = new System.Windows.Forms.CheckedListBox();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonRunSearch
            // 
            this.buttonRunSearch.Location = new System.Drawing.Point(329, 90);
            this.buttonRunSearch.Name = "buttonRunSearch";
            this.buttonRunSearch.Size = new System.Drawing.Size(75, 23);
            this.buttonRunSearch.TabIndex = 0;
            this.buttonRunSearch.Text = "Search";
            this.buttonRunSearch.UseVisualStyleBackColor = true;
            this.buttonRunSearch.Click += new System.EventHandler(this.ButtonRunSearch_Click);
            // 
            // textBoxSearchText
            // 
            this.textBoxSearchText.Location = new System.Drawing.Point(12, 37);
            this.textBoxSearchText.Multiline = true;
            this.textBoxSearchText.Name = "textBoxSearchText";
            this.textBoxSearchText.Size = new System.Drawing.Size(311, 76);
            this.textBoxSearchText.TabIndex = 1;
            this.textBoxSearchText.Text = "CEASE";
            // 
            // webBrowserResults
            // 
            this.webBrowserResults.Location = new System.Drawing.Point(13, 119);
            this.webBrowserResults.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowserResults.Name = "webBrowserResults";
            this.webBrowserResults.Size = new System.Drawing.Size(531, 493);
            this.webBrowserResults.TabIndex = 3;
            this.webBrowserResults.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.WebBrowserResults_Navigating);
            // 
            // comboBoxPageSelect
            // 
            this.comboBoxPageSelect.FormattingEnabled = true;
            this.comboBoxPageSelect.Location = new System.Drawing.Point(472, 92);
            this.comboBoxPageSelect.Name = "comboBoxPageSelect";
            this.comboBoxPageSelect.Size = new System.Drawing.Size(47, 21);
            this.comboBoxPageSelect.TabIndex = 5;
            this.comboBoxPageSelect.SelectedIndexChanged += new System.EventHandler(this.ComboBoxPageSelect_SelectedIndexChanged);
            // 
            // buttonNextPage
            // 
            this.buttonNextPage.Location = new System.Drawing.Point(518, 91);
            this.buttonNextPage.Name = "buttonNextPage";
            this.buttonNextPage.Size = new System.Drawing.Size(19, 23);
            this.buttonNextPage.TabIndex = 6;
            this.buttonNextPage.Text = ">";
            this.buttonNextPage.UseVisualStyleBackColor = true;
            this.buttonNextPage.Click += new System.EventHandler(this.ButtonNextPage_Click);
            // 
            // buttonPreviousPage
            // 
            this.buttonPreviousPage.Location = new System.Drawing.Point(456, 91);
            this.buttonPreviousPage.Name = "buttonPreviousPage";
            this.buttonPreviousPage.Size = new System.Drawing.Size(17, 23);
            this.buttonPreviousPage.TabIndex = 7;
            this.buttonPreviousPage.Text = "<";
            this.buttonPreviousPage.UseVisualStyleBackColor = true;
            this.buttonPreviousPage.Click += new System.EventHandler(this.ButtonPreviousPage_Click);
            // 
            // checkBoxExport
            // 
            this.checkBoxExport.AutoSize = true;
            this.checkBoxExport.Location = new System.Drawing.Point(329, 59);
            this.checkBoxExport.Name = "checkBoxExport";
            this.checkBoxExport.Size = new System.Drawing.Size(82, 17);
            this.checkBoxExport.TabIndex = 8;
            this.checkBoxExport.Text = "Export Data";
            this.checkBoxExport.UseVisualStyleBackColor = true;
            // 
            // comboBoxSavedSearches
            // 
            this.comboBoxSavedSearches.FormattingEnabled = true;
            this.comboBoxSavedSearches.Location = new System.Drawing.Point(13, 11);
            this.comboBoxSavedSearches.Name = "comboBoxSavedSearches";
            this.comboBoxSavedSearches.Size = new System.Drawing.Size(116, 21);
            this.comboBoxSavedSearches.TabIndex = 9;
            this.comboBoxSavedSearches.SelectedIndexChanged += new System.EventHandler(this.ComboBoxSavedSearches_SelectedIndexChanged);
            // 
            // buttonSaveSearch
            // 
            this.buttonSaveSearch.Location = new System.Drawing.Point(128, 9);
            this.buttonSaveSearch.Name = "buttonSaveSearch";
            this.buttonSaveSearch.Size = new System.Drawing.Size(79, 23);
            this.buttonSaveSearch.TabIndex = 10;
            this.buttonSaveSearch.Text = "Save Search";
            this.buttonSaveSearch.UseVisualStyleBackColor = true;
            this.buttonSaveSearch.Click += new System.EventHandler(this.ButtonSaveSearch_Click);
            // 
            // checkBoxHideIgnoredResults
            // 
            this.checkBoxHideIgnoredResults.AutoSize = true;
            this.checkBoxHideIgnoredResults.Checked = true;
            this.checkBoxHideIgnoredResults.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxHideIgnoredResults.Location = new System.Drawing.Point(329, 36);
            this.checkBoxHideIgnoredResults.Name = "checkBoxHideIgnoredResults";
            this.checkBoxHideIgnoredResults.Size = new System.Drawing.Size(87, 17);
            this.checkBoxHideIgnoredResults.TabIndex = 12;
            this.checkBoxHideIgnoredResults.Text = "Hide Ignored";
            this.checkBoxHideIgnoredResults.UseVisualStyleBackColor = true;
            // 
            // dateTimePickerEffectiveDate
            // 
            this.dateTimePickerEffectiveDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePickerEffectiveDate.Location = new System.Drawing.Point(213, 13);
            this.dateTimePickerEffectiveDate.MinDate = new System.DateTime(2020, 3, 26, 0, 0, 0, 0);
            this.dateTimePickerEffectiveDate.Name = "dateTimePickerEffectiveDate";
            this.dateTimePickerEffectiveDate.Size = new System.Drawing.Size(108, 20);
            this.dateTimePickerEffectiveDate.TabIndex = 13;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(210, 1);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "Effective date:";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.termAnalyserToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.downloaderToolStripMenuItem,
            this.autoComplaintGeneratorToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(181, 114);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStrip1_Opening);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.SettingsToolStripMenuItem_Click);
            // 
            // downloaderToolStripMenuItem
            // 
            this.downloaderToolStripMenuItem.Name = "downloaderToolStripMenuItem";
            this.downloaderToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.downloaderToolStripMenuItem.Text = "Downloader";
            this.downloaderToolStripMenuItem.Click += new System.EventHandler(this.DownloaderToolStripMenuItem_Click);
            // 
            // termAnalyserToolStripMenuItem
            // 
            this.termAnalyserToolStripMenuItem.Name = "termAnalyserToolStripMenuItem";
            this.termAnalyserToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.termAnalyserToolStripMenuItem.Text = "Term Analyser";
            this.termAnalyserToolStripMenuItem.Click += new System.EventHandler(this.TermAnalyserToolStripMenuItem_Click);
            // 
            // autoComplaintGeneratorToolStripMenuItem
            // 
            this.autoComplaintGeneratorToolStripMenuItem.Name = "autoComplaintGeneratorToolStripMenuItem";
            this.autoComplaintGeneratorToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.autoComplaintGeneratorToolStripMenuItem.Text = "False Claims Finder";
            this.autoComplaintGeneratorToolStripMenuItem.Click += new System.EventHandler(this.AutoComplaintGeneratorToolStripMenuItem_Click);
            // 
            // listBoxCategories
            // 
            this.listBoxCategories.CheckOnClick = true;
            this.listBoxCategories.FormattingEnabled = true;
            this.listBoxCategories.Location = new System.Drawing.Point(417, 9);
            this.listBoxCategories.Name = "listBoxCategories";
            this.listBoxCategories.Size = new System.Drawing.Size(127, 79);
            this.listBoxCategories.TabIndex = 15;
            // 
            // SearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(556, 624);
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Controls.Add(this.listBoxCategories);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dateTimePickerEffectiveDate);
            this.Controls.Add(this.checkBoxHideIgnoredResults);
            this.Controls.Add(this.buttonSaveSearch);
            this.Controls.Add(this.comboBoxSavedSearches);
            this.Controls.Add(this.checkBoxExport);
            this.Controls.Add(this.buttonPreviousPage);
            this.Controls.Add(this.buttonNextPage);
            this.Controls.Add(this.comboBoxPageSelect);
            this.Controls.Add(this.webBrowserResults);
            this.Controls.Add(this.textBoxSearchText);
            this.Controls.Add(this.buttonRunSearch);
            this.Name = "SearchForm";
            this.Text = "ShysterWatch Search";
            this.Load += new System.EventHandler(this.SearchForm_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonRunSearch;
        private System.Windows.Forms.TextBox textBoxSearchText;
        private System.Windows.Forms.WebBrowser webBrowserResults;
        private System.Windows.Forms.ComboBox comboBoxPageSelect;
        private System.Windows.Forms.Button buttonNextPage;
        private System.Windows.Forms.Button buttonPreviousPage;
        private System.Windows.Forms.CheckBox checkBoxExport;
        private System.Windows.Forms.ComboBox comboBoxSavedSearches;
        private System.Windows.Forms.Button buttonSaveSearch;
        private System.Windows.Forms.CheckBox checkBoxHideIgnoredResults;
        private System.Windows.Forms.DateTimePicker dateTimePickerEffectiveDate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem downloaderToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem termAnalyserToolStripMenuItem;
        private System.Windows.Forms.CheckedListBox listBoxCategories;
        private System.Windows.Forms.ToolStripMenuItem autoComplaintGeneratorToolStripMenuItem;
    }
}