using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ShysterWatch
{

    class SoHXmlFileScraper : MembershipDatabaseScraper
    {

        string RootFolderPath
        {
            get
            {
                return Path.Combine(Sys.CurrentGroup.FolderPath, "SoHXmlFileScraper");
            }
        }
        string XmlFilesPath
        {
            get
            {
                return Path.Combine(RootFolderPath, "XmlFeeds");
            }
        }


        public async Task<bool> DownloadXmlFile()
        {
            var s = new BrowserSession();
            var h = s.CreateWebHit("https://homeopathy-soh.org/xml-feed/");

            string filename = DateTime.Now.ToString("yyyyMMddHHmmss") + ".xml";
            string filePath = Path.Combine(XmlFilesPath, filename);


            var response = await h.Call();
            string html = await response.Content.ReadAsStringAsync();
            Directory.CreateDirectory(XmlFilesPath);
            File.WriteAllText(filePath, html);

            return true;
        }


        public async Task ParseXml()
        {

            await Task.Run(() =>
            {

                foreach (var file in new DirectoryInfo(XmlFilesPath).GetFiles().OrderBy(x => x.CreationTime))
                {
                    ParseXMLFile(file.FullName);
                }

            });
        }


        public SoHXmlFileScraper()
        {
            ScrapingProcessFunctions.Add(new ScrapingProcessFunction()
            {
                Explanation = "Download Membership XML",
                Process = DownloadXmlFile
            });

            ScrapingProcessFunctions.Add(new ScrapingProcessFunction()
            {
                Explanation = "Parse Downloaded XML",
                Process = ParseXml
            });

            AddStandardProcessFunctions();
        }






        public void ParseXMLFile(string filename)
        {

            FileInfo xmlFileInfo = new FileInfo(filename);


            XElement doc = XElement.Load(filename);
            


            var clinicNodes = doc.Descendants("member");

            Debug.WriteLine(clinicNodes.First().ToString());

            var clinics = from c in clinicNodes
                          select new {
                              id = c.Element("id").Value,
                              optout = c.Element("optout").Value == "true",
                              title = c.Element("title").Value,
                              name = c.Element("name").Value,
                              suffix = c.Element("suffix").Value,
                              qualified = c.Element("qualified").Value,
                              address1 = c.Element("location").Element("address1").Value,
                              address2 = c.Element("location").Element("address2").Value,
                              address3 = c.Element("location").Element("address3").Value,
                              address4 = c.Element("location").Element("address4").Value,
                              city = c.Element("location").Element("city").Value,
                              county = c.Element("location").Element("county").Value,
                              postcode = c.Element("location").Element("postcode").Value,
                              country = c.Element("location").Element("country").Value,
                              website = c.Element("location").Element("website").Value,
                              email = c.Element("location").Element("email").Value,
                              phone = c.Element("location").Element("phone").Value


                          };



            var memberIds = clinics.Select(x => x.id).Distinct();

            //Update the last seen date of found members
            foreach(var memberId in memberIds)
            {
                var sohMember = ((SohMemberDatabase)Sys.CurrentGroup.MembershipDb).GetById(memberId);

                //If this is a new member
                if(sohMember == null)
                {
                    var newMember = new SohMember();
                    newMember.Id = memberId;
                    newMember.FirstSeen = xmlFileInfo.CreationTime;
                    newMember.LastSeen = xmlFileInfo.CreationTime;
                    ((SohMemberDatabase)Sys.CurrentGroup.MembershipDb).Members.Add(newMember);
                }

                //If it's an existing member
                if(sohMember != null)
                {
                    sohMember.LastSeen = xmlFileInfo.CreationTime;
                }

            }

            //Remove any clinics that no longer exist
            var OldMembers = ((SohMemberDatabase)Sys.CurrentGroup.MembershipDb).Members.Where(x => !memberIds.Contains(x.Id));
            foreach(var member in OldMembers)
            {
                member.NoticedDeleted = xmlFileInfo.CreationTime;
            }



            foreach(var clinic in clinics)
            {
                //Get the member 
                var sohMember = ((SohMemberDatabase)Sys.CurrentGroup.MembershipDb).GetById(clinic.id);

                var newClinic = new SohClinic();
                newClinic.Id = clinic.id;
                newClinic.Name = clinic.name;
                newClinic.Qualified = clinic.qualified;
                newClinic.Address1 = clinic.address1;
                newClinic.Address2 = clinic.address2;
                newClinic.Address3 = clinic.address3;
                newClinic.Address4 = clinic.address4;
                newClinic.City = clinic.city;
                newClinic.County = clinic.county;
                newClinic.Postcode = clinic.postcode;
                newClinic.Country = clinic.country;
                newClinic.Email = clinic.email;
                newClinic.PhoneNumber = clinic.phone;
                newClinic.Website = clinic.website;


                var matchingClinic = sohMember.Clinics.Where(x => x.DataIsSameAs(newClinic)).FirstOrDefault();

                if(matchingClinic != null)
                {
                    matchingClinic.DataVerified.Add(xmlFileInfo.CreationTime);
                    matchingClinic.TimeLastSeen = xmlFileInfo.CreationTime;
                }
                else
                {
                    newClinic.DataVerified.Add(xmlFileInfo.CreationTime);
                    newClinic.TimeFirstSeen = xmlFileInfo.CreationTime;
                    newClinic.TimeLastSeen = xmlFileInfo.CreationTime;
                    sohMember.Clinics.Add(newClinic);
                }
            }

            foreach(var sohMember in ((SohMemberDatabase)Sys.CurrentGroup.MembershipDb).Members)
            {
                foreach(var deadClinic in sohMember.Clinics.Where(x => (x.TimeLastSeen != xmlFileInfo.CreationTime) && (x.TimeNoticeRemoved == null)))
                {
                    deadClinic.TimeNoticeRemoved = xmlFileInfo.CreationTime;
                }


            }



            ((SohMemberDatabase)Sys.CurrentGroup.MembershipDb).Save();
            
        }



    }

    
    


}
