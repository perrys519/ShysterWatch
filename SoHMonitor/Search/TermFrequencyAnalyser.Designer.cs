namespace ShysterWatch.Search
{
    partial class TermFrequencyAnalyser
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
            this.textBoxTerms = new System.Windows.Forms.TextBox();
            this.buttonGo = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textboxOutput = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBoxTerms
            // 
            this.textBoxTerms.Location = new System.Drawing.Point(13, 34);
            this.textBoxTerms.Multiline = true;
            this.textBoxTerms.Name = "textBoxTerms";
            this.textBoxTerms.Size = new System.Drawing.Size(514, 126);
            this.textBoxTerms.TabIndex = 0;
            // 
            // buttonGo
            // 
            this.buttonGo.Location = new System.Drawing.Point(15, 166);
            this.buttonGo.Name = "buttonGo";
            this.buttonGo.Size = new System.Drawing.Size(75, 23);
            this.buttonGo.TabIndex = 1;
            this.buttonGo.Text = "Analyse Terms";
            this.buttonGo.UseVisualStyleBackColor = true;
            this.buttonGo.Click += new System.EventHandler(this.buttonGo_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(456, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Terms should be placed one per line, with alternative spellings seperated by a pi" +
    "pe chanracter: |";
            // 
            // textboxOutput
            // 
            this.textboxOutput.Location = new System.Drawing.Point(15, 196);
            this.textboxOutput.Multiline = true;
            this.textboxOutput.Name = "textboxOutput";
            this.textboxOutput.Size = new System.Drawing.Size(512, 441);
            this.textboxOutput.TabIndex = 3;
            // 
            // TermFrequencyAnalyser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(539, 649);
            this.Controls.Add(this.textboxOutput);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonGo);
            this.Controls.Add(this.textBoxTerms);
            this.Name = "TermFrequencyAnalyser";
            this.Text = "TermFrequencyAnalyser";
            this.Load += new System.EventHandler(this.TermFrequencyAnalyser_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxTerms;
        private System.Windows.Forms.Button buttonGo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textboxOutput;
    }
}