using OpenQA.Selenium.DevTools.V94.Network;
using ShysterWatch.ComplaintGenerator;
using ShysterWatch.MembershipDatabases.CNHC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace ShysterWatch
{
    public abstract class AiAnalysisHelper
    {
        public delegate void SendMessage(string message);

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public virtual async Task<bool> ExportData(SendMessage messageDelegate)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            throw new System.Exception("ExportData not implemented.");
        }

        public virtual PromptSpecification PromptSpecification
        {
            get
            {
                return PromptSpecification.PromptSpecifications[Properties.Settings.Default.AiPromptSpecificationReference];
            }
        }





        /// <summary>
        /// A list of websites that are relevant to the current study.
        /// </summary>
        public virtual List<Website> RelevantWebsites
        {
            get
            {
                throw new Exception("Not implemented");
            }
        }


        /// <summary>
        /// Relevant websites where at least one page has downloaded.
        /// </summary>
        public virtual List<Website> OperatingWebsites => RelevantWebsites.Where(w => w.WebPages.Where(p => p.ActiveVersion != null).Count() > 0).ToList();


        /// <summary>
        /// All web pages that have been downloaded.
        /// </summary>
        public virtual List<WebPage> DownloadedWebPages => OperatingWebsites.SelectMany(x => x.WebPages).Where(wp => wp.ActiveVersion != null).ToList();


        public virtual List<WebPage> DownloadedWebPagesWithMalwareDetected => DownloadedWebPages.Where(x => x.ActiveVersion?.MalwareFound == true).ToList();

        public virtual List<WebPage> DownloadedWebPagesWithoutMalwareDetected => DownloadedWebPages.Where(x => x.ActiveVersion?.MalwareFound == false).ToList();

        /// <summary>
        /// Web pages that are to be included in the study. For instance, some studies may limit the number of pages being checked.
        /// </summary>
        public virtual List<WebPage> StudyWebPages => OperatingWebsites.SelectMany(wp => wp.WebPagesInStudy()).ToList();

        /// <summary>
        /// Web pages that have been assessed by the AI.
        /// </summary>
        public virtual List<WebPage> AssessedWebPages => DownloadedWebPagesWithoutMalwareDetected.Where(wp => (wp.ActiveVersion.GetAiClaimsResult(PromptSpecification.PromptReferenceNumber) != null)).ToList();

        /// <summary>
        /// Web page versions that have been assessed by the AI.
        /// </summary>
        public virtual List<WebPageVersion> AssessedWebPageVersions => AssessedWebPages.Select(wp => wp.ActiveVersion).OrderBy(p => p.WebPage.Url).ToList();

        /// <summary>
        /// Operating Websites with Relevant Content
        /// </summary>
        public virtual List<Website> RelevantContentWebsites => RelevantWebPageVersions.Select(pv => pv.WebPage.Website).Distinct().ToList();

        /// <summary>
        /// Operating Websites that have no relevant content.
        /// </summary>
        public virtual List<Website> IrrelevantContentWebsites
        {
            get
            {
                var relevantContentWebsitesHash = new HashSet<Website>(RelevantContentWebsites);
                return OperatingWebsites.Where(w => !relevantContentWebsitesHash.Contains(w)).ToList();
            }
        }


        /// <summary>
        /// The total cost of assessing all web page versions in the study.
        /// </summary>
        public virtual decimal TotalCostSoFar => AssessedWebPageVersions.Sum(v => v.GetAiClaimsResult(PromptSpecification.PromptReferenceNumber)?.Cost) ?? 0;


        /// <summary>
        /// The total number of tokens used assessing all web page versions in the study.
        /// </summary>
        public virtual int TotalTokensSoFar => AssessedWebPageVersions.Sum(v => v.GetAiClaimsResult(PromptSpecification.PromptReferenceNumber)?.TotalTokens) ?? 0;

        public virtual List<WebPageVersion> IrrelevantWebPageVersions => AssessedWebPageVersions.Where(pv => pv.GetAiClaimsResult(PromptSpecification.PromptReferenceNumber).Reason == "Text Irrelevant").ToList();
        public virtual List<WebPageVersion> RelevantWebPageVersions => AssessedWebPageVersions.Where(pv => pv.GetAiClaimsResult(PromptSpecification.PromptReferenceNumber).Reason != "Text Irrelevant").ToList();

        public virtual List<WebPageVersion> RelevantWebPageVersionsWithoutFalseClaims => RelevantWebPageVersions.Where(pv => pv.GetAiClaimsResult(PromptSpecification.PromptReferenceNumber)?.Success == false).ToList();

        public virtual List<WebPageVersion> RelevantWebPageVersionsWithFalseClaims => RelevantWebPageVersions.Where(pv => pv.GetAiClaimsResult(PromptSpecification.PromptReferenceNumber)?.Success == true).ToList();

        public virtual List<Website> RelevantWebsitesWithFalseClaims => RelevantWebPageVersionsWithFalseClaims.Select(x => x.WebPage.Website).Distinct().ToList();



        private List<(int total, Website website)> _WebsitesAndTotalClaims = null;
        public virtual List<(int total, Website website)> WebsitesAndTotalClaims
        {
            get
            {
                if (_WebsitesAndTotalClaims == null)
                {
                    _WebsitesAndTotalClaims = RelevantWebPageVersionsWithFalseClaims.GroupBy(v => v.WebPage.Website).Select(g => (total: g.Sum(v => v.FalseClaims(PromptSpecification.PromptReferenceNumber).Count), website: g.Key)).ToList();

                    var websitesWithClaimsHash = new HashSet<Website>(RelevantWebsitesWithFalseClaims);
                    var relevantWebsitesWithoutClaims = RelevantContentWebsites.Where(w => !websitesWithClaimsHash.Contains(w));
                    _WebsitesAndTotalClaims.AddRange(relevantWebsitesWithoutClaims.Select(w => (total: 0, website: w)));
                }
                return _WebsitesAndTotalClaims;
            }
        }

        /// <summary>
        /// These are WebPageVersions where the JSON returned from ChatGPT is invalid, often due to either using a " character within a string, or using curly quotes to enclose a string.
        /// </summary>
        public virtual List<WebPageVersion> RelevantWebPageVersionsWithDeserializationProblems => RelevantWebPageVersionsWithoutFalseClaims.Where(pv => pv.GetAiClaimsResult(PromptSpecification.PromptReferenceNumber).DeserializationError).ToList();


        public virtual void CreateReport(SendMessage messageDelegate)
        {

            throw new System.Exception("CreateReport not implemented.");
        }






        public virtual void ExtractSampleForManualCheck(int sampleSize, SendMessage messageDelegate)
        {

            var webPages = Sys.CurrentGroup.WebsiteDb.Websites.SelectMany(w => w.WebPages);

            var webPageVersions2 = webPages.Where(wp => wp.PageVersions.Count > 0).Select(wp => wp.PageVersions[0]);

            var processedWebPageVersions = webPageVersions2.Where(v => File.Exists(v.ChatGPT35ExtractedClaimsFilePath(PromptSpecification.PromptReferenceNumber)));

            var pageVersions = processedWebPageVersions.ToList();

            string targetPath = Path.Combine(ShysterWatch.Sys.CurrentGroup.FolderPath, @"SampleExport");

            if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);

            var rnd = new Random();

            var rndSample = pageVersions.Where(v => File.Exists(v.InnerTextFilePath)).OrderBy(x => rnd.Next()).Take(sampleSize).ToList();


            foreach (var pv in rndSample)
            {
                var targetFolder = Path.Combine(targetPath);
                if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);



                var targetFilename = Path.Combine(targetFolder, pv.WebPage.UniqueId + ".txt");
                var aiResponseTargetFilename = Path.Combine(targetFolder, "AI__" + pv.WebPage.UniqueId + ".txt");

                File.Copy(pv.InnerTextFilePath, targetFilename);
                File.Copy(pv.ChatGPT35ExtractedClaimsFilePath(PromptSpecification.PromptReferenceNumber), aiResponseTargetFilename);

            }
            messageDelegate($"Sample data saved at {targetPath}.");
        }

        int pageCount = 0;
        DateTime startTime = DateTime.Now;
        int doneBeforeWeStarted = 0;

        public virtual List<Website> GetWebsiteObjectsForAiProcessing()
        {
            throw new System.Exception("GetWebsiteObjectsForAiProcessing not implemented.");
        }


        public virtual async Task<bool> ProcessClaimsWithAi(ShysterWatch.ComplaintGenerator.Tools.SendMessage messageDelegate)
        {
            //var members = ((CNHCMemberDatabase)Sys.CurrentGroup.MembershipDb).RelevantMembers;
            //var clinics = ((CNHCMemberDatabase)Sys.CurrentGroup.MembershipDb).RelevantClinics;

            //var websiteObjects = clinics.Where(x => x.Website != null).Select(c => c.WebsiteObj).Where(x => x != null).Distinct().ToList();

            var websiteObjects = GetWebsiteObjectsForAiProcessing();

            List<WebPage> webPagesToProcess = new List<WebPage>();
            List<WebPage> webPagesAlreadyDone = new List<WebPage>();





            var numberOfPagesToProcessPerWebsite = 30;

            for (int i = 0; i < numberOfPagesToProcessPerWebsite; i++)
            {

                foreach (var w in websiteObjects)
                {
                    if (w.WebPages.Count > i)
                    {
                        var page = w.WebPages[i];
                        if (page.PageVersions.Count > 0)
                        {
                            if (!page.PageVersions.First().MalwareFound)
                            {
                                if (page.PageVersions[0].RequiresProcessingByAi(PromptSpecification.PromptReferenceNumber))
                                {

                                    webPagesToProcess.Add(page);
                                }
                                else
                                {
                                    webPagesAlreadyDone.Add(page);
                                }
                            }
                        }

                    }
                }
            }



            messageDelegate($"Total {webPagesToProcess.Count()}");

            messageDelegate($"Response failed for {webPagesToProcess.Where(wp => wp.PageVersions.First().AiResponseFailed(PromptSpecification.PromptReferenceNumber) == true).Count()}");
            messageDelegate($"Deserialization failed for {webPagesToProcess.Where(wp => wp.PageVersions.First().GetAiClaimsResult(PromptSpecification.PromptReferenceNumber) != null).Where(wp => wp.PageVersions.First().GetAiClaimsResult(PromptSpecification.PromptReferenceNumber).DeserializationError == true).Count()}");

            foreach (var p in webPagesToProcess)
            {
                if (p.RequiresAiProcessing())
                {
                    if (p.PageVersions.First().AiResponseFailed(PromptSpecification.PromptReferenceNumber) == false)
                    {
                        var v = p.PageVersions.First();
                    }
                }
            }

            pageCount = 0;
            startTime = DateTime.Now;



            doneBeforeWeStarted = webPagesAlreadyDone.Count();
            messageDelegate("doneBeforeWeStarted : " + doneBeforeWeStarted);

            var webpages = webPagesToProcess.ToList();

            messageDelegate("webpages to do : " + webpages.Count);



            foreach (var webPage in webpages)
            {
                await ProcessPage(webPage, messageDelegate);
            }



            messageDelegate("done.");

            return false;


        }


        private async Task<bool> ProcessPage(WebPage webPage, ShysterWatch.ComplaintGenerator.Tools.SendMessage messageDelegate)
        {
            var ok = false;

            if (!webPage.RequiresAiProcessing()) return false;



            var pageVersionA = webPage.PageVersions.Last();



            //if (File.Exists(pageVersionA.ChatGPT35ExtractedClaimsFilePath(2))) return false;

            messageDelegate("\r\n\r\n#############################################");
            messageDelegate($"{DateTime.Now}");
            messageDelegate("Starting web page: " + webPage.FolderPath);

            try
            {
                ok = await ShysterWatch.ComplaintGenerator.Tools.ExtractAndSaveClaimsToTreat(pageVersionA, messageDelegate, PromptSpecification);

                pageCount++;

                var timeTaken = DateTime.Now - startTime;
                var timeTakenText = timeTaken.ToString(@"hh\:mm\:ss");
                var perHour = (int)Math.Round((double)pageCount / (double)timeTaken.TotalHours);

                messageDelegate("Done web page: " + webPage.FolderPath + "");
                messageDelegate($"Total done : {doneBeforeWeStarted + pageCount}");
                messageDelegate($"This session: {pageCount} in {timeTakenText}. {perHour} per hour.");
                messageDelegate(pageVersionA.WebPage.Url + "");
                messageDelegate("ok? " + ok + "");
                messageDelegate("Session cost: $" + ShysterWatch.ComplaintGenerator.Tools.CostThisSession + "");
                messageDelegate("");


                return true;

            }
            catch (Exception )
            {

                messageDelegate("Error... continuing to next page.");
            }

            return ok;
        }
    }



}
