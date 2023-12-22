using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShysterWatch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using System.Security.Policy;
using System.Diagnostics;
using Microsoft.Office.Interop.Excel;
using System.Web;
using ShysterWatch.MembershipDatabases.CNHC;
using System.Threading;
using System.IO.Compression;

namespace ShysterWatch.Scrapers
{
    class CNHCScraper : MembershipDatabaseScraper
    {

        public CNHCScraper()
        {
            ScrapingProcessFunctions.Add(new ScrapingProcessFunction()
            {
                Explanation= "Download web pages",
                Process = DownloadWebPages
            }
            );
            ScrapingProcessFunctions.Add(new ScrapingProcessFunction()
            {
                Explanation = "Scrape web pages",
                Process = ScrapePages
            }
            );

            ScrapingProcessFunctions.Add(new ScrapingProcessFunction()
            {
                Explanation = "Generate Stats",
                Process = GenerateStats
            }
);

            AddStandardProcessFunctions();
        }

        async Task GenerateStats()
        {
            await Task.Run(() =>
            {



                var db = MembershipDatabases.CNHC.CNHCMemberDatabase.Load();

                MessageSender($"Total members claimed by CNHC at time of scrape: 6217");
                MessageSender($"Members found: {db.Members.Count()}");
                MessageSender($"Clinics found: {db.Clinics.Count()}");
                MessageSender($"Members in Clinics: {db.Clinics.SelectMany(c => c.Members).Count()}");
                MessageSender($"Unique Members in Clinics: {db.Clinics.SelectMany(c => c.Members).Distinct().Count()}");

                //Need:
                // websites, web pages, sucessful web sites/failed. Reason why failed?
                


            });
        }

        async Task ScrapePages()
        {
            await Task.Run(() =>
            {

                var dir = new DirectoryInfo(this.HtmlFolderPath);
                var db = new MembershipDatabases.CNHC.CNHCMemberDatabase();

                foreach (var file in dir.GetFiles())
                {
                    var subDb = ScrapePage(file.FullName);

                    foreach (var newMem in subDb.Members)
                    {
                        if (db.Members.Where(x => x.MembershipNumber == newMem.MembershipNumber).FirstOrDefault() == null)
                        {
                            db.Members.Add(newMem);
                        }
                    }
                    foreach (var newClinic in subDb.Clinics)
                    {
                        var matchingClinic = db.Clinics.Where(x => x.Website == newClinic.Website).Where(x => x.Postcode == newClinic.Postcode).FirstOrDefault();
                        if (matchingClinic == null)
                        {
                            newClinic.Db = db;
                            db.Clinics.Add(newClinic);
                        }
                        else
                        {
                            foreach (var memNumber in newClinic.MemberNumbers)
                            {
                                if (!matchingClinic.MemberNumbers.Contains(memNumber)) matchingClinic.MemberNumbers.Add(memNumber);
                            }
                        }
                    }

                }

                db.Save();
            });
        }

