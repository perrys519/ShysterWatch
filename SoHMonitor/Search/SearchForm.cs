using ShysterWatch.Search;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace ShysterWatch
{
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public partial class SearchForm : Form
    {
        public SearchForm()
        {
            InitializeComponent();
        }




        void UpdateSavedSearches()
        {
            comboBoxSavedSearches.Items.Clear();

            comboBoxSavedSearches.Items.Add("Saved searches....");

            foreach (var savedSearch in Sys.CurrentGroup.SearchHistory["Website"].SavedSearches)
            {
                comboBoxSavedSearches.Items.Add(savedSearch.SearchText.Replace("\n", " / "));
            }

            comboBoxSavedSearches.SelectedIndex = 0;
        }

        private void SearchForm_Load(object sender, EventArgs e)
        {

            this.Text += " - " + Sys.CurrentGroup.FolderName;

            UpdateSavedSearches();
            webBrowserResults.ObjectForScripting = this;
            dateTimePickerEffectiveDate.Value = DateTime.Now;

            textBoxSearchText.Text = ShysterWatch.Properties.Settings.Default.Search_LastTerm;

            //initialise the dictionaries for speed later
            Sys.CurrentGroup.WebsiteDb.WebsiteFindByUniqueId("");
            Sys.CurrentGroup.WebsiteDb.WebPageFindByUniqueIds("", "");

            foreach(var category in Sys.CurrentGroup.MembershipDb.GetCatgeories())
            {
                listBoxCategories.Items.Add(category);
            }
        }

        const int PageSize = 5;

        Search.SearchResults Results;


        public bool CheckboxClick(string webSiteUniqueId, string webPageUniqueId, int versionIndex, string WhichButton, bool isChecked)
        {
            if (!isChecked) WhichButton = "None";
            return RadioButtonClick(webSiteUniqueId, webPageUniqueId, versionIndex, WhichButton);
        }

        /// <summary>
        /// Handles the clicks from the radio buttons in the search results.
        /// </summary>
        public bool RadioButtonClick(string webSiteUniqueId, string webPageUniqueId, int versionIndex, string WhichButton)
        {

            if (SavedSearch == null)
            {
                MessageBox.Show("You can only make these modifications for saved searches.\r\nSave your search and try again?");
                return false;
            }


            var augType = SoHMonitor.Search.SearchResultAugmentationType.None;
            if (WhichButton == "IgnoreForever") augType = SoHMonitor.Search.SearchResultAugmentationType.IgnoreForever;
            if (WhichButton == "IgnoreThisVersion") augType = SoHMonitor.Search.SearchResultAugmentationType.IgnoreThisVersion;
            if (WhichButton == "Concerning") augType = SoHMonitor.Search.SearchResultAugmentationType.Concerning;

            SavedSearch.SetAugmentation(webSiteUniqueId, webPageUniqueId, versionIndex, augType);

            Sys.CurrentGroup.SearchHistory.Save();

            return true;
        }


        SoHMonitor.Search.SavedSearch SavedSearch
        {
            get
            {
                return Sys.CurrentGroup.SearchHistory["Website"].SavedSearches.Find(x => WebsiteDb.FixLineBreaks(x.SearchText) == textBoxSearchText.Text);
            }
        }

        void ShowNoResultsPage()
        {
            string s = $@"
<html>
    <head>
        <style type=""text/css"">
            *{{
                font-family: Tahoma;
                font-size: 8pt;
             }}
            h2{{
                font-size: 10pt;
                margin-bottom: 1mm;
            }}
        </style>
    </head>
    <body>Sorry, no results</body></html>
        ";
            webBrowserResults.DocumentText = s;
        }


        void ShowPage(int PageNumber)
        {
            var ResultsOnPage = Results.GroupByWebsite.Skip(PageNumber * PageSize).Take(PageSize);




            string s = $@"
<html>
    <head>
        <style type=""text/css"">
            *{{
                font-family: Tahoma;
                font-size: 8pt;
             }}
            h2{{
                font-size: 10pt;
                margin-bottom: 1mm;
            }}
            .highlight{{
                font-weight: bold;
                background-color: yellow;
            }}
            .PageId{{
                color: darkblue;
                text-decoration: underline;
                font-size: 10pt;
            }}


            .IgnoreForever{{
                background-color: lightgreen;
            }}
            .IgnoreThisVersion{{
                background-color: orange;
            }}
            .Concerning{{
                background-color: red;
            }}

        </style>
    </head>
    <body>
       {Results.GroupByWebsite.Count} results.
";


            foreach (var group in ResultsOnPage)
            {
                SoHMonitor.Search.SearchResultAugmentationType siteAugmentation = SoHMonitor.Search.SearchResultAugmentationType.None;
                if (SavedSearch != null)
                    siteAugmentation = SavedSearch.GetAugmentationType(group.Website.UniqueId);

                s += $@"
        <div>
            <h2>{HttpUtility.HtmlEncode(group.Website.SourceUrl)} <input {(siteAugmentation == SoHMonitor.Search.SearchResultAugmentationType.IgnoreForever ? "checked" : "")} onclick=""return window.external.CheckboxClick('{group.Website.UniqueId}', '', 0, 'IgnoreForever', this.checked);"" name=""|0"" type=checkbox value='IgnoreForever' title='Ignore forever' class='IgnoreForever' /></h2>";

                foreach (var r in group.Results)
                {
                    SoHMonitor.Search.SearchResultAugmentationType augmentation = SoHMonitor.Search.SearchResultAugmentationType.None;
                    if (SavedSearch != null)
                        augmentation = SavedSearch.GetAugmentationType(r.WebSiteUniqueId, r.WebPageUniqueId, r.WebPageVersion.Index);


                    s += $@"<div>
            [<a title=""{HttpUtility.HtmlAttributeEncode(r.WebPageVersion.VersionDateSummary())}"" href=""javascript:a'file://{HttpUtility.HtmlAttributeEncode(r.CachedHtmlFile.FullName)}"">cached v{r.WebPageVersion.Index}</a>] ";


                    if (r.WebPage != null) s += $@"
            [<a href=""javascript:a'{HttpUtility.HtmlAttributeEncode(r.WebPage.Url)}"">live</a>]
";
                    //if (savedSearch != null)
                    {
                        s += $@"
            <input {(augmentation == SoHMonitor.Search.SearchResultAugmentationType.IgnoreForever ? "checked" : "")} onclick=""return window.external.RadioButtonClick('{r.WebSiteUniqueId}', '{r.WebPageUniqueId}', {r.WebPageVersion.Index}, 'IgnoreForever');"" name=""{r.WebPageUniqueId}|{r.WebPageVersion.Index}"" type=radio value='IgnoreForever' title='Ignore forever' class='IgnoreForever' />
            <input {(augmentation == SoHMonitor.Search.SearchResultAugmentationType.IgnoreThisVersion ? "checked" : "")} onclick=""return window.external.RadioButtonClick('{r.WebSiteUniqueId}', '{r.WebPageUniqueId}', {r.WebPageVersion.Index}, 'IgnoreThisVersion');"" name=""{r.WebPageUniqueId}|{r.WebPageVersion.Index}"" type=radio value='IgnoreThisVersion' title='Ignore this version' class='IgnoreThisVersion' />
            <input {(augmentation == SoHMonitor.Search.SearchResultAugmentationType.Concerning ? "checked" : "")} onclick=""return window.external.RadioButtonClick('{r.WebSiteUniqueId}', '{r.WebPageUniqueId}', {r.WebPageVersion.Index}, 'Concerning');"" name=""{r.WebPageUniqueId}|{r.WebPageVersion.Index}"" type=radio value='Concerning' title='Flag as concerning content' class='Concerning' />
";
                    }
                    s += $@"
            <span class=PageId>{HttpUtility.HtmlEncode(r.WebPageUniqueId)} </span>
                    </div>
                    <div>
";

                    foreach (var sentence in r.RelevantSentences)
                    {
                        var highlight = HttpUtility.HtmlEncode(sentence);

                        foreach (var term in r.Searchresults.WebsiteSearch.SearchTerms)
                        {
                            var re = new Regex(Regex.Escape(term), RegexOptions.IgnoreCase);
                            highlight = re.Replace(highlight, "<span class=highlight>$&</span>");
                        }

                        s += $@"
                        <div>.....&quot;{highlight}&quot;</div>
";
                    }


                    s += $@"
                    </div>";

                }


                s += @"

        </div>
";
            }

            s += @"
    </body>
</html>
";

            webBrowserResults.DocumentText = s;

        }




        private void ButtonRunSearch_Click(object sender, EventArgs e)
        {
            ShysterWatch.Properties.Settings.Default.Search_LastTerm = textBoxSearchText.Text;
            ShysterWatch.Properties.Settings.Default.Save();

            var Search = new Search.WebsiteSearch();

            // textBoxResults.Text = Search.Run();

            Results = Search.Run(textBoxSearchText.Text, dateTimePickerEffectiveDate.Value);
            Results.SurpressResults = checkBoxHideIgnoredResults.Checked;

            if (listBoxCategories.SelectedItems.Count > 0)
            {
                Results.LimitToCategories = new List<string>();
                foreach(string cat in listBoxCategories.CheckedItems)
                {
                    Results.LimitToCategories.Add(cat);
                }
            }

            if (checkBoxExport.Checked) Results.SaveToExcelFile();

            int Pages = (int)Math.Ceiling((float)Results.GroupByWebsite.Count / (float)PageSize);


            comboBoxPageSelect.Items.Clear();
            for(int i=1; i<=Pages; i++)
            {
                comboBoxPageSelect.Items.Add(i);
            }

            if(comboBoxPageSelect.Items.Count > 0)
            {
                if (comboBoxPageSelect.SelectedIndex == 0) ShowPage(comboBoxPageSelect.SelectedIndex);
                else comboBoxPageSelect.SelectedIndex = 0;
            }
            if(Results.Results.Count == 0)
            {
                ShowNoResultsPage();
            }


        }

        private void WebBrowserResults_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            //cancel the current event

            if (e.Url.ToString() == "about:blank") return;

            e.Cancel = true;

            var path = e.Url.ToString().Substring("javascript:a'".Length);


            //this opens the URL in the user's default browser
            Process.Start(path);
        }

        private void ComboBoxPageSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowPage(comboBoxPageSelect.SelectedIndex);
        }

        private void ButtonNextPage_Click(object sender, EventArgs e)
        {
            if (comboBoxPageSelect.SelectedIndex + 1 < comboBoxPageSelect.Items.Count) comboBoxPageSelect.SelectedIndex++;
        }

        private void ButtonPreviousPage_Click(object sender, EventArgs e)
        {
            if (comboBoxPageSelect.SelectedIndex - 1>=0) comboBoxPageSelect.SelectedIndex--;

        }

        private void ButtonSaveSearch_Click(object sender, EventArgs e)
        {
            if (SavedSearch != null)
            {
                return;
            }

            var search = new SoHMonitor.Search.SavedSearch
            {
                SearchText = textBoxSearchText.Text
            };
            Sys.CurrentGroup.SearchHistory["Website"].SavedSearches.Add(search);
            UpdateSavedSearches();

            Sys.CurrentGroup.SearchHistory.Save();

        }

        private void ComboBoxSavedSearches_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxSavedSearches.SelectedIndex == 0) return;
            
            textBoxSearchText.Text = WebsiteDb.FixLineBreaks(Sys.CurrentGroup.SearchHistory["Website"].SavedSearches[comboBoxSavedSearches.SelectedIndex-1].SearchText);

            comboBoxSavedSearches.SelectedIndex = 0;
        }

        private void ContextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void DownloaderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                Application.Run(new SpiderDownloaderForm());
            });
        }


        void OpenSettingsForm()
        {
            Application.Run(new Settings());
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            Thread t = new Thread(OpenSettingsForm);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();


        }


        void OpenTermFrequencyAnalyserForm()
        {
            Application.Run(new TermFrequencyAnalyser());
        }
        private void TermAnalyserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(OpenTermFrequencyAnalyserForm);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        void OpenComplaintGeneratorForm()
        {
            Application.Run(new FalseClaimFinderForm());
        }

        private void AutoComplaintGeneratorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(OpenComplaintGeneratorForm);
            t.SetApartmentState(ApartmentState.STA) ;
            t.Start();
        }
    }
}
