using BrotliSharpLib;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static System.Net.WebRequestMethods;

namespace ShysterWatch
{
    public class WebPage
    {
        public string Url;
        public string OriginalUrl;
        public bool WebPageDeleted = false;
        public bool WebPageIsNotHtml = false;

        public DateTime? Lastchecked = null;
        public List<WebPageVersion> PageVersions = new List<WebPageVersion>();


        

        public bool AllowDownload
        {
            get
            {
                if (WebPageIsNotHtml) return false;
                if (WebPageDeleted) return false;
                if (IsTooSimilarToAnotherPage) return false;
                return true;
            }
        }


        public bool RequiresAiProcessing()
        {
            if(PageVersions.Count == 0) return false;
            if(!this.Website.ContinueCheckingForClaims()) return false;

            return true;
        }


        //simplifies a Url to check if it is the same URL as another.
        public static string SimplifyUrl(string url)
        {
            var u = url.ToLower();
            u = u.Replace("http://", "");
            u = u.Replace("https://", "");
            if (u.IndexOf("www.") == 0) u = u.Substring(4);

            //remove any hash
            if (u.IndexOf("#") != -1) u = u.Substring(0, u.IndexOf("#"));

            //if the last character is a "/", remove it
            if (u.Substring(u.Length - 1, 1) == "/") u = u.Substring(0, u.Length - 1);

            u = u.Replace("//", "/");

            return u;
        }

        public string SimplifiedUrl
        {
            get
            {
                return SimplifyUrl(Url);
            }
        }

        private string uniqueId = "";
        public string UniqueId
        {
            get
            {
                if (uniqueId == "")
                {
                    if (Url == "") throw new Exception("Cannot create UniqueId until Url set");

                    Uri uri = new Uri(Url);


                    string url = uri.Host + "-" + uri.AbsolutePath;

                    Regex filenameClean = new Regex(@"[^a-zA-Z0-9\-]+");

                    url = filenameClean.Replace(url, "-");
                    url = url.Replace("--", "-");

                    if (url.Length > 40) url = url.Substring(0, 40);

                    string testUrl = url;
                    int count = 0;

                    var cont = (Website.WebPages.Where(x => x != this).Where(x => x.uniqueId == testUrl).FirstOrDefault() != null);

                    while (cont)
                    {
                        count++;
                        testUrl = url + count;

                        cont = (Website.WebPages.Where(x => x != this).Where(x => x.uniqueId == testUrl).FirstOrDefault() != null);
                    }
                    uniqueId = testUrl;

                    if (uniqueId == "") uniqueId = "-";

                }
                return uniqueId;
            }
            set
            {
                uniqueId = value;
            }
        }

        /// <summary>
        /// Gets the page version relevant to what the user is working on as set in Properties.Settings.Default.RelevantDateForChoosingVersion
        /// If null, it will just pick the most recent version (default).
        /// </summary>
        [XmlIgnore]
        public WebPageVersion ActiveVersion
        {
            get{
                if (PageVersions == null) return null;
                if (PageVersions.Count == 0) return null;

                if (Properties.Settings.Default.RelevantDateForChoosingVersion == null) return PageVersions.Last();

                //if there's a date set, return the latest version that hasn't expired by this date.
                return PageVersions.Where(v =>
                ((v.WebPageChangedFromThisVersion == null) || (v.WebPageChangedFromThisVersion > Properties.Settings.Default.RelevantDateForChoosingVersion))
                ).LastOrDefault();


                
            }
        }

        [XmlIgnore]
        bool DownloadInProgress = false;

        [XmlIgnore]
        public Website Website;


        public static string WebsiteFolderPath
        {
            get
            {
                return Path.Combine(Sys.CurrentGroup.FolderPath, "Websites");
            }
        }

        public string FolderPath
        {
            get
            {
                return Path.Combine(WebsiteFolderPath, Website.UniqueId, UniqueId);
            }
        }

