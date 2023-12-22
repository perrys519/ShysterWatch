using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SoHMonitor
{
    public class SohSiteScraper
    {

        string Foldername;

        string RootFolderPath
        {
            get
            {
                return Path.Combine(Properties.Settings.Default.DataFolder, "SohSiteScraper");
            }
        }
        string FolderForFilesAwaitingScrape
        {
            get
            {
                return Path.Combine(RootFolderPath, "HtmlFilesAwaitingScrape");
            }
        }
        string FolderForFilesAlreadyScraped
        {
            get
            {
                return Path.Combine(RootFolderPath, "HtmlFilesScraped");
            }
        }

        public SohSiteScraper()
        {
            Foldername = StartTime.ToString("yyyy-MM-dd HHmmss");
        }


        private const string DisableCachingName = @"TestSwitch.LocalAppContext.DisableCaching";
        private const string DontEnableSchUseStrongCryptoName = @"Switch.System.Net.DontEnableSchUseStrongCrypto";

        public void DownloadSearchResultsPage()
        {


            //AppContext.SetSwitch(DisableCachingName, true);
            // AppContext.SetSwitch(DontEnableSchUseStrongCryptoName, true);

            var s = new BrowserSession();

            //var h1 = s.CreateWebHit("https://homeopathy-soh.org/find-a-homeopath-by-location/");

            //h1.Call();

            var h = s.CreateWebHit("https://homeopathy-soh.org/find-a-homeopath-by-location/");
            h.PostParameters = new Dictionary<string, string>();
            h.PostParameters.Add("results_page_id", "5130");
            h.PostParameters.Add("units", "mi");
            h.PostParameters.Add("location_text", "L2");
            h.PostParameters.Add("object_name", "post");
            h.PostParameters.Add("map_post_type", "homeopath");
            h.PostParameters.Add("radius", "20000");
            h.PostParameters.Add("geo_mashup_search_submit", "Search");

            h.ChromeRequestHeaderDump = @":authority: homeopathy-soh.org
:method: POST
:path: /find-a-homeopath-by-location/
:scheme: https
accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9
accept-encoding: gzip, deflate, br
accept-language: fr-FR,fr;q=0.9,en-GB;q=0.8,en;q=0.7,en-US;q=0.6
cache-control: max-age=0
content-length: 129
content-type: application/x-www-form-urlencoded
cookie: wordpress_test_cookie=WP+Cookie+check; mailmunch_second_pageview=true; _mailmunch_visitor_id=c986f795-10c1-42ee-b6c9-760765c7c795; mailmunch-scrollbox-526445=true; __atuvc=20%7C12; __atuvs=5e75d006e6b59489003
dnt: 1
origin: https://homeopathy-soh.org
referer: https://homeopathy-soh.org/find-a-homeopath-by-location/
sec-fetch-dest: document
sec-fetch-mode: navigate
sec-fetch-site: same-origin
sec-fetch-user: ?1
upgrade-insecure-requests: 1
user-agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36";

            h.Call();


            //Delete all other files in the folder as they'll be out of date
            var dir = new DirectoryInfo(Path.Combine(RootFolderPath, "SearchResultsPagesAwaitingScrape"));
            foreach(var file in dir.GetFiles())
            {
                file.Delete();
            }

            string filename = DateTime.Now.ToString("yyyyMMddHHmmss") + ".html";
            File.WriteAllText(Path.Combine(RootFolderPath, "SearchResultsPagesAwaitingScrape", filename), h.html);



            //var db = new SohMemberDatabase();

            //var m = new SohMember();
            //m.Id = 92;
            //m.Postname = "test";

            //var md = new SohMemberDetailVersion();
            //md.Name = "jeff B";


            //m.DetailVersions.Add(md);

            //db.Members.Add(m);


            //db.Save();
        }

        DateTime StartTime;

        SohMemberDatabase db = null;
        SohMemberDatabase Db
        {
            get
            {
                if (db == null)
                {
                    db = SohMemberDatabase.Load();
                }
                return db;
            }
        }


        public void Run()
        {

            Debug.WriteLine(Db.MainClinics.Count());

            //DownloadSearchResultsPage();

            //UpdateMemberList();
            //DownloadSohMemberPages();
            //ProcessDownloadedSohMemberPages();
        }


        public void ProcessDownloadedSohMemberPage(string filename)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.Load(filename);

            var Mv = new SohMemberDetailVersion();

            var linkNode = doc.DocumentNode.SelectSingleNode("//link[@rel='canonical']");
            var url = linkNode.Attributes["href"].Value;

            Mv.Id = url.Replace("https://homeopathy-soh.org/homeopath/", "").Replace("/", "");
            
            var nameTitle = doc.DocumentNode.SelectSingleNode("//div[@id='inner-content']//h3");
            Mv.Name = nameTitle.InnerText;

            var addressInfoLis = doc.DocumentNode.SelectNodes("//div[@id='inner-content']//div[@class='columns small-12 medium-6']/ul/li");

            var qualifiedLi = addressInfoLis.Where(x => x.InnerHtml.IndexOf("<b>Qualified:</b> ") == 0).FirstOrDefault();
            if(qualifiedLi != null)
            {
                Mv.Qualified = qualifiedLi.InnerText.Replace("Qualified: ", "");
                
            }

            var emailLi = addressInfoLis.Where(x => x.InnerHtml.IndexOf("<a href='mailto:") == 0).FirstOrDefault();
            var websiteLi = addressInfoLis.Where(x => x.InnerHtml.IndexOf("<a href='http") == 0).FirstOrDefault();

            Regex rePhone = new Regex(@"[0-9 \+]{10,15}");
            var phoneNumberLi = addressInfoLis.Where(x => rePhone.Match(x.InnerText).Success).FirstOrDefault();

            var afterAddressLiIndex = addressInfoLis.Count();
            if (websiteLi != null)
            {
                afterAddressLiIndex = addressInfoLis.IndexOf(websiteLi);
                Mv.Website = websiteLi.InnerText;

            }
            if (phoneNumberLi != null)
            {
                afterAddressLiIndex = addressInfoLis.IndexOf(phoneNumberLi);
                Mv.PhoneNumber = phoneNumberLi.InnerText;
            }

            if (emailLi != null)
            {
                afterAddressLiIndex = addressInfoLis.IndexOf(emailLi);
                Mv.Email = emailLi.InnerText;
            }

            Mv.Address = "";
            for(int i=0; i<afterAddressLiIndex; i++)
            {
                if (Mv.Address != "") Mv.Address += "\r\n";
                Mv.Address += addressInfoLis[i].InnerText;

            }

            Mv.MainClinic = Mv.Id;

            var OtherClinicNodes = doc.DocumentNode.SelectNodes("//div[@id='inner-content']//div[@class='columns small-12 medium-6']/ul[2]/li/a");
            if (OtherClinicNodes != null)
            {

                foreach (var otherClinicNode in OtherClinicNodes)
                {
                    string ocUrl = otherClinicNode.Attributes["href"].Value;
                    string ocId = ocUrl.Replace("https://homeopathy-soh.org/homeopath/", "").Replace("/", "");

                    Mv.OtherClinicIds.Add(ocId);

                    if (ocId.Length < Mv.MainClinic.Length) Mv.MainClinic = ocId;

                }
            }




            FileInfo fileInfo = new FileInfo(filename);
            DateTime scrapeDate = fileInfo.LastWriteTime;
            Mv.ScrapedFromFilename = fileInfo.Name;








            var member = Db.Members.Where(x => x.Id == Mv.Id).FirstOrDefault();
            if (member == null) throw new Exception("We've found a new member in the HTML which is just looking for new versions...");

            //If there are no previous versions
            if(member.DetailVersions.Count == 0)
            {
                Mv.TimeFirstSeen = scrapeDate;
                Mv.TimeLastSeen = scrapeDate;
                Mv.TimeReplaced = null;

                member.DetailVersions.Add(Mv);

            }

            Debug.WriteLine("Processed " + Mv.Id);


        }

        public void ProcessDownloadedSohMemberPages()
        {
            var dir = new DirectoryInfo(FolderForFilesAwaitingScrape);
            var files = dir.GetFiles();


            foreach(var file in files)
            {
                ProcessDownloadedSohMemberPage(file.FullName);
            }

            Db.Save();

            Directory.CreateDirectory(FolderForFilesAlreadyScraped);
            foreach (var file in files)
            {
                File.Move(file.FullName, Path.Combine(FolderForFilesAlreadyScraped, file.Name));
            }


            
            



        }

        public void DownloadSohMemberPage(SohMember member)
        {
            var s = new BrowserSession();
            var h = s.CreateWebHit(member.SohPageUrl);

            h.Call();

            Directory.CreateDirectory(this.FolderForFilesAwaitingScrape);

            Debug.WriteLine(member.Id);

            string filename = DateTime.Now.ToString("yyyyMMddHHmmss-") + member.Id + ".html";
            string filepath = Path.Combine(this.FolderForFilesAwaitingScrape, filename);

            File.WriteAllText(filepath, h.html);

        }

        public void DownloadSohMemberPages(IEnumerable<SohMember> members)
        {
            foreach(var m in members)
            {
                DownloadSohMemberPage(m);
            }
        }

        public void DownloadSohMemberPages()
        {
            //first do all the members that have no info available.
            DownloadSohMemberPages(Db.Members.Where(x => x.DetailVersions.Count() == 0));

        }

        /// <summary>
        /// Takes a manually downloaded file "results.html" and parses it to update the members list.
        /// </summary>
        public void UpdateMemberList()
        {

            var dir = new DirectoryInfo(Path.Combine(RootFolderPath, "SearchResultsPagesAwaitingScrape"));

            var resultsFile = dir.GetFiles().OrderByDescending(x => x.CreationTime).FirstOrDefault();
            if (resultsFile == null) return;

            StartTime = resultsFile.CreationTime;

            HtmlDocument SearchResultsPage = new HtmlDocument();
            SearchResultsPage.Load(resultsFile.FullName);


            //Directory.CreateDirectory(FolderPath);

            var table = SearchResultsPage.DocumentNode.SelectSingleNode("//table");

            var rows = table.SelectNodes("./tr");


            List<string> FoundMemberIds = new List<string>();

            foreach (var row in rows)
            {
                var link = row.SelectSingleNode("./th/a");
                string url = link.Attributes["href"].Value;

                string id = url.Replace("https://homeopathy-soh.org/homeopath/", "").Replace("/", "");

                string name = link.InnerText;

                FoundMemberIds.Add(id);


                if (Db.Members.Where(x => x.Id == id).Count() == 0)
                {
                    SohMember m = new SohMember();
                    m.Id = id;

                    Debug.WriteLine("New Member :" + m.Id);

                    Db.Members.Add(m);
                }
                
            }

            //Look for members that no longer exist
            var absentMembers = Db.Members.Where(x => !FoundMemberIds.Contains(x.Id));

            foreach(var absentMember in absentMembers)
            {
                //Get latest version Info
                var latest = absentMember.DetailVersions.Where(x => x.TimeReplaced == null).FirstOrDefault();

                var newVersionNeeded = false;

                if (latest == null)
                {
                    newVersionNeeded = true;
                }
                else
                {
                    if(latest.NoLongerExists == false)
                    {
                        newVersionNeeded = true;

                        latest.TimeReplaced = DateTime.Now;
                    }


                }

                if (newVersionNeeded)
                {
                    var newV = new SohMemberDetailVersion();
                    newV.Id = absentMember.Id;
                    newV.NoLongerExists = true;

                    absentMember.DetailVersions.Add(newV);
                }




            }


            //SohMemberDatabase.Save(Db, @"Z:\Simon\Dropbox\Simon\Good Thinking Society\SoHMonitoring\Data\test.xml");
            Db.Save();

            File.Move(resultsFile.FullName, Path.Combine(RootFolderPath, "SearchResultsPagesScraped", resultsFile.Name));


        }


    }


}
