using ShysterWatch.ComplaintGenerator;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShysterWatch
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void TextBoxRootFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = textBoxRootFolder.Text;
            var result = folderBrowserDialog1.ShowDialog();

            if(result == DialogResult.OK)
            {
                textBoxRootFolder.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            textBoxRootFolder.Text = Properties.Settings.Default.RootFolder;
            numericPageLimit.Value = Properties.Settings.Default.WebsitePageLimit;
            textBoxOpenAIApiKey.Text = Properties.Settings.Default.OpenAIApiKey;
            dateTimePickerRelevantDateForChoosingVersion.Value = Properties.Settings.Default.RelevantDateForChoosingVersion;

            foreach (var ps in PromptSpecification.PromptSpecifications.Values)
            {
                comboBoxPromptSpecification.Items.Add(ps);
                if (ps.PromptReferenceNumber == Properties.Settings.Default.AiPromptSpecificationReference) comboBoxPromptSpecification.SelectedIndex = comboBoxPromptSpecification.Items.Count - 1;
            }

            foreach (var grp in ShysterWatch.Sys.Groups)
            {
                comboBoxDataSet.Items.Add(grp);
                if (grp.FolderName == Properties.Settings.Default.SelectedDataSource) comboBoxDataSet.SelectedItem = grp ;
            }
        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {

            Properties.Settings.Default.RootFolder = textBoxRootFolder.Text;
            Properties.Settings.Default.WebsitePageLimit = (int)numericPageLimit.Value;
            Properties.Settings.Default.SelectedDataSource = ((ShysterGroup)comboBoxDataSet.SelectedItem).FolderName;
            Properties.Settings.Default.OpenAIApiKey = textBoxOpenAIApiKey.Text;
            Properties.Settings.Default.RelevantDateForChoosingVersion = dateTimePickerRelevantDateForChoosingVersion.Value;

            if (comboBoxPromptSpecification.SelectedItem is PromptSpecification specification)
            {
                Properties.Settings.Default.AiPromptSpecificationReference = specification.PromptReferenceNumber;
            }
            Properties.Settings.Default.Save();
            Close();
        }



    }
}