        /// <summary>
        /// If, when downloaded, this page seems to be very similar to another on one the website.
        /// </summary>
        public bool IsTooSimilarToAnotherPage = false;



        string GetBaseUri(HtmlDocument doc)
        {

            var baseTag = doc.DocumentNode.SelectSingleNode("//base");

            //If there is no base tag, return the URI
            if (baseTag == null)
            {
                return this.Url;
            }

            if (baseTag.Attributes["href"] != null)
            {
                var currentBase = baseTag.Attributes["href"].Value;

                //If there is a base tag, resolve it.
                var abs = GetAbsoluteUrlString(this.Url, currentBase);

                return abs;
            }

            return this.Url;

        }


        void InsertBaseTag(HtmlDocument doc)
        {

            var baseTag = doc.DocumentNode.SelectSingleNode("//base");
            if (baseTag != null)
            {
                if (baseTag.Attributes["href"] != null)
                {
                    baseTag.Attributes["href"].Value = GetBaseUri(doc);
                    return;
                }
            }



            HtmlNode head = doc.DocumentNode.SelectSingleNode("/html/head");
            if (head == null)
            {
                HtmlNode htmlTag = doc.DocumentNode.SelectSingleNode("/html");

                if (htmlTag == null) return;

                head = doc.CreateElement("head");
                htmlTag.InsertBefore(head, htmlTag.FirstChild);

            }

            HtmlNode link = doc.CreateElement("base");

            link.Attributes.Add("href", GetBaseUri(doc));
            head.InsertBefore(link, head.FirstChild);
        }

        static string GetAbsoluteUrlString(string baseUrl, string url)
        {

            if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute)) return "";

            var uri = new Uri(url, UriKind.RelativeOrAbsolute);
            if (!uri.IsAbsoluteUri)
            {
                if (!Uri.IsWellFormedUriString(baseUrl, UriKind.Absolute)) return "";
                uri = new Uri(new Uri(baseUrl), uri);
            }
            return uri.ToString();



        }

        bool IsInWebsite(string url)
        {
            Uri uri = new Uri(url);
            Uri BaseUri = new Uri(this.Website.SourceUrl);
            Uri PageBaseUri = new Uri(this.Url);

            if (uri.Host.ToLower() != BaseUri.Host.ToLower())
            {
                var a = uri.Host.ToLower().Replace("www.", "");
                var b = BaseUri.Host.ToLower().Replace("www.", "");

                if (a != b)
                {
                    if (uri.Host.ToLower() != PageBaseUri.Host.ToLower())
                    {
                        var a2 = uri.Host.ToLower().Replace("www.", "");
                        var b2 = BaseUri.Host.ToLower().Replace("www.", "");

                        if (a2 != b2)
                        {
                            return false;
                        }

                    }


                }

            }


            if (BaseUri.AbsolutePath == "") return true;

            if (uri.AbsolutePath.ToLower().IndexOf(BaseUri.AbsolutePath.ToLower()) == 0) return true;

            return false;
        }


        public string FoundOnPage = "";

        List<string> GetLinks(HtmlDocument doc)
        {
            List<string> links = new List<string>();

            var aNodes = doc.DocumentNode.SelectNodes("//a");
            if (aNodes == null) return links;
            var hrefs = aNodes.Where(l => l.Attributes["href"] != null).Select(l => l.Attributes["href"].Value);

            var baseHref = GetBaseUri(doc);

            foreach (var href in hrefs)
            {


                if (href.IndexOf("#") != 0)
                {
                    var absUrl = GetAbsoluteUrlString(baseHref, href);

                    var urlLower = absUrl.ToLower();
                    if ((urlLower.IndexOf("http://") == 0) || (urlLower.IndexOf("https://") == 0))
                    {
                        if (IsInWebsite(absUrl))
                        {
                            //remove the hashtag
                            if (absUrl.IndexOf("#") != -1)
                            {
                                absUrl = absUrl.Substring(0, absUrl.IndexOf("#"));
                            }
                            links.Add(absUrl);
                        }



                    }


                }
            }
            return links.Distinct().ToList();
        }

        public void Test()
        {
            this.Url = @"https://one-life.org.uk/";
            var x = this.Download();
            x.Wait();
        }

        public async Task<string> Download()
        {


            DownloadInProgress = true;

            Lastchecked = DateTime.Now;
            

            DateTime? LastDownload = null;
            if (this.PageVersions.Count  > 0)
            {
                if (this.PageVersions.Last().WebPageCheckedAsValid.Last() != null)
                {
                    LastDownload = this.PageVersions.Last().WebPageCheckedAsValid.Last();
                }
            }

            BrowserSession bs = new BrowserSession();
            var h = bs.CreateWebHit(this.Url);

            HttpResponseMessage HeaderResponseMessage;

            

            try
            {
                HeaderResponseMessage = await h.GetHeaders();
            }
            catch (Exception e)
            {
                this.Lastchecked = DateTime.Now;
                DownloadInProgress = false;
                
                return "Download failed on getting headers: " + e.Message;
            }

            if(this.Url != HeaderResponseMessage.RequestMessage.RequestUri.ToString())
            {
                this.OriginalUrl = this.Url;
                this.Url = HeaderResponseMessage.RequestMessage.RequestUri.ToString();
                
            }


            //If it wasn't sucessful
            if (!HeaderResponseMessage.IsSuccessStatusCode)
            {
                this.Lastchecked = DateTime.Now;
                DownloadInProgress = false;

                if(HeaderResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    WebPageDeleted = true;
                }
                if (HeaderResponseMessage.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    WebPageDeleted = true;
                }


                return "Download failed with status " + HeaderResponseMessage.StatusCode;
            }

            if(HeaderResponseMessage.Content.Headers.ContentType == null)
            {
                this.Lastchecked = DateTime.Now;
                DownloadInProgress = false;
                if (DownloadInProgress) { }
                return "Download failed - no content type";
            }


            //If it was sucessful, but not HTML
            if (HeaderResponseMessage.Content.Headers.ContentType.MediaType != "text/html")
            {
                this.WebPageIsNotHtml = true;
                this.Lastchecked = DateTime.Now;
                DownloadInProgress = false;
                return "Not HTML";
            }
            




            if (HeaderResponseMessage.Content.Headers.LastModified != null) 
            {
                DateTime modified = ((DateTimeOffset)HeaderResponseMessage.Content.Headers.LastModified).DateTime;


                if (modified < LastDownload)
                {
                    Lastchecked = DateTime.Now;
                    this.PageVersions.Last().WebPageCheckedAsValid.Add(DateTime.Now);
                    DownloadInProgress = false;
                    return "Page not modified from before (headers)";

                }
            }



            //At this point, we have the headers and the page needs updating.


            HttpResponseMessage response;


            try
            {
                response = await h.Call();
            }
            catch(Exception e)
            {
                this.Lastchecked = DateTime.Now;
                DownloadInProgress = false;
                return "Download failed while getting page:" + e.Message;
            }

            


            if (!response.IsSuccessStatusCode)
            {
                Lastchecked = DateTime.Now;
                DownloadInProgress = false;
                return "Headers OK, but page failed";
            }

            string html;
            try
            {

                if (response.Content.Headers.ContentEncoding.Contains("gzip"))
                {
                    using (var decompressionStream = new GZipStream(await response.Content.ReadAsStreamAsync(), CompressionMode.Decompress))
                    using (var reader = new StreamReader(decompressionStream))
                    {
                        html = await reader.ReadToEndAsync();
                    }
                }
                else if(response.Content.Headers.ContentEncoding.Contains("br"))
                {
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var brotliStream = new BrotliStream(stream, CompressionMode.Decompress))
                    using (var reader = new StreamReader(brotliStream, Encoding.UTF8))  // Assuming the text encoding is UTF-8
                    {
                        html = await reader.ReadToEndAsync();
                        // Now decompressedContent contains the decompressed HTML
                    }

                }
                else
                {
                    html = await response.Content.ReadAsStringAsync();
                }



            }
            catch (Exception)
            {
                Lastchecked = DateTime.Now;
                DownloadInProgress = false;
                return "Content error";
            }

            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            InsertBaseTag(doc);

            html = doc.DocumentNode.OuterHtml;

            var v = new WebPageVersion
            {
                WebPage = this
            };
            PageVersions.Add(v);
            

            Directory.CreateDirectory(FolderPath);
            System.IO.File.WriteAllText(v.HtmlFilePath, html);

            v.LoadLinkFreeInnerText();
            v.LoadInnerText();
            v.Sentences();


            //check the text inside the HTML against the previous version
            if (PageVersions.Count > 1)
            {
                var previousVersion = PageVersions[PageVersions.Count - 2];

                if (previousVersion.LoadInnerText() == v.LoadInnerText())
                {
                    previousVersion.WebPageCheckedAsValid.Add(DateTime.Now);
                    Lastchecked = DateTime.Now;
                    DownloadInProgress = false;

                    v.Delete();

                    return "Page not modified from before (html)";

                }

                //It's changed, so mark the previous version as having moved on.
                previousVersion.WebPageChangedFromThisVersion = DateTime.Now;
            }





            v.WebPageCheckedAsValid.Add(DateTime.Now);




            //If this is the first version, we need to compare it to the other pages on the site
            //by comparing the sentences.
            if(this.PageVersions.Count ==1)
            {
                //bool TooSimilarToAnotherPage = false;

                var SentencesSharedWith = v.SentencesAreSharedWith();


                //string similarTo = "";


                //if (false){
                //    var peers = FindLatestPeers();
                //    var DiffMatcher = new DiffMatchPatch.diff_match_patch();

                //    var linkFreeInnerT = v.LoadLinkFreeInnerText();
                //    var linkFreeInnerTLen = linkFreeInnerT.Length;
                    

                //    foreach (var peer in peers.OrderBy(x => Math.Abs(x.LoadLinkFreeInnerText().Length - linkFreeInnerTLen)))
                //    {
                //        var peerText = peer.LoadLinkFreeInnerText();

                //        var sizeRatio = (float)peerText.Length / (float)linkFreeInnerT.Length;


                //        if ((sizeRatio > 0.75) && (sizeRatio < 1.33))
                //        {


                //            var difference = DiffMatcher.diff_main(linkFreeInnerT, peerText);
                //            var diffScore = DiffMatcher.diff_levenshtein(difference);
                //            if (diffScore < 250)
                //            {
                //                TooSimilarToAnotherPage = true;
                //                Debug.WriteLine(this.Url + " is too similar (" + diffScore + ") to " + peer.WebPage.Url);

                //                similarTo = peer.WebPage.Url;
                //                break;
                //            }
                //        }
                //    }
                //}



                //if (TooSimilarToAnotherPage)
                //{
                //    this.IsTooSimilarToAnotherPage = true;
                //    v.Delete();
                //    return "Page is too similar to " + similarTo;
                //}

                if (SentencesSharedWith != null)
                {
                    this.IsTooSimilarToAnotherPage = true;
                    v.Delete();
                    return "Page is too similar to " + SentencesSharedWith.WebPage.Url;
                }

            }





            //Find links on the page and add them to the database.

            foreach (var link in GetLinks(doc))
            {
                Website.AddWebPage(link, this.UniqueId);
            }


            DownloadInProgress = false;

            

            return "ok";
        }


        public void Delete()
        {
            foreach(var v in PageVersions.ToList())
            {
                v.Delete();
            }

            this.Website.WebPages.Remove(this);

        }

        ///// <summary>
        ///// This page should be hidden from any search results for the search terms listed.
        ///// </summary>
        //public List<string> IgnoreSearchTerms = new List<string>();




    }


}
