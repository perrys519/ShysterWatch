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
using SoHMonitor;
using System.Globalization;
using CsvHelper;

namespace ShysterWatch.Scrapers {


	internal class CSVCharity
    {
		public int registered_charity_number{ get; set; }
		public string charity_name		{ get; set; }
		public string charity_contact_web		{ get; set; }
	}
	public class CharityCommissionScraper : MembershipDatabaseScraper
	{
		public CharityCommissionScraper()
		{

			ScrapingProcessFunctions.Add(new ScrapingProcessFunction()
			{
				Explanation = "Import Data from XLSX.",
				Process = ImportDataFromXLSX
			});

            ScrapingProcessFunctions.Add(new ScrapingProcessFunction()
            {
                Explanation = "Put websites into Members.xml.",
                Process = PopulateWebsiteList
            });


            AddStandardProcessFunctions();
		}



		public async Task ImportDataFromXLSX()
        {
			await Task.Run(() =>
			{
				using (var reader = new StreamReader(@"Z:\ComplianceMonitor\Extract 2.csv"))
				using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
				{
					var records = csv.GetRecords<CSVCharity>();

					//MessageSender(records.First().charity_name);

					var db = new CharityCommissionMemberDatabase();
					foreach(CSVCharity record in records)
                    {
						var mem = new CharityCommissionMember();
						mem.CharityName = record.charity_name;
						mem.CharityNumber = record.registered_charity_number;
						string web = record.charity_contact_web;
						if(web != "")
                        {
							if(!((web.IndexOf("http://") == 0) || (web.IndexOf("https://") == 0)))
                            {
								web = "http://" + web;
                            }

							mem.WebsiteUrl = web;
                        }

						db.Members.Add(mem);
                    }

					db.Save();

				}
			}
			);
        }


		public async Task PopulateWebsiteList()
		{
			await Task.Run(() =>
			{
                var db = (CharityCommissionMemberDatabase)Sys.CurrentGroup.MembershipDb;
                foreach (var m in db.Members)
                {
                    Sys.CurrentGroup.WebsiteDownloader.AddSite(m.WebsiteUrl);

                }

                Sys.CurrentGroup.WebsiteDb.Save();

            });

		}


		string HtmlFolderPath
		{
			get
			{
				return Path.Combine(RootFolderPath, "CSV File");
			}
		}

		string RootFolderPath
		{
			get
			{
				return Path.Combine(Sys.CurrentGroup.FolderPath, "Charity Commission Data");
			}
		}

	}
}