using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShysterWatch.Scrapers
{
    class GoSCScraper : MembershipDatabaseScraper
    {

        public GoSCScraper()
        {
            ScrapingProcessFunctions.Add(new ScrapingProcessFunction()
            {
                Explanation = "Download web pages",
                Process = DownloadWebPages
            });

            ScrapingProcessFunctions.Add(new ScrapingProcessFunction()
            {
                Explanation = "Scrape web pages",
                Process = ScrapePages
            }

);

            AddStandardProcessFunctions();
        }


        async Task ScrapePages()
        {
            await Task.Run(() =>
            {

                var dir = new DirectoryInfo(this.HtmlFolderPath);
                var db = new MembershipDatabases.GoSC.GoSCMemberDatabase();

                foreach (var file in dir.GetFiles())
                {
                    var subDb = ScrapePage(file.FullName);



                    foreach (var newMem in subDb.Members)
                    {
                        if (db.Members.Where(x => x.UrlId == newMem.UrlId).FirstOrDefault() == null)
                        {
                            db.Members.Add(newMem);
                        }
                    }
                    foreach (var newClinic in subDb.Clinics)
                    {
                        var matchingClinic = db.Clinics.Where(x => x.Phone == newClinic.Phone).Where(x => x.Postcode == newClinic.Postcode).FirstOrDefault();
                        if (matchingClinic == null)
                        {
                            newClinic.Db = db;
                            db.Clinics.Add(newClinic);
                        }
                        else
                        {
                            foreach (var memNumber in newClinic.MemberUrlIds)
                            {
                                if (!matchingClinic.MemberUrlIds.Contains(memNumber)) matchingClinic.MemberUrlIds.Add(memNumber);
                            }
                        }
                    }



                }

                db.Save();
            });
        }



        MembershipDatabases.GoSC.GoSCMemberDatabase ScrapePage(string filePath)
        {
            FileInfo fi = new FileInfo(filePath);
            MessageSender("Scraping " + fi.Name);

            HtmlDocument doc = new HtmlDocument();
            doc.Load(filePath);

            var db = new MembershipDatabases.GoSC.GoSCMemberDatabase();

            var resultsDiv = doc.DocumentNode.SelectSingleNode(".//div[@id='practices']");
            var resList = doc.DocumentNode.SelectNodes(".//div[@class='item']");
            if (resList == null) return db;

            var results1 = resList.ToList();
            var results = results1;

            MessageSender(results.Count + " results found");

            var AddressCleanRegEx = new Regex(@"\t*\n\t*");
                var AddressCleanRegEx2 = new Regex(@"\n\n");

            var ClinicUrlPartRegEx = new Regex(@"\/practices\/(?<UrlPart>[\d]*\-[^\/]*)\/");
            var MemberUrlPartRegEx = new Regex(@"\/registrants\/(?<UrlPart>[\d]*\-[^\/]*)\/");


            foreach (var result in results)
            {
                var practiceNameNode = result.SelectSingleNode(".//h3/a");
                var clinic = new MembershipDatabases.GoSC.Clinic();
                clinic.Name = practiceNameNode.InnerText.Substring(4);



                clinic.UrlId = ClinicUrlPartRegEx.Match(practiceNameNode.Attributes["href"].Value).Groups[1].Value;
                MessageSender("clinic.UrlId " + clinic.UrlId);

                var addressNode = result.SelectSingleNode(".//div[@class='practice-address']/address");
                var phoneNode = result.SelectSingleNode(".//div[@class='practice-details']/dl/dd[@class='tel']");
                var emailNode = result.SelectSingleNode(".//div[@class='practice-details']/dl/dd[@class='email']/a");
                var webNode = result.SelectSingleNode(".//div[@class='practice-details']/dl/dd[@class='web']/a");

                clinic.Address = AddressCleanRegEx.Replace(addressNode.InnerText, "\n").Trim();
                clinic.Address = clinic.Address.Replace("\r\n\r\n", "\r\n");
                clinic.Address = clinic.Address.Replace("\r\n\r\n", "\r\n");
                clinic.Address = clinic.Address.Replace("\r\n\r\n", "\r\n");
                MessageSender(clinic.Address);

                if(phoneNode != null)
                clinic.Phone = phoneNode.InnerText.Trim();

                MessageSender("phone: " + clinic.Phone);

                if (emailNode != null)
                {
                    clinic.Email = emailNode.Attributes["href"].Value.Substring(7);
                    MessageSender(emailNode.Attributes["href"].Value.Substring(7));
                }
                if (webNode != null)
                {
                    clinic.Website = webNode.Attributes["href"].Value;
                    MessageSender(webNode.Attributes["href"].Value);
                }

                db.Clinics.Add(clinic);

                var practitionerNodes = result.SelectNodes(".//div[@class='practitioners']/ul/li/a");

                if(practitionerNodes != null)
                foreach(var practitionerNode in practitionerNodes)
                {
                    var member = new MembershipDatabases.GoSC.GoSCMember();
                    member.Name = practitionerNode.InnerText.Trim();

                    MessageSender(" **" + member.Name);

                    member.UrlId = MemberUrlPartRegEx.Match(practitionerNode.Attributes["href"].Value).Groups[1].Value;

                    db.Members.Add(member);
                    clinic.MemberUrlIds.Add(member.UrlId);
                }

            }


            return db;
        }



            async Task<bool> DownloadWebPage(int page)
        {

            string filename = Path.Combine(HtmlFolderPath, page + ".html");
            if (File.Exists(filename)) return false;

            var bSession = new BrowserSession();
            var hit = bSession.CreateWebHit($@"https://www.osteopathy.org.uk/register-search/practices/?location=cv4+7au&radius=1000&surname=&registration=&p={page}");

            var task = hit.Call();

            var response = await task;

            if (response.IsSuccessStatusCode)
            {
                string html = await response.Content.ReadAsStringAsync();


                Directory.CreateDirectory(HtmlFolderPath);



                File.WriteAllText(filename, html);

                if (html.IndexOf("No results were found; please try broadening your search") != -1) return true;
            }

            return false;
        }

        async Task DownloadWebPages()
        {
            
            for(int c=1; c < 10000; c++)
            {
                var finished = await DownloadWebPage(c);
                MessageSender("Page " + c + " downloaded");
                if (finished) return;
            }
        }


        string HtmlFolderPath
        {
            get
            {
                return Path.Combine(RootFolderPath, "GoSC Pages");
            }
        }

        string RootFolderPath
        {
            get
            {
                return Path.Combine(Sys.CurrentGroup.FolderPath, "GoSC Scraping Data");
            }
        }

    }
}
