using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShysterWatch.Search
{
    public partial class TermFrequencyAnalyser : Form
    {
        public TermFrequencyAnalyser()
        {
            InitializeComponent();
        }

        private async void buttonGo_Click(object sender, EventArgs e)
        {
            string s = textBoxTerms.Text;
            s = s.Replace("\r", "");
            var terms = s.Split('\n');

            foreach(var term in terms)
            {
                var searchtext = term.Replace("|", "\r\n");

                var search = new WebsiteSearch();
                

                SearchResults Results = await Task.Run(() =>
                {
                    var res = search.Run(searchtext, DateTime.Now);
                    _ = res.Websites;
                    return res;
                });




                textboxOutput.AppendText(term + "\t" + Results.Results.Count + "\t" + Results.Websites.Count() + "\r\n");

            }

        }

        private void TermFrequencyAnalyser_Load(object sender, EventArgs e)
        {
            textBoxTerms.AppendText(Properties.Settings.Default.SearchTerms);
        }



    }
}
