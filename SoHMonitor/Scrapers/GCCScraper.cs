using HtmlAgilityPack;
using Microsoft.Office.Interop.Excel;
using SoHMonitor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ShysterWatch
{


    public class GccData
    {
        public List<GccMember> Members = new List<GccMember>();
        public List<GccClinic> Clinics = new List<GccClinic>();
    }

    public class GccClinic
    {
        public string ClinicName;
        public string Address;
        public string Postcode;
        public string PhoneNumber;

        public List<int> Members = new List<int>();


        public List<string> Websites = new List<string>();


    }

    public class GccMember
    {
        public string Name;
        public int RegistrationNumber;
        public string RegistrationStatus;
        public string EmailAddress;
        //public string Address;
        //public string Postcode;
        //public string PhoneNumber;



        public string[] names = null;
        public string[] Names
        {
            get
            {
                if(names == null)
                {
                    names = Name.Split(' ');
                }
                return names;
            }
        }

        public int ContainsNames(string[] otherNames)
        {
            int nameCount = 0;
            foreach(var name in otherNames)
            {
                if (this.Names.Contains(name)) nameCount++;
            }

            return nameCount;

        }


    }

    public class BCAClinic
    {
        public string ClinicName;
        public List<string> Names = new List<string>();
        public string Address;
        public string Postcode;
        public string PhoneNumber;
        public string EmailAddress;
        public string WebsiteAddress;

        public override bool Equals(object obj)
        {
            var c = (BCAClinic)obj;
            if (ClinicName != c.ClinicName) return false;
            if (EmailAddress != c.EmailAddress) return false;
            if (Postcode != c.Postcode) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ClinicName, EmailAddress, Postcode);

        }

    }


    public class MCAMember
    {
        public string Name;
        public string Address;
        public string Postcode;
        public string PhoneNumber;
        public string EmailAddress;
        public string WebsiteAddress;

        public int MemberNumber;
    }

    public class UCAMember
    {
        public string Name;
        public string ClinicName;
        public string Address;
        public string Postcode;
        public string PhoneNumber;
        public string EmailAddress;
        public string WebsiteAddress;
        public string MemberNumber;

        public override bool Equals(object obj)
        {
            var c = (UCAMember)obj;
            if (ClinicName != c.ClinicName) return false;
            if (EmailAddress != c.EmailAddress) return false;
            if (Postcode != c.Postcode) return false;
            if (Name != c.Name) return false;

            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ClinicName, EmailAddress, Postcode, Name);
        }
    }

    class GCCScraper : MembershipDatabaseScraper
    {




        public GCCScraper()
        {
            ScrapingProcessFunctions.Add(new ScrapingProcessFunction()
            {
                Explanation = "Download membership pages from the GCC website.",
                Process = DownloadGCCWebPages
            });
            ScrapingProcessFunctions.Add(new ScrapingProcessFunction()
            {
                Explanation = "Scrape data from GCC files.",
                Process = ScrapeGccFiles
            });

            ScrapingProcessFunctions.Add(new ScrapingProcessFunction()
            {
                Explanation = "Download membership pages from the BCA website.",
                Process = DownloadBCAWebPages
            });

            ScrapingProcessFunctions.Add(new ScrapingProcessFunction()
            {
                Explanation = "Scrape data from BCA files.",
                Process = ScrapeBcaFiles
            });

            ScrapingProcessFunctions.Add(new ScrapingProcessFunction()
            {
                Explanation = "Download membership pages from the MCA website.",
                Process = DownloadMCAWebPages
            });

            ScrapingProcessFunctions.Add(new ScrapingProcessFunction()
            {
                Explanation = "Scrape data from MCA files.",
                Process = ScrapeMcaFiles
            });


            ScrapingProcessFunctions.Add(new ScrapingProcessFunction()
            {
                Explanation = "Download UCA List Page.",
                Process = DownloadUCAListPage
            });

            ScrapingProcessFunctions.Add(new ScrapingProcessFunction()
            {
                Explanation = "Scrape UCA Index Page and download listings pages.",
                Process = ScrapeUCAListPageAndDownloadListingPages
            });



            ScrapingProcessFunctions.Add(new ScrapingProcessFunction()
            {
                Explanation = "Scrape UCA Listings.",
                Process = ScrapeUCAListingPages
            });

            ScrapingProcessFunctions.Add(new ScrapingProcessFunction()
            {
                Explanation = "Merge Data into Members.xml.",
                Process = MergeData
            });

            ScrapingProcessFunctions.Add(new ScrapingProcessFunction()
            {
                Explanation = "Put websites into Members.xml.",
                Process = PopulateWebsiteList
            });

            AddStandardProcessFunctions();
        }


        public async Task PopulateWebsiteList()
        {
            await Task.Run(() =>
            {
                var db = (UkChiroMemberDatabase)Sys.CurrentGroup.MembershipDb;
                foreach (var m in db.Members)
                {
                    Sys.CurrentGroup.WebsiteDownloader.AddSite(m.WebsiteUrl);

                }

                Sys.CurrentGroup.WebsiteDb.Save();

            });

        }


            public async Task MergeData()
        {
            await Task.Run(() => {

                var db = (UkChiroMemberDatabase)Sys.CurrentGroup.MembershipDb;

                {
                    List<BCAClinic> BcaClinics = new List<BCAClinic>();
                    BcaClinics = (List<BCAClinic>)Utilities.Load(BcaClinics.GetType(), BCAPureMemberXmlFilepath);

                    foreach (var bcaClinic in BcaClinics.Where(x => x.WebsiteAddress != string.Empty))
                    {
                        {

                            var mem = new UkChiroMember();
                            mem.ClinicName = bcaClinic.ClinicName;
                            mem.Postcode = bcaClinic.Postcode;
                            mem.AssociationName = "BCA";
                            mem.WebsiteUrl = bcaClinic.WebsiteAddress;

                            mem.PractitionerNames = bcaClinic.Names;

                            db.Members.Add(mem);
                        }
                    }
                }

                {
                    var members = (List<MCAMember>)Utilities.Load(typeof(List<MCAMember>), MCAPureMemberXmlFilepath);

                    foreach (var member in members.Where(x => x.WebsiteAddress != string.Empty))
                    {
                        {

                            var mem = new UkChiroMember();
                            mem.ClinicName = member.Name;
                            mem.MemberId = member.MemberNumber;
                            mem.Postcode = member.Postcode;
                            mem.AssociationName = "MCA";
                            mem.WebsiteUrl = member.WebsiteAddress;

                            mem.PractitionerNames.Add(member.Name);

                            db.Members.Add(mem);
                        }
                    }
                }


                {
                    var members = (List<UCAMember>)Utilities.Load(typeof(List<UCAMember>), UCAPureMemberXmlFilepath);

                    foreach (var member in members.Where(x => x.WebsiteAddress != string.Empty))
                    {
                        {

                            var mem = new UkChiroMember();
                            mem.ClinicName = member.ClinicName;
                            mem.Postcode = member.Postcode;
                            mem.AssociationName = "UCA";
                            mem.WebsiteUrl = member.WebsiteAddress;
                            mem.PractitionerNames.Add(member.Name);

                            db.Members.Add(mem);
                        }
                    }
                }

                db.Save();



            });
        }

        public void Run(SendMessage SendMsg)
        {
            MessageSender = SendMsg;

            //var t = DownloadGCCWebPages();
            //var t2 = DownloadBCAWebPages();
            //var t3 = DownloadMCAWebPages();
            //ScrapeGccFiles();
            //ScrapeBcaFiles();
            //ScrapeMcaFiles();
            //DownloadUCAListPage();
            //ScrapeUCAListPageAndDownloadTopLevelCategories();
            //await ScrapeUCACategories();

            //ScrapeUCAListingPages();

            MessageSender("done.");
        }


        public async Task ScrapeUCAListingPages()
        {

            await Task.Run(() => {
                List<UCAMember> members = new List<UCAMember>();


                var dir = new DirectoryInfo(UCAListingsPagesHtmlFolderPath);
                foreach (var file in dir.GetFiles())
                {
                    var submembers = ScrapeUCAListingPage(file.FullName);


                    submembers.ForEach(x =>
                    {
                        if (!members.Contains(x)) members.Add(x);
                    });
                }
                Utilities.Save(members, members.GetType(), UCAPureMemberXmlFilepath);
                });
            
        }

        List<UCAMember> ScrapeUCAListingPage(string filepath)
        {



            string GetText(HtmlNode node, string divClassname, int index=1, string tagName="div")
            {
                var div = node.SelectSingleNode($@".//{tagName}[@class='{divClassname}'][{index}]");
                if (div == null) return "";
                return div.InnerText;
            }

            List<UCAMember> members = new List<UCAMember>();

            HtmlDocument doc = new HtmlDocument();
            doc.Load(filepath);

            MessageSender(filepath);

            var quackDivs = doc.DocumentNode.SelectNodes(@"//div[@class='folder-details address-span clickthrough-none']");
            if(quackDivs != null)
            {
                foreach(var quackDiv in quackDivs)
                {
                    UCAMember m = new UCAMember();

                    m.Name = GetText(quackDiv, "pg-title-list", 1, "h3");
                    m.ClinicName = GetText(quackDiv, "directory-organisation");
                    string addr = GetText(quackDiv, "directory-address1");
                    addr += "\r\n" + GetText(quackDiv, "directory-town", 2);

                    m.Address = addr.Trim();

                    m.Postcode = GetText(quackDiv, "directory-postcode");



                    m.PhoneNumber = GetText(quackDiv, "directory-tel");
                    if (m.PhoneNumber.IndexOf("|") != -1)
                    {
                        m.PhoneNumber = m.PhoneNumber.Substring(0, m.PhoneNumber.IndexOf("|")).Trim();
                    }

                    m.MemberNumber = GetText(quackDiv, "pg-custom pg-custom-memno");


                    var linkdiv = quackDiv.SelectSingleNode($@".//div[@class='directory-website'][1]");

                    if(linkdiv != null)
                    {
                        var link = linkdiv.SelectSingleNode($@".//a");
                        if(link != null) m.WebsiteAddress = link.Attributes["href"].Value;

                    }


                    members.Add(m);

                }


            }

            return members;
        }


        //async Task ScrapeUCACategories()
        //{
        //    DirectoryInfo dir = new DirectoryInfo(UCACategoryHtmlFolderPath);
        //    foreach(var file in dir.GetFiles().Where(x => x.Name != "home.html"))
        //    {

        //        await ScrapeUCACategoryAndDownloadSubCategories(file.FullName);

        //    }
        //}

        async Task ScrapeUCACategoryAndDownloadSubCategories(string filepath)
        {

            HtmlDocument doc = new HtmlDocument();
            doc.Load(filepath);

            var tableNode = doc.DocumentNode.SelectSingleNode("//table[@class='directoryContents']");



            var bs = new BrowserSession();
            Directory.CreateDirectory(UCAListingsPagesHtmlFolderPath);

            var links = tableNode.SelectNodes(".//a");

            foreach (var link in links)
            {

                var webPath = link.Attributes["href"].Value;
                var url = "https://unitedchiropractic.org" + webPath;
                var filename = FileUnsafeCharacters.Replace(webPath, "-") + ".html";
                var filepath2 = Path.Combine(UCAListingsPagesHtmlFolderPath, filename);

                var hit = bs.CreateWebHit(url);
                var response = await hit.Call();
                string html = await response.Content.ReadAsStringAsync();

                File.WriteAllText(filepath2, html);

                MessageSender(link.Attributes["href"].Value);
            }




        }




        public async Task ScrapeUCAListPageAndDownloadListingPages()
        {
            

            HtmlDocument doc = new HtmlDocument();
            doc.Load(Path.Combine(UCACategoryHtmlFolderPath, "home.html"));

            Regex or = new Regex(@"\/143\/ajax\/folder_page\.php\'\,\W*data\:\W\{\'ipg\'\:\W(?<ipg>\d*)\,");

            var ipgMatch = or.Match(doc.Text);


            if (ipgMatch.Success)
            {
                var ipg = ipgMatch.Groups["ipg"].Value;

                var bs = new BrowserSession();
                Directory.CreateDirectory(UCAListingsPagesHtmlFolderPath);



                var url = @"https://unitedchiropractic.org/143/ajax/folder_page.php?ipg=" + ipg + "&npage=";

                var page = 0;
                var finished = false;

                while (!finished)
                {
                    var hit = bs.CreateWebHit(url + page);
                    var response = await hit.Call();
                    string html = await response.Content.ReadAsStringAsync();


                    if(html.IndexOf("<li ") != -1)
                    {

                        var filepath2 = Path.Combine(UCAListingsPagesHtmlFolderPath, page + ".html");

                        File.WriteAllText(filepath2, html);

                        MessageSender(page + ".html");

                        page++;
                    }else
                    {
                        finished = true;

                    }

                }




            }
            else
            {
                MessageSender("No IPG");

            }



        }

        public async Task DownloadUCAListPage()
        {
            BrowserSession bs = new BrowserSession();
            var hit = bs.CreateWebHit("https://unitedchiropractic.org/143/Members");
            var response = await hit.Call();

            string html = await response.Content.ReadAsStringAsync();

            Directory.CreateDirectory(UCACategoryHtmlFolderPath);
            File.WriteAllText(Path.Combine(UCACategoryHtmlFolderPath, "home.html"), html);
            
        }

        string UCACategoryHtmlFolderPath => Path.Combine(UCAHtmlFolderPath, "Categories");
        string UCAListingsPagesHtmlFolderPath => Path.Combine(UCAHtmlFolderPath, "ListingsPages");


        public async Task ScrapeMcaFiles()
        {

            await Task.Run(() =>
            {

                List<MCAMember> members = new List<MCAMember>();

                foreach (var file in new DirectoryInfo(MCAHtmlFolderPath).GetFiles())
                {
                    var newMember = ScrapeMcaFile(file.FullName);

                    if (newMember != null) members.Add(newMember);

                }

                Utilities.Save(members, members.GetType(), MCAPureMemberXmlFilepath);

                MessageSender(members.Count + " MCA members found.");

            });
        }

        MCAMember ScrapeMcaFile(string filename)
        {
            List<BCAClinic> clinics = new List<BCAClinic>();

            HtmlDocument doc = new HtmlDocument();
            doc.Load(filename);


            var h1 = doc.DocumentNode.SelectSingleNode("//h1");
            if (h1 == null) return null;

            var m = new MCAMember();

            m.MemberNumber = int.Parse(Path.GetFileNameWithoutExtension(filename));



            m.Name = h1.InnerText.Split('\n')[0].Trim();

            var addressDiv = doc.DocumentNode.SelectSingleNode(".//div[@class='col-md-6']");
            if (addressDiv != null)
            {

                var websiteNode = addressDiv.SelectSingleNode(".//a[@target='_blank']");
                if (websiteNode != null)
                {

                    m.WebsiteAddress = websiteNode.Attributes["href"].Value.Trim();
                    if (m.WebsiteAddress == "http://") m.WebsiteAddress = "";

                }

                var links = addressDiv.SelectNodes("./a");
                var emailLink = links.Where(x => x.Attributes["href"].Value.IndexOf("Mailto: ") == 0).FirstOrDefault();
                if (emailLink != null)
                {
                    m.EmailAddress = emailLink.Attributes["href"].Value.Substring("Mailto: ".Length);
                }


                int BrsInARow = 0;
                m.Address = "";

                var addressFinished = false;

                foreach (var node in addressDiv.ChildNodes)
                {

                    if ((node.Name == "span") || (node.Name == "br"))
                    {


                        if (node.Name == "br") BrsInARow++;
                        else BrsInARow = 0;

                        if (BrsInARow == 2) addressFinished = true;

                        if ((node.Name == "span") && (!addressFinished))
                        {
                            if (m.Address != "") m.Address += "\r\n";
                            var line = node.InnerText.Trim();

                            m.Address += line;

                            if (SoHMonitor.Utilities.UkPostcodeRegEx.IsMatch(line)) m.Postcode = line;
                        }

                        if ((node.Name == "span") && (addressFinished))
                        {
                            string maybePhone = node.InnerText.Trim();
                            if (PhoneNumber.IsMatch(maybePhone)) m.PhoneNumber = maybePhone;
                        }

                    }

                }


            }

            //MessageSender("" + m.Name);
            //MessageSender("  -" + m.Address);
            //MessageSender("  -" + m.Postcode);
            //MessageSender("  -" + m.PhoneNumber);
            //MessageSender("  -" + m.EmailAddress);
            //MessageSender("  -" + m.WebsiteAddress);
            return m;
        }



        async Task ScrapeBcaFiles()
        {
            await Task.Run(() =>
            {
                List<BCAClinic> clinics = new List<BCAClinic>();

                foreach (var file in new DirectoryInfo(BCAHtmlFolderPath).GetFiles())
                {
                    var newClinics = ScrapeBcaFile(file.FullName);

                    newClinics.ForEach(x => { if (!clinics.Contains(x)) clinics.Add(x); });

                }

                Utilities.Save(clinics, clinics.GetType(), BCAPureMemberXmlFilepath);
                MessageSender(clinics.Count() + " BCA members found");

            });
        }


        List<BCAClinic> ScrapeBcaFile(string filename)
        {
            List<BCAClinic> clinics = new List<BCAClinic>();

            HtmlDocument doc = new HtmlDocument();
            doc.Load(filename);

            var listDivs = doc.DocumentNode.SelectNodes("//div[@class='facr-clinic basic']");

            foreach (var listDiv in listDivs)
            {
                var clinic = new BCAClinic();

                var basicDiv = listDiv.SelectSingleNode(".//div[@class='facr-basic']");
                var clinicP = listDiv.SelectSingleNode(".//p[@class='facr-title']");
                clinic.ClinicName = clinicP.InnerText.Trim();

                var fullDiv = listDiv.SelectSingleNode(".//div[@class='facr-full']");

                var namePs = fullDiv.SelectNodes(".//div[@class='chiros']/p");
                foreach(var p in namePs)
                {
                    clinic.Names.Add(p.InnerText.Trim());
                    //MessageSender(p.InnerText.Trim());
                }

                var InfoParagraphs = fullDiv.SelectNodes("./p");

                //var clinicP = listDiv.SelectSingleNode(".//p[@class='facr-title']/a");
                //var clinicName = listDiv.SelectSingleNode(".//p[@class='facr-title']").InnerText.Trim();
                //var clinicName = listDiv.PreviousSibling.PreviousSibling.InnerText;

                var addressP = InfoParagraphs.Where(x => x.InnerHtml.IndexOf(@"<i class=""fa fa-fw fa-location-arrow"">") != -1).FirstOrDefault();
                var spacers = addressP.SelectNodes("./span");
                foreach (var spacer in spacers) spacer.InnerHtml = "\n";
                var addressLines = addressP.InnerText.Split('\n');

                clinic.Address = "";
                foreach (var addressLine in addressLines)
                {
                    var line = addressLine.Trim();
                    
                    if (line != "")
                    {
                        if (clinic.Address != "") clinic.Address += "\r\n";
                        clinic.Address += "" + line +"";

                        if (SoHMonitor.Utilities.UkPostcodeRegEx.IsMatch(line)) clinic.Postcode = line;
                    }
                }

                var phoneNode = InfoParagraphs.Where(x => x.InnerHtml.IndexOf(@"<i class=""fa fa-fw fa-phone"">") != -1).FirstOrDefault();
                if (phoneNode != null)
                {
                    clinic.PhoneNumber = phoneNode.InnerText.Trim();

                }

                var emailNode = InfoParagraphs.Where(x => x.InnerHtml.IndexOf(@"<i class=""fa fa-fw fa-envelope"">") != -1).FirstOrDefault();
                if (emailNode != null)
                {
                    var emailLinkNode = emailNode.SelectSingleNode(".//a");


                    clinic.EmailAddress = HttpUtility.HtmlDecode(emailLinkNode.Attributes["href"].Value.Substring(7));

                    //MessageSender("[" + clinic.EmailAddress + "]\r\n");
                }


                var webNode = fullDiv.SelectSingleNode(".//a[@class='clinic-link']");
                if (webNode != null)
                {
                    var href = webNode.Attributes["href"].Value;
                    href = href.Substring(href.IndexOf("?addr=") + 6);
                    clinic.WebsiteAddress = href;

                }



                clinics.Add(clinic);

            }


            return clinics;
        }



        async Task ScrapeGccFiles()
        {
            await Task.Run(() =>
            {

                GccData gccData = new GccData();

                foreach (var file in new DirectoryInfo(GCCHtmlFolderPath).GetFiles())
                {
                    var gccDataPortion = ScrapeGccFile(file.FullName);
                    gccData.Members.AddRange(gccDataPortion.Members);


                    //Merge clinic data
                    foreach(var foundClinic in gccDataPortion.Clinics)
                    {
                        var matchingClinic = gccData.Clinics.Where(x => (x.Postcode == foundClinic.Postcode) && (x.ClinicName == foundClinic.ClinicName)).FirstOrDefault();

                        if(matchingClinic == null)
                        {
                            gccData.Clinics.Add(foundClinic);
                        }
                        else
                        {
                            var alreadyContains = matchingClinic.Members.Contains(foundClinic.Members.First());

                            if (!alreadyContains) matchingClinic.Members.Add(foundClinic.Members.First());
                            

                        }


                    }

                }

                Utilities.Save(gccData, gccData.GetType(), GCCPureMemberXmlFilepath);

                MessageSender(gccData.Members.Count() + " GCC members and " + gccData.Clinics.Count + " clinics found");

            });

        }

        string GCCPureMemberXmlFilepath => Path.Combine(RootFolderPath, "GCC-Members.xml");
        string BCAPureMemberXmlFilepath => Path.Combine(RootFolderPath, "BCA-Members.xml");
        string MCAPureMemberXmlFilepath => Path.Combine(RootFolderPath, "MCA-Members.xml");
        string UCAPureMemberXmlFilepath => Path.Combine(RootFolderPath, "UCA-Members.xml");


        GccData ScrapeGccFile(string filename)
        {

            var gccData = new GccData();

            HtmlDocument doc = new HtmlDocument();
            doc.Load(filename);

            var chiroNodes = doc.DocumentNode.SelectNodes("//div[@class='card chiro_card p-4 border-none']");

            foreach (var chiroNode in chiroNodes)
            {
                var m = new GccMember();

                var h3 = chiroNode.SelectSingleNode(".//h3");
                m.Name = h3.InnerText;

                var sRegNumber = chiroNode.SelectSingleNode(".//span[@class='text-light-blue'][text() ='Registration Number:']/following-sibling::span").InnerText;

                int.TryParse(sRegNumber, out m.RegistrationNumber);
                //                m.RegistrationNumber = int.Parse(sRegNumber);


                m.RegistrationStatus = chiroNode.SelectSingleNode(".//span[@class='text-light-blue'][text() ='Registration Status:']/following-sibling::span").InnerText;

                var tmp = chiroNode.SelectSingleNode(".//a");
                //[starts-with(local-name(), 'FOO')]

                if (tmp.Attributes["href"] != null)
                {
                    if (tmp.Attributes["href"].Value.IndexOf("mailto:") == 0)
                        m.EmailAddress = tmp.Attributes["href"].Value.Substring(7);
                }

                var detailsTab = chiroNode.SelectSingleNode(".//div[@class='practice-detail-container tab-pane active show']");
                if (detailsTab == null)
                    detailsTab = chiroNode.SelectSingleNode(".//div[@class='practice-detail-container tab-pane ']");

                var clinicsTabs = detailsTab.SelectNodes(".//div[@class='card']");
                if (clinicsTabs != null)
                {
                    foreach (var clinicTab in clinicsTabs)
                    {
                        var clinic = new GccClinic();
                        clinic.Members.Add(m.RegistrationNumber);

                        var clinicNameNode = detailsTab.SelectSingleNode(".//h6");
                        if (clinicNameNode != null) clinic.ClinicName = clinicNameNode.InnerText.Trim();

                        var addressTab = detailsTab.SelectSingleNode(".//p");

                        if (addressTab != null)
                        {

                            var address = addressTab.InnerText;
                            var addressElements = address.Split('\n');


                            clinic.Address = "";
                            foreach (var an in addressTab.ChildNodes)
                            {
                                var t = an.InnerText.Trim();
                                if (t != "")
                                {
                                    if (clinic.Address != "") clinic.Address += "\r\n";
                                    clinic.Address += t;

                                    if (SoHMonitor.Utilities.UkPostcodeRegEx.IsMatch(t)) clinic.Postcode = t;

                                }
                            }
                        }

                        var phoneNode = detailsTab.SelectSingleNode(".//a[@href='tel:']");

                        if (phoneNode != null)
                        {
                            var phone = phoneNode.InnerText.Trim();
                            if (phone != "No phone number listed") clinic.PhoneNumber = phone;
                        }
                        gccData.Clinics.Add(clinic);
                    }
                }


                gccData.Members.Add(m);
                //MessageSender(m.Name);

            }

            return gccData;


        }



        async Task<bool> DownloadBCAWebPage(string postcodePart)
        {

            string filename = Path.Combine(BCAHtmlFolderPath, postcodePart + ".html");
            if (File.Exists(filename)) return false;

            var bSession = new BrowserSession();
            var hit = bSession.CreateWebHit($@"https://chiropractic-uk.co.uk/find-a-chiropractor/?loc={postcodePart}&rad=1000&order=dist&nam=&who=cli");

            var task = hit.Call();

            var response = await task;

            if (response.IsSuccessStatusCode)
            {
                string html = await response.Content.ReadAsStringAsync();


                Directory.CreateDirectory(BCAHtmlFolderPath);



                File.WriteAllText(filename, html);
            }

            return false;
        }

        async Task DownloadBCAWebPages()
        {
            foreach(var postcode in SoHMonitor.Utilities.UkPostCodeDistricts)
            {
                await DownloadBCAWebPage(postcode);
                MessageSender(postcode + " downloaded");
            }



            MessageSender("Complete");
        }






        async Task<bool> DownloadGCCWebPage(int page)
        {

            string filename = Path.Combine(GCCHtmlFolderPath, page + ".html");
            if (File.Exists(filename)) return false;

            var bSession = new BrowserSession();
            var hit = bSession.CreateWebHit(@"https://www.gcc-uk.org/search/chiro_results/P" + page);

            var task = hit.Call();

            var response = await task;

            if (response.IsSuccessStatusCode)
            {
                string html = await response.Content.ReadAsStringAsync();

                if (html.IndexOf(@"Sorry, there are no results. Please try <a href=""/"">another search</a>.") != -1)
                {

                    return true;
                }

                Directory.CreateDirectory(GCCHtmlFolderPath);



                File.WriteAllText(filename, html);
            }

            return false;
        }

        async Task DownloadGCCWebPages()
        {
            int page = 0;

            while (!await DownloadGCCWebPage(page))
            {
                MessageSender(page + " downloaded");
                page += 10;
            }

            MessageSender("Complete");
        }






        async Task<bool> DownloadMCAWebPage(int page)
        {

            string filename = Path.Combine(MCAHtmlFolderPath, page + ".html");
            if (File.Exists(filename)) return false;

            var bSession = new BrowserSession();
            var hit = bSession.CreateWebHit(@"https://mca-chiropractic.org/chiropractor.aspx?ID=" + page);

            var task = hit.Call();

            var response = await task;

            if (response.IsSuccessStatusCode)
            {
                string html = await response.Content.ReadAsStringAsync();


                Directory.CreateDirectory(MCAHtmlFolderPath);



                File.WriteAllText(filename, html);
            }

            return false;
        }

        async Task DownloadMCAWebPages()
        {
            for(int i=1000; i<2150; i++)
            {
                await DownloadMCAWebPage(i);
                MessageSender(i + " downloaded");
            }


            MessageSender("Complete");
        }

        string UCAHtmlFolderPath
        {
            get
            {
                return Path.Combine(RootFolderPath, "UCA Pages");
            }
        }


        string MCAHtmlFolderPath
        {
            get
            {
                return Path.Combine(RootFolderPath, "MCA Pages");
            }
        }

        string BCAHtmlFolderPath
        {
            get
            {
                return Path.Combine(RootFolderPath, "BCA Pages");
            }
        }

        string GCCHtmlFolderPath
        {
            get
            {
                return Path.Combine(RootFolderPath, "GCC Pages");
            }
        }
        string RootFolderPath
        {
            get
            {
                return Path.Combine(Sys.CurrentGroup.FolderPath, "GCC Scraping Data");
            }
        }




        static Regex PhoneNumber = new Regex(@"\+{0,1}[0-9][0-9\s]{7,}[0-9]");

        static Regex FileUnsafeCharacters = new Regex(@"[^0-9a-zA-Z\-]");

    }
}
