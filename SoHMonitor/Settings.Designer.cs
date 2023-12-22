namespace ShysterWatch
{
    partial class Settings
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
            this.buttonSave = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.textBoxRootFolder = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.numericPageLimit = new System.Windows.Forms.NumericUpDown();
            this.comboBoxDataSet = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBoxOpenAIApiKey = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxPromptSpecification = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.dateTimePickerRelevantDateForChoosingVersion = new System.Windows.Forms.DateTimePicker();
            this.label6 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericPageLimit)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(12, 382);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 0;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.ButtonSave_Click);
            // 
            // textBoxRootFolder
            // 
            this.textBoxRootFolder.Location = new System.Drawing.Point(12, 28);
            this.textBoxRootFolder.Name = "textBoxRootFolder";
            this.textBoxRootFolder.Size = new System.Drawing.Size(266, 20);
            this.textBoxRootFolder.TabIndex = 1;
            this.textBoxRootFolder.Click += new System.EventHandler(this.TextBoxRootFolder_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Data Folder";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(178, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Download Limit (Pages per Website)";
            // 
            // numericPageLimit
            // 
            this.numericPageLimit.Location = new System.Drawing.Point(12, 86);
            this.numericPageLimit.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericPageLimit.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericPageLimit.Name = "numericPageLimit";
            this.numericPageLimit.Size = new System.Drawing.Size(91, 20);
            this.numericPageLimit.TabIndex = 5;
            this.numericPageLimit.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // comboBoxDataSet
            // 
            this.comboBoxDataSet.FormattingEnabled = true;
            this.comboBoxDataSet.Location = new System.Drawing.Point(12, 134);
            this.comboBoxDataSet.Name = "comboBoxDataSet";
            this.comboBoxDataSet.Size = new System.Drawing.Size(199, 21);
            this.comboBoxDataSet.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 118);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(49, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Data Set";
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.Control;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Location = new System.Drawing.Point(12, 340);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(239, 36);
            this.textBox1.TabIndex = 8;
            this.textBox1.Text = "After any settings change, all windows should be closed and the application resta" +
    "rted.";
            // 
            // textBoxOpenAIApiKey
            // 
            this.textBoxOpenAIApiKey.Location = new System.Drawing.Point(12, 191);
            this.textBoxOpenAIApiKey.Name = "textBoxOpenAIApiKey";
            this.textBoxOpenAIApiKey.Size = new System.Drawing.Size(266, 20);
            this.textBoxOpenAIApiKey.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 172);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(87, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Open AI API Key";
            // 
            // comboBoxPromptSpecification
            // 
            this.comboBoxPromptSpecification.FormattingEnabled = true;
            this.comboBoxPromptSpecification.Location = new System.Drawing.Point(12, 244);
            this.comboBoxPromptSpecification.Name = "comboBoxPromptSpecification";
            this.comboBoxPromptSpecification.Size = new System.Drawing.Size(263, 21);
            this.comboBoxPromptSpecification.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 228);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "AI Prompt";
            // 
            // dateTimePickerRelevantDateForChoosingVersion
            // 
            this.dateTimePickerRelevantDateForChoosingVersion.Location = new System.Drawing.Point(12, 299);
            this.dateTimePickerRelevantDateForChoosingVersion.Name = "dateTimePickerRelevantDateForChoosingVersion";
            this.dateTimePickerRelevantDateForChoosingVersion.Size = new System.Drawing.Size(200, 20);
            this.dateTimePickerRelevantDateForChoosingVersion.TabIndex = 13;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 283);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(187, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Date of Webpage Version to deal with";
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(360, 447);
            this.Controls.Add(this.dateTimePickerRelevantDateForChoosingVersion);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.comboBoxPromptSpecification);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxOpenAIApiKey);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBoxDataSet);
            this.Controls.Add(this.numericPageLimit);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxRootFolder);
            this.Controls.Add(this.buttonSave);
            this.Name = "Settings";
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.Settings_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericPageLimit)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.TextBox textBoxRootFolder;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericPageLimit;
        private System.Windows.Forms.ComboBox comboBoxDataSet;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBoxOpenAIApiKey;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBoxPromptSpecification;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DateTimePicker dateTimePickerRelevantDateForChoosingVersion;
        private System.Windows.Forms.Label label6;
    }
}