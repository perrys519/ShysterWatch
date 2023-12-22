using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ShysterWatch
{
    public class WebSiteDownloader
    {
        public delegate void SendMessage(string message);

        public SendMessage MessageSender;

        static string[] ExtensionBlacklist = { "doc", "jpg", "pdf", "png", "docx", "mp4", "gif", "jpeg", "xml", "swf", "ram", "m4v" };
        public static bool ExtensionIsBlacklisted(string path)
        {

            if (!Uri.IsWellFormedUriString(path, UriKind.Absolute))
            {
                return false;
            }

            Uri uri = new Uri(path, UriKind.Absolute);

            var ext = System.IO.Path.GetExtension(uri.AbsolutePath);

            if (ext != "") ext = ext.Substring(1).ToLower();



            return ExtensionBlacklist.Contains(ext);
        }

        List<WebPage> PagesThatRequireFirstDownload
        {

            get
            {
                return (from site in Sys.CurrentGroup.WebsiteDb.Websites
                        from page in site.WebPages
                        where page.AllowDownload && (page.Lastchecked == null)
                        select page).ToList<WebPage>();


            }

        }



        List<WebPage> PagesThatRequireReScrape_FivePerSite
        {
            get
            {
                DateTime ReScrapeOlderThan = DateTime.Now - new TimeSpan(2, 0, 0, 0);

                List<WebPage> pages = new List<WebPage>();


                for (int getNumber = 0; getNumber < 4; getNumber++)
                {


                    foreach (var site in Sys.CurrentGroup.WebsiteDb.Websites)
                    {
                        //.Where(x => x.Url== "http://www.hironakata.com/index_eng.html")
                        var PagesToDownload = site.WebPages.Where(x => (((x.Lastchecked == null)||(x.Lastchecked < ReScrapeOlderThan)) && (x.AllowDownload))).OrderBy(x => x.Lastchecked).ToList();
                        if (PagesToDownload.Count() > getNumber)
                        {
                            pages.Add(PagesToDownload[getNumber]);
                        }
                    }
                }

                return pages;

                //return (from website in WebsiteDb.Websites
                //                            from webpage in website.WebPages
                //                            where webpage.Lastchecked == null && webpage.AllowDownload
                //                            select webpage).ToList<WebPage>();

            }
        }


        List<WebPage> PagesThatRequireFirstDownload_FivePerSite
        {
            get
            {

                List<WebPage> pages = new List<WebPage>();


                for (int getNumber = 0; getNumber < 4; getNumber++)
                {

                    foreach (var site in Sys.CurrentGroup.WebsiteDb.Websites)
                    {
                        var NeverDownloadedPages = site.WebPages.Where(x => ((x.Lastchecked == null) && (x.AllowDownload))).ToList();
                        if (NeverDownloadedPages.Count() > getNumber)
                        {
                            pages.Add(NeverDownloadedPages[getNumber]);
                        }
                    }
                }

                return pages;

                //return (from website in WebsiteDb.Websites
                //                            from webpage in website.WebPages
                //                            where webpage.Lastchecked == null && webpage.AllowDownload
                //                            select webpage).ToList<WebPage>();

            }
        }

        public bool EndAtNextStop = false;

        class PageInRun
        {
            public WebPage Page;
            public Task<string> Task;
            public string Result;

        }


        public async Task<bool> DownloadBatch(List<WebPage> WebPages)
        {





            var TaskHandler = (from pageInRun in WebPages
                               select new PageInRun
                               {
                                   Page = pageInRun
                               }).ToList<PageInRun>();


            MessageSender("Queueing " + WebPages.Count() + " urls for download:");
            foreach (var taskhandle in TaskHandler)
            {
                MessageSender("  -" + taskhandle.Page.Url);
                taskhandle.Task = taskhandle.Page.Download();

            }

            MessageSender("Awaiting Tasks");

            foreach (var taskhandle in TaskHandler)
            {

                taskhandle.Result = await taskhandle.Task;
            }

            MessageSender("Tasks Returned " + TaskHandler.Count());

            foreach (var taskhandle in TaskHandler)
            {
                MessageSender("Completed " + taskhandle.Result + " " + taskhandle.Page.Url);
            }



            return true;
        }


        async Task WorkOnQueue(int taskReference)
        {
            WebPage PageToWorkOn;

            var ok = PermCycleQueue.TryDequeue(out PageToWorkOn);


            while (ok && !Stop)
            {


                try
                {
                    //MessageSender(taskReference + ". Start: " + PageToWorkOn.Url);
                    var task = PageToWorkOn.Download();
                    var result = await task;

                    MessageSender(result + " - " + PageToWorkOn.Url);


                    
                }
                catch(Exception)
                {
                    Debug.WriteLine("Error on " + PageToWorkOn.Url);
                }


                ok = PermCycleQueue.TryDequeue(out PageToWorkOn);

            }


        }

        ConcurrentQueue<WebPage> PermCycleQueue;

        public bool Stop = false;


        public enum CycleType { InitialDownload, RepeatDownload }

        public async Task PermCycle(CycleType cycleType)
        {
            MessageSender("Starting Perm Cycle.");
            EndAtNextStop = false;
            DateTime TimeLastSaved = DateTime.Now;

            List<WebPage> ToDownload;
            
            if(cycleType == CycleType.InitialDownload) ToDownload = PagesThatRequireFirstDownload_FivePerSite.ToList();
            else ToDownload = PagesThatRequireReScrape_FivePerSite.ToList();


            bool SavedOnLastLoop = false;

            while (ToDownload.Count() > 0)
            {


                PermCycleQueue = new ConcurrentQueue<WebPage>();
                foreach (var p in ToDownload)
                {
                    PermCycleQueue.Enqueue(p);
                }

                int NumberOfConcurrentTasks = 5;
                Task[] ConcurrentTasks = new Task[NumberOfConcurrentTasks];

                for (int i = 0; i < NumberOfConcurrentTasks; i++)
                {
                    ConcurrentTasks[i] = WorkOnQueue(i);
                }

                
                for (int i = 0; i < NumberOfConcurrentTasks; i++)
                {
                    await ConcurrentTasks[i];
                }

                if(PermCycleQueue.Count() == 0) MessageSender("All Tasks Complete");



                SavedOnLastLoop = false;
                if (((DateTime.Now - TimeLastSaved).TotalMinutes > 5) || (PermCycleQueue.Count()==0) || (Stop))
                {
                    MessageSender("Saving XML");

                    try
                    {
                        Sys.CurrentGroup.WebsiteDb.Save();
                        TimeLastSaved = DateTime.Now;
                        SavedOnLastLoop = true;
                    }
                    catch (Exception)
                    {
                    }
                    MessageSender("XML Saved");
                    MessageSender("##########################################################################");
                }
                if (cycleType == CycleType.InitialDownload) ToDownload = PagesThatRequireFirstDownload_FivePerSite.ToList();
                else ToDownload = PagesThatRequireReScrape_FivePerSite.ToList();


                



                Stop = false;

                if (EndAtNextStop) break;

                MessageSender("Restarting with " + ToDownload.Count);
            }

            if (!SavedOnLastLoop)
            {
                try
                {
                    Sys.CurrentGroup.WebsiteDb.Save();
                    TimeLastSaved = DateTime.Now;
                    SavedOnLastLoop = true;
                }
                catch (Exception)
                {
                }
                MessageSender("XML Saved after last loop");
                MessageSender("##########################################################################");
            }


            MessageSender("Finished");

        }



        public async Task DownloadNextPage()
        {



            int NumberOfSimultaneousRequests = 25;


            var ToDownload = PagesThatRequireFirstDownload_FivePerSite.ToList<WebPage>();


            while (ToDownload.Count() != 0)
            {

                for (int i = 0; i < ToDownload.Count; i += NumberOfSimultaneousRequests)
                {
                    var batch = ToDownload.Skip(i).Take(NumberOfSimultaneousRequests).ToList<WebPage>();
                    MessageSender("Downloading " + i + "-" + (i + batch.Count) + " of " + ToDownload.Count() + ".");

                    await DownloadBatch(batch);
                }

                MessageSender("####################################");
                MessageSender("Starting new batch....");
                ToDownload = PagesThatRequireFirstDownload_FivePerSite.ToList<WebPage>();
            }


            //var pagesInThisRun = PagesThatRequireFirstDownload.Take(NumberOfSimultaneousRequests).ToList<WebPage>();



            //bool moreToDownload = true;
            //while (moreToDownload)
            //{
            //    moreToDownload = await DownloadBatch();
            //}

        }







        public void AddSite(string Url)
        {

            if (Url == "") return;


            if (!Uri.IsWellFormedUriString(Url, UriKind.Absolute)) return;

            var uri = new Uri(Url);
            if (uri.Host.ToLower().IndexOf("homeopathy-soh.org") != -1) return;
            if (uri.Host.ToLower().IndexOf(".nhs.uk") != -1) return;

            var simpleUrl = WebPage.SimplifyUrl(Url);
            var matchingSite = Sys.CurrentGroup.WebsiteDb.Websites.Where(x => x.SimplifiedUrl == simpleUrl).FirstOrDefault();
            if(matchingSite == null)
            {
                matchingSite = new Website();
                matchingSite.WebsiteDb = Sys.CurrentGroup.WebsiteDb;
                matchingSite.SourceUrl = Url;

                var page = new WebPage();
                page.Url = Url;
                page.Website = matchingSite;
               
                matchingSite.WebPages.Add(page);

                Sys.CurrentGroup.WebsiteDb.Websites.Add(matchingSite);


            }
            
        }
    }



}
