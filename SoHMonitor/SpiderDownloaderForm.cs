using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Office.Interop.Excel;

namespace ShysterWatch
{
    public partial class SpiderDownloaderForm : Form
    {
        public SpiderDownloaderForm()
        {
            InitializeComponent();
            synchronizationContext = SynchronizationContext.Current;
        }


        void ShowBigSites()
        {

            var wsd = new WebSiteDownloader();
            wsd.MessageSender = SendMessage;


            var heavySites = from site in Sys.CurrentGroup.WebsiteDb.Websites
                             where site.WebPages.Count() > 100
                             orderby site.WebPages.Count() descending
                             select site;

            string s = "";
            foreach (var site in heavySites)
            {
                s += site.WebPages.Count() + " : " + site.SourceUrl + "\r\n";

                foreach (var p in site.WebPages.Take(50))
                {
                    s += "   " + p.Url + "\r\n";
                }
            }

            textBoxProgressReport.Text = s;

            return;
        }








        void GeneralDebug()
        {



            foreach(var ws in Sys.CurrentGroup.WebsiteDb.Websites)
            {
                foreach (var wp in ws.WebPages)
                {
                    if(wp.PageVersions.Count > 1)
                    {
                        var latestVersion = wp.PageVersions.Last();
                        var previousVersion = wp.PageVersions[wp.PageVersions.Count - 2];


                        if((previousVersion.WebPageChangedFromThisVersion == null) && (latestVersion.WebPageChangedFromThisVersion != null))
                        {
                            previousVersion.WebPageChangedFromThisVersion = latestVersion.WebPageChangedFromThisVersion;
                            latestVersion.WebPageChangedFromThisVersion = null;
                        }

                    }
                }
                textBoxProgressReport.Text += "done " + ws.SourceUrl;

            }

            Sys.CurrentGroup.WebsiteDb.Save();

            return;







#pragma warning disable CS0162 // Unreachable code detected
            var dSite = Sys.CurrentGroup.WebsiteDb.Websites.Find(x => x.SourceUrl.ToLower().IndexOf("www.healinghomeopaths.com") != -1);
#pragma warning restore CS0162 // Unreachable code detected
            var pageVersions = from page in dSite.WebPages
                        where page.PageVersions.Count > 0
                        select page.PageVersions.Last();

            foreach(var v in pageVersions.OrderBy(x => x.Sentences().Length))
            {
                textBoxProgressReport.Text += v.Sentences().Length + " - " + v.WebPage.Url + "\r\n";

                foreach(var sent in v.Sentences())
                {
                    textBoxProgressReport.Text += "      " + sent + "\r\n";
                }

                var copy = v.SentencesAreSharedWith();
                if(copy != null)
                {
                    textBoxProgressReport.Text += "     ** shared:" + v.WebPage.Url + "\r\n";
                }

            }

            return;


            //{
            //    var ws = WebsiteDb.Db.Websites.OrderByDescending(x => x.WebPages.Count).Take(5);

            //    foreach (var w in ws)
            //    {
            //        textBoxProgressReport.Text += "" + w.SourceUrl + "\r\n";

            //        for (int i = w.WebPages.Count - 1; i > w.WebPages.Count - 20; i--)
            //        {
            //            var p = w.WebPages[i];

            //            textBoxProgressReport.Text += "     -" + p.Url + "\r\n";
            //        }

            //    }
            //}










            ////Delete pages
            //{
            //    var urlStart = "https://www.deeatkinson.net/catalog";

            //    var dSite = WebsiteDb.Db.Websites.Find(x => x.SourceUrl.ToLower().IndexOf("www.deeatkinson.net") != -1);

            //    {
            //        var pages = dSite.WebPages.Where(x => x.Url.IndexOf(urlStart) == 0).Count();
            //        textBoxProgressReport.Text += pages + "\r\n";
            //    }



            //    var dpages = dSite.WebPages.Where(x => x.Url.IndexOf(urlStart) == 0).ToList();
            //    foreach (var dp in dpages)
            //    {

            //        dp.Delete();

            //    }

            //    {
            //        var pages = dSite.WebPages.Where(x => x.Url.IndexOf(urlStart) == 0).Count();
            //        textBoxProgressReport.Text += pages + " pages now\r\n";
            //    }
            //    WebsiteDb.Db.Save();
            //}



        }




        WebSiteDownloader WebsiteDownloader;
        private async void buttonRun_Click(object sender, EventArgs e)
        {
            buttonRun.Enabled = false;


            WebsiteDownloader = new WebSiteDownloader();
            WebsiteDownloader.MessageSender = SendMessage;

            await Task.Run(() => { _ = WebsiteDownloader.PermCycle(WebSiteDownloader.CycleType.InitialDownload); });

            await Task.Run(() => { _ = WebsiteDownloader.PermCycle(WebSiteDownloader.CycleType.RepeatDownload); });

            buttonRun.Enabled = true;

            return;


        }

        private readonly SynchronizationContext synchronizationContext;


        string MessagesToAdd = "";
        public void SendMessage(string message)
        {

            synchronizationContext.Post(new SendOrPostCallback(o =>
            {

                if (!checkBoxPauseOutput.Checked)
                {
                    if (textBoxProgressReport.Text.Length > 20000) textBoxProgressReport.Text = textBoxProgressReport.Text.Substring(5000);
                }

                MessagesToAdd += ("\r\n");
                MessagesToAdd += ((string)o);

                if (!checkBoxPauseOutput.Checked)
                {
                    textBoxProgressReport.AppendText(MessagesToAdd);
                    MessagesToAdd = "";
                    textBoxProgressReport.SelectionStart = textBoxProgressReport.Text.Length;
                    textBoxProgressReport.ScrollToCaret();
                }


            }), message);


        }

        private void buttonGeneralDebug_Click(object sender, EventArgs e)
        {
            GeneralDebug();
        }

        private async void buttonReSpider_Click(object sender, EventArgs e)
        {
            var wsd = new WebSiteDownloader();
            wsd.MessageSender = SendMessage;

            await wsd.PermCycle(WebSiteDownloader.CycleType.RepeatDownload);
            return;
        }

        private void buttonScrapeDataSource_Click(object sender, EventArgs e)
        {
            //Sys.CurrentGroup.Scraper.Run(this.SendMessage);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text += " - " + Sys.CurrentGroup.FolderName;

            foreach (var process in Sys.CurrentGroup.Scraper.ScrapingProcessFunctions)
            {
                checkedListBoxProcessesToRun.Items.Add(process);
            }
        }

        private async void buttonRunProcesses_Click(object sender, EventArgs e)
        {

            buttonRunProcesses.Enabled = false;

            Sys.CurrentGroup.Scraper.MessageSender = this.SendMessage;

            DateTime ProcessListStartTime = DateTime.Now;

            foreach(ScrapingProcessFunction item in checkedListBoxProcessesToRun.CheckedItems)
            {


                SendMessage("#########################################################");
                DateTime ProcessStartTime = DateTime.Now;
                SendMessage("Starting process: " + item.Explanation + " at " + ProcessStartTime.ToLongTimeString());

                Task task = item.Process();
                await task;

                SendMessage("Process complete: " + item.Explanation + " at " + DateTime.Now.ToLongTimeString());
                SendMessage("That took " + (DateTime.Now - ProcessStartTime).ToString());


                SendMessage("");
            }

            SendMessage("All processes complete at " + DateTime.Now.ToLongTimeString());
            SendMessage("That took " + (DateTime.Now - ProcessListStartTime).ToString());

            buttonRunProcesses.Enabled = true;

        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            if (WebsiteDownloader != null) WebsiteDownloader.Stop = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (WebsiteDownloader != null)
            {
                WebsiteDownloader.EndAtNextStop = true;
                WebsiteDownloader.Stop = true;
            }
        }
    }
}