        MembershipDatabases.CNHC.CNHCMemberDatabase ScrapePage(string filePath)
        {
            FileInfo fi = new FileInfo(filePath);
            MessageSender("Scraping " + fi.Name);

            HtmlDocument doc = new HtmlDocument();
            doc.Load(filePath);

            var db = new MembershipDatabases.CNHC.CNHCMemberDatabase();

            //try
            {

                var resultsDiv = doc.DocumentNode.SelectSingleNode(".//div[@class='search-table mt-4']");

                var resList = doc.DocumentNode.SelectNodes(".//div[@class='row search-table-body ' or @class='row search-table-body background']");
                if (resList == null) return db;

                var results1 = resList.ToList();
                var results = results1;

                //var resList2 = doc.DocumentNode.SelectNodes(".//div[@class='hide']");
                //if (resList2 != null)
                //{
                //    var results2 = resList2.ToList();


                //    //if (results2 != null) results2.ForEach(x => results = results.Append(x).ToList());
                //    if (results2 != null) results2.ForEach(x => results.Add(x));
                //}



                foreach (var result in results)
                {
                    var searchResultLine1 = result.SelectSingleNode(".//div[@class='col-xs-12 col-sm-6 col-md-3 col-lg-3']");
                    var nameSpan = searchResultLine1.SelectSingleNode(".//p[@class='name']");

                    var mem = new MembershipDatabases.CNHC.CNHCMember();
                    mem.Name = nameSpan.InnerText;

                    var regTitle = searchResultLine1.SelectSingleNode(@".//p[@class='value']");
                    mem.MembershipNumber = (regTitle.InnerText);
                    mem.MembershipNumber = mem.MembershipNumber.Substring(0, mem.MembershipNumber.IndexOf(" "));


                    var disciplineTitleNode = result.SelectSingleNode(@".//span[@class='dot']");
                    if (disciplineTitleNode != null)
                    {
                        HtmlNode sibling = disciplineTitleNode;
                        while ((sibling = sibling.NextSibling) != null){
                            string stext = sibling.InnerText.Trim();
                            if (stext != "") mem.Disciplines.Add(stext);

                        }
                    }

                    db.Members.Add(mem);

                    var clinic = new MembershipDatabases.CNHC.Clinic();
                    clinic.MemberNumbers.Add(mem.MembershipNumber);


                    var linkNode = result.SelectSingleNode(".//a[@target='_blank']");
                    if(linkNode != null)
                    {
                        var href = linkNode.Attributes["href"].Value;

                        Uri uriResult;
                        bool isValidUrl = Uri.TryCreate(href, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                        //if (isValidUrl)
                        {
                            clinic.ListedWebsite = href;

                            if (isValidUrl) clinic.Website = uriResult.ToString();

                            clinic.Db = db;
                        }

                    }
                    db.Clinics.Add(clinic);



                    //var searchResultLine2 = result.SelectSingleNode(".//div[@class='col-xs-12 col-sm-12 col-md-6 col-lg-6 row']");

                    //var allDivs = searchResultLine2.SelectNodes(".//div");
                    //var divCollections = new List<List<HtmlNode>>();

                    //{
                    //    var divCollection = new List<HtmlNode>();

                    //    foreach (var div in allDivs)
                    //    {

                    //        if (div.HasClass("clearfix"))
                    //        {
                    //            divCollections.Add(divCollection);
                    //            divCollection = new List<HtmlNode>();
                    //        }
                    //        else
                    //        {
                    //            divCollection.Add(div);
                    //        }
                    //    }
                    //}




                    //foreach (var divCollection in divCollections)
                    //{
                    //    var clinic = new MembershipDatabases.CNHC.Clinic();

                    //    //remove the title line
                    //    divCollection.RemoveAt(0);

                    //    var linkDiv = divCollection.Where(x => x.Attributes["class"].Value == "col-xs-12 col-sm-6 col-md-3 col-lg-3 addressLine").Where(x => x.SelectSingleNode(".//a") != null).FirstOrDefault();
                    //    if(linkDiv != null)
                    //    {

                    //        var a = linkDiv.SelectSingleNode(".//a");
                    //        if(a != null)
                    //        {
                    //            clinic.Website = a.Attributes["href"].Value;
                    //            if (clinic.Website.Length < 9) clinic.Website = "";


                    //        }
                    //        divCollection.Remove(linkDiv);
                    //    }

                    //    var phoneDiv = divCollection.Where(x => (x.InnerText.IndexOf("Phone: ") != -1)|| (x.InnerText.IndexOf("Mobile: ") != -1)).FirstOrDefault();
                    //    if(phoneDiv != null)
                    //    {
                    //        {
                    //            var phoneEl = phoneDiv.ChildNodes.Where(x => (x.InnerText.IndexOf("Phone: ") != -1)).FirstOrDefault();
                    //            if (phoneEl != null)
                    //            {
                    //                clinic.Phone = phoneEl.InnerText.Replace("Phone: ", "").Trim();
                    //            }
                    //        }
                    //        {
                    //            var phoneEl = phoneDiv.ChildNodes.Where(x => (x.InnerText.IndexOf("Mobile: ") != -1)).FirstOrDefault();
                    //            if (phoneEl != null)
                    //            {
                    //                clinic.Mobile = phoneEl.InnerText.Replace("Mobile: ", "").Trim();

                    //            }
                    //        }

                    //        divCollection.Remove(phoneDiv);
                    //    }

                    //    //                        var addressDiv = divCollection.Where(x => x.Attributes["class"].Value == "col-xs-12 col-sm-6 col-md-3 col-lg-3 addressLine").Where(x => x.InnerText.IndexOf()).FirstOrDefault();

                    //    var addressDiv = divCollection.FirstOrDefault();
                    //    if(addressDiv != null)
                    //    {
                    //        var addressLines = addressDiv.InnerText.Split(',');
                    //        foreach(var line in addressLines)
                    //        {
                    //            var l = line.Trim();
                    //            if(l != "")
                    //            {
                    //                if (clinic.Address != "") clinic.Address += "\r\n";
                    //                clinic.Address += l;

                    //                if (SoHMonitor.Utilities.UkPostcodeRegEx.IsMatch(l)) clinic.Postcode = l;
                    //            }

                    //        }
                    //    }

                    //    clinic.MemberNumbers.Add(mem.MembershipNumber);
                    //    clinic.Db = db;
                    //    db.Clinics.Add(clinic);
                    //}



                }



                MessageSender(results.Count + " / " + results1.Count);

            }
           // catch (Exception exececc)
            {
            }
            return db;
        }


        async Task<bool> DownloadWebPage(string postcodePart)
        {

            string name = $"{postcodePart}.html";


            string filename = Path.Combine(HtmlFolderPath, name);
            if (File.Exists(filename)) return false;

            var bSession = new BrowserSession();

            var url = $@"https://search.cnhcregister.org.uk/?name=&therapy=&city=&radius={10}&Search&postcode={HttpUtility.UrlPathEncode(postcodePart)}";
            Debug.WriteLine(url);

            var hit = bSession.CreateWebHit(url);

            var task = hit.Call();

            var response = await task;

            if (response.IsSuccessStatusCode)
            {
                string html = "";
                if (response.Content.Headers.ContentEncoding.Contains("gzip"))
                {
                    using (var decompressionStream = new GZipStream(await response.Content.ReadAsStreamAsync(), CompressionMode.Decompress))
                    using (var reader = new StreamReader(decompressionStream))
                    {
                        html = await reader.ReadToEndAsync();
                    }
                }else html = await response.Content.ReadAsStringAsync();


                Directory.CreateDirectory(HtmlFolderPath);



                File.WriteAllText(filename, html, Encoding.UTF8);


                if(html.IndexOf(@">Next &rsaquo;</a>") != -1)
                {
                    return true;
                }

            }
            else
            {
                throw new Exception("Error when getting page " + postcodePart);
            }

            return false;
        }

        async Task DownloadWebPages()
        {

            


            foreach (var postcode in SoHMonitor.Utilities.UKPostcodesOnePerArea)
            {
                await DownloadWebPage(postcode);
                MessageSender(postcode + " downloaded");
            }



            MessageSender("Complete");
        }


        string HtmlFolderPath
        {
            get
            {
                return Path.Combine(RootFolderPath, "CNHC Pages");
            }
        }

        string RootFolderPath
        {
            get
            {
                return Path.Combine(Sys.CurrentGroup.FolderPath, "GNHC Scraping Data");
            }
        }

    }
}
