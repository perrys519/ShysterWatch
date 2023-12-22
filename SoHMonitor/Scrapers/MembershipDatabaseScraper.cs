using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShysterWatch
{

    public abstract class MembershipDatabaseScraper
    {
        public SendMessage MessageSender;

        public delegate void SendMessage(string message);

        public delegate Task RunScrapingProcess();

        /// <summary>
        /// A list of the processes that are required to run the scraping process, in order.
        /// </summary>
        public List<ScrapingProcessFunction> ScrapingProcessFunctions = new List<ScrapingProcessFunction>();
        public void AddStandardProcessFunctions()
        {
            ScrapingProcessFunctions.Add(new ScrapingProcessFunction()
            {
                Explanation = "Copy Urls To Website DB",
                Process = CopySourceUrlsToWebsite
            }
            );
        }


        public async Task CopySourceUrlsToWebsite()
        {
            await Task.Run(() =>
            {
                foreach(var url in Sys.CurrentGroup.MembershipDb.GetAllSourceUrls)
                {
                    Sys.CurrentGroup.WebsiteDownloader.AddSite(url);
                }

                Sys.CurrentGroup.WebsiteDb.Save();

            });
        }



        //public abstract void Run(SendMessage SendMsg);
    }

    public class ScrapingProcessFunction
    {
        public MembershipDatabaseScraper.RunScrapingProcess Process;
        public string Explanation;

        public override string ToString()
        {
            return Explanation;
        }
    }
    


}
