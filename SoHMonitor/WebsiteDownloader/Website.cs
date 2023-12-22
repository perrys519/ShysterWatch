using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace ShysterWatch
{
    public class Website
    {
        /// <summary>
        /// Most likely the home page - the source Url from which we find the whole site.
        /// </summary>
        public string SourceUrl;


        /// <summary>
        /// If it's worth continuing to check a website for claims, or if the website appears irrelevant
        /// based upon previous pages.
        /// </summary>
        /// <returns></returns>
        public bool ContinueCheckingForClaims()
        {
            int totalPagesCount = 0;
            int checkedPagesCount = 0;
            int irrelevantPagesCount = 0;

            foreach (var page in this.WebPages)
            {
                if (page.PageVersions.Count > 0)
                {
                    totalPagesCount++;

                    var claimResult = page.PageVersions[0].GetAiClaimsResult(2);
                    if (claimResult != null)
                    {

                        checkedPagesCount++;

                        if (claimResult.Reason == "Text Irrelevant") irrelevantPagesCount++;
                    }


                }
            }

            if ((checkedPagesCount >= 5) && (irrelevantPagesCount == checkedPagesCount)) return false;
            //if ((checkedPagesCount >= 20) && ((checkedPagesCount - irrelevantPagesCount) < 3)) return false;

            return true;


        }



        public string SimplifiedUrl
        {
            get
            {
                return WebPage.SimplifyUrl(SourceUrl);
            }
        }

        /// <summary>
        /// The folder name for the website
        /// </summary>
        private string uniqueId = "";
        public string UniqueId
        {
            get
            {
                if(uniqueId == "")
                {
                    if (SourceUrl == "") throw new Exception("Cannot create UniqueId until SourceUrl set");
                    string url = SourceUrl;
                    url = url.Replace("http://", "");
                    url = url.Replace("https://", "");

                    Regex filenameClean = new Regex(@"[^a-zA-Z0-9\-\.]+");

                    url = filenameClean.Replace(url, "-");
                    url = url.Replace("--", "-");

                    if (url.Length > 40) url = url.Substring(0, 40);

                    string testUrl = url;
                    int count = 0;

                    while (WebsiteDb.Websites.Where(x => x != this).Where(x => x.uniqueId == testUrl).FirstOrDefault() != null)
                    {
                        count++;
                        testUrl = url + count;
                    }
                    uniqueId = testUrl;
                }
                return uniqueId;
            }
            set
            {
                uniqueId = value;
            }
        }

        public List<WebPage> WebPages = new List<WebPage>();


        public IEnumerable<WebPage> WebPagesInStudy()
        {
            var relevantPages = WebPages.Take(30).Where(x => x.PageVersions.Count() > 0).Where(x => x.PageVersions.First().MalwareFound == false);
            return relevantPages;
        }


        [XmlIgnore]
        public WebsiteDb WebsiteDb;


        public void AddWebPage(string link, string UniqueIdOfPageFoundOn)
        {

            if(this.WebPages.Count() >= Properties.Settings.Default.WebsitePageLimit)
            {
                return;
            }


            if (WebSiteDownloader.ExtensionIsBlacklisted(link)) return;
            if (link.IndexOf("https://www.deeatkinson.net/catalog") == 0) return;
            if (link.IndexOf("https://www.thenaturalhealthhub.co.uk/tag/") == 0) return;

            if (link.IndexOf("https://www.ecnt.co.uk/customer/account/login/referer/") != -1) return;

            //if the URL is too long
            if (link.Length > 250) return;



            //don't add anything with a query string
            if (link.IndexOf("?") != -1) return;

            var simpleLink = WebPage.SimplifyUrl(link);


            //check if the page already exists
            if (WebPages.Where(x => x.SimplifiedUrl == simpleLink).FirstOrDefault() == null)
            {
                WebPage page = new WebPage();
                page.Website = this;
                page.Url = link;
                page.FoundOnPage = UniqueIdOfPageFoundOn;
                WebPages.Add(page);


            }

        }
    }


}
