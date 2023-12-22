using AngleSharp;
using DiffPlex.DiffBuilder;
using DiffPlex;
using HtmlAgilityPack;
using NReadability;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml.Serialization;
using ShysterWatch.MisleadingClaimFinder;
using System.Text;
using ShysterWatch.ComplaintGenerator;

namespace ShysterWatch
{
    public class WebPageVersion
    {
        [XmlIgnore]
        public WebPage WebPage;


        public string SavedHtml
        {
            get
            {
                return File.ReadAllText(HtmlFilePath);
            }
        }


        public List<AiDetectedMisleadingClaim> FalseClaims(int promptReferenceNumber)
        {
            {
                return GetAiClaimsResult(promptReferenceNumber)?.FunctionCallResponse?.misleadingClaims ?? new List<AiDetectedMisleadingClaim>();
            }
        }



        private bool? malwareFound = null;
        public bool MalwareFound
        {
            get
            {
                if (malwareFound == null)
                {
                    //If there is a webpage version that doesn't have a HTML file, that's because Windows Defender detected and deleted it.
                    if (!File.Exists(this.HtmlFilePath))
                    {
                        malwareFound = true;
                    }
                    else malwareFound = false;
                }
                return (bool)malwareFound;
            }
        }




        /// <summary>
        /// Returns true if there is no AiClaimsResult, or the response from ChatGPT failed
        /// </summary>
        /// <returns></returns>
        public bool RequiresProcessingByAi(int promptSpecificationVersion)
        {

                if (GetAiClaimsResult(promptSpecificationVersion) == null) return true;
                if (AiResponseFailed(promptSpecificationVersion))
                {
                    return true;
                }

                return false;
            
        }


        public bool AiResponseFailed(int promptSpecificationVersion)
        {
            //get
            {
                var aiCR = GetAiClaimsResult(promptSpecificationVersion);

                if (aiCR == null) return false;
                if ((aiCR.Success == false) && (aiCR.Reason.IndexOf("Response from ChatGPT failed") == 0))
                {
                    return true;
                }
                return false;
            }
        }


        private Dictionary<int,AiClaimsResult> _aiClaimsResult = new Dictionary<int, AiClaimsResult> ();
        public AiClaimsResult GetAiClaimsResult(int promptSpecificationVersion)
        {
           // get
            {
                if(!_aiClaimsResult.ContainsKey(promptSpecificationVersion))
                {
                    _aiClaimsResult[promptSpecificationVersion] = AiClaimsResult.Load(ChatGPT35ExtractedClaimsFilePath(promptSpecificationVersion));
                    
                }
                return _aiClaimsResult[promptSpecificationVersion];
            }
        }

        //public string ChatGPT35ExtractedClaimsFilename
        //{
        //    get
        //    {
        //        return Index + ".html.Gpt3ExtractedClaims.txt";
        //    }
        //}

        public string ChatGPT35ExtractedClaimsFilename(int promptReferenceNumber)
        {
            {
                return $"{Index}.html.Gpt3ExtractedClaims{promptReferenceNumber}.txt";
            }
        }

        public string ChatGPT35ExtractedClaimsFilePath(int promptReferenceNumber)
        {
            
            {
                return Path.Combine(WebPage.FolderPath, ChatGPT35ExtractedClaimsFilename(promptReferenceNumber));
            }
        }
       


        public string Filename
        {
            get
            {
                return Index + ".html";
            }
        }

        public string HtmlFilePath
        {
            get
            {
                return Path.Combine(WebPage.FolderPath, Filename);
            }
        }

        public string InnerTextFilePath
        {
            get
            {
                return Path.Combine(WebPage.FolderPath, Filename + ".txt");
            }
        }


        public string LinkFreeInnerTextFilePath
        {
            get
            {
                return Path.Combine(WebPage.FolderPath, Filename + ".linkfree.txt");
            }
        }

        //Loads the html off the disk
        public string LoadHtml()
        {
            var a = HtmlFilePath;
            if (!File.Exists(HtmlFilePath)) return "";
            try
            {
                return File.ReadAllText(HtmlFilePath);
            }
            catch
            {
                return "";
            }

        }

        public static string GetAndCleanInnerText(HtmlDocument doc)
        {
            var text = doc.DocumentNode.InnerText;
            //Remove big whitespaces
            text = text.Replace("\r", "");
            text = (new Regex(@"\n\s*\n")).Replace(text, "\n");
            text = (new Regex(@"\t+")).Replace(text, "\n");
            text = (new Regex(@" +")).Replace(text, " ");
            text = HttpUtility.HtmlDecode(text);

            return text;
        }


        public string LoadFullText()
        {
            if (!File.Exists(HtmlFilePath)) return "";

            var text = ExtractTextFromHtml(this.LoadHtml());


            return text;
        }

        public string LoadInnerText()
        {
            

            var fileLoc = InnerTextFilePath;

            var fi = new FileInfo(fileLoc);

            //This is when I improved the algorithm. It will overwrite files made before this.
            var minLastWriteTime = new DateTime(2023, 4, 24, 19, 15, 0);
            
            if ((fi.Exists) && (fi.LastWriteTime > minLastWriteTime)) return File.ReadAllText(fileLoc);
            

            if (!File.Exists(HtmlFilePath)) return "";


            var pageVersion = this;
            var pageVersionText = pageVersion.LoadFullText();

            WebPage comparisonPage;
            var indexOfThisPage = pageVersion.WebPage.Website.WebPages.IndexOf(pageVersion.WebPage);
            var numberOfPages = pageVersion.WebPage.Website.WebPages.Count();

            //if it's the first page.
            if (indexOfThisPage == 0)
            {
                if (numberOfPages > 1)
                {
                    comparisonPage = pageVersion.WebPage.Website.WebPages[1];
                }
                else return pageVersionText;
            }
            else
            {
                if (indexOfThisPage == 1)
                {
                    if (numberOfPages > 2)
                    {
                        comparisonPage = pageVersion.WebPage.Website.WebPages[2];
                        
                    }
                    else comparisonPage = pageVersion.WebPage.Website.WebPages[0];
                }
                else
                {
                    comparisonPage = pageVersion.WebPage.Website.WebPages[1];
                }
            }
            if (!Directory.Exists(comparisonPage.FolderPath)) return pageVersionText;

            WebPageVersion comparisonPageVersion;
            if (comparisonPage.PageVersions.Count() > pageVersion.Index) comparisonPageVersion = comparisonPage.PageVersions[pageVersion.Index];
            else
            {
                var a = comparisonPage;
                var b = comparisonPage.PageVersions;
                comparisonPageVersion = new WebPageVersion();
                try
                {
                    comparisonPageVersion = comparisonPage.PageVersions[comparisonPage.PageVersions.Count() - 1];
                }
                catch
                {
                }

            }
            var comparisonPageVersionText = comparisonPageVersion.LoadFullText();

            var differ = new Differ();
            var builder = new InlineDiffBuilder(differ);
            var diff = builder.BuildDiffModel(comparisonPageVersionText, pageVersionText);

            var uniqueText = "";
            foreach (var line in diff.Lines)
            {
                if ((line.Type == DiffPlex.DiffBuilder.Model.ChangeType.Modified) || (line.Type == DiffPlex.DiffBuilder.Model.ChangeType.Inserted))
                {
                    uniqueText += line.Text + "\r\n";
                }
            }






            File.WriteAllText(fileLoc, uniqueText);
            return uniqueText;
        }



        public static string ExtractTextFromHtml(string htmlString)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml((htmlString));

            return ReduceWhitespace(ExtractTextFromNode(htmlDoc.DocumentNode));
        }


        static string ExtractMainContent(string htmlContent)
        {

            try
            {

                var transcoder = new NReadabilityTranscoder();


                TranscodingInput ti = new TranscodingInput(htmlContent);

                var transcodedContent = transcoder.Transcode(ti);



                return transcodedContent.ExtractedContent;
            }
            catch
            {
                return htmlContent;
            }
        }


        static string ExtractTextFromNode(HtmlNode node)
        {
            if (node == null)
            {
                return string.Empty;
            }

            if (node.NodeType == HtmlNodeType.Text)
            {
                return ReduceWhitespace(HtmlEntity.DeEntitize(node.InnerText));
            }

            if (node.Name.Equals("script", StringComparison.OrdinalIgnoreCase) ||
                node.Name.Equals("style", StringComparison.OrdinalIgnoreCase))
            {
                return string.Empty;
            }

            string result = string.Empty;

            string[] tagNames = { "p", "div", "br" };

            foreach (HtmlNode childNode in node.ChildNodes)
            {

                if (tagNames.Contains(childNode.Name))
                {
                    result += "\r\n\r\n";
                }
                result += (" " + ExtractTextFromNode(childNode) + " ");

                if (tagNames.Contains(childNode.Name))
                {
                    result += "\r\n\r\n";
                }
            }

            return result;
        }

        public static string ReduceWhitespace(string input)
        {
            var sInput = input.Trim();
            var sOutput = sInput;
            var done = false;

            while (!done)
            {

                {
                    sOutput = sOutput.Replace("\t ", " ");
                }
                {
                    var regex = new Regex(@" {2,}");
                    sOutput = regex.Replace(sOutput, " ");
                }
                {
                    var regex = new Regex(@"\r\n +\r\n");
                    sOutput = regex.Replace(sOutput, "\r\n\r\n");
                }

                {
                    sOutput = sOutput.Replace("\r\n ", "\r\n");
                }
                {
                    sOutput = sOutput.Replace(" \r\n", "\r\n");
                }
                {
                    sOutput = sOutput.Replace("\r\n\r\n\r\n", "\r\n\r\n");
                }

                if (sOutput == sInput) done = true;
                sInput = sOutput;

            }

            return sOutput;
        }

        string LinkFreeInnerTextCache = "<b>";

        /// <summary>
        /// The innertext of the document once all of the links have been removed to hide nav
        /// </summary>
        /// <returns></returns>
        public string LoadLinkFreeInnerText()
        {



            //if (LinkFreeInnerTextCache != "<b>") return LinkFreeInnerTextCache;

            var fileLoc = LinkFreeInnerTextFilePath;

            if (File.Exists(fileLoc)) return File.ReadAllText(fileLoc);


            if (!File.Exists(HtmlFilePath)) return "";

            HtmlDocument doc = new HtmlDocument();

            try
            {
                doc.Load(HtmlFilePath, Encoding.UTF8);
            }
            catch(System.IO.IOException)
            {
                return "";
            }

            var links = doc.DocumentNode.SelectNodes("//a");

            if (links != null)
            {
                foreach (var a in links)
                {
                    a.ParentNode.RemoveChild(a);

                }
            }








            var text = ReduceWhitespace(ExtractTextFromNode(doc.DocumentNode));


            //var text = doc.DocumentNode.InnerText;
            //var titleTags = doc.DocumentNode.SelectNodes("//*[self::h1 or self::h2 or self::h3 or self::h4 or self::h5 or self::h6]");
            //foreach (var t in titleTags)
            //{
            //    t.InnerHtml = $"\n{t.InnerHtml}\n";
            //}

            ////Remove big whitespaces
            //text = text.Replace("\r", "");
            //text = (new Regex(@"\n\s*\n")).Replace(text, "\n");
            //text = (new Regex(@"\t+")).Replace(text, "\n");
            //text = (new Regex(@" +")).Replace(text, " ");
            //text = HttpUtility.HtmlDecode(text);


            File.WriteAllText(fileLoc, text);


            LinkFreeInnerTextCache = text;

            return text;

        }

        int index=-1;
        public int Index
        {
            get
            {
                if(index == -1)
                {
                    index = WebPage.PageVersions.IndexOf(this);
                }

                return index;
            }
            set
            {
                index = value;
            }
        }



        public List<DateTime> WebPageCheckedAsValid = new List<DateTime>();
        public DateTime? WebPageChangedFromThisVersion = null;

        public void Delete()
        {
            
            if(File.Exists(this.LinkFreeInnerTextFilePath)) File.Delete(this.LinkFreeInnerTextFilePath);
            if (File.Exists(this.InnerTextFilePath)) File.Delete(this.InnerTextFilePath);
            if (File.Exists(this.HtmlFilePath)) File.Delete(this.HtmlFilePath);
            if (File.Exists(this.SentencesFilePath)) File.Delete(this.SentencesFilePath);

            DirectoryInfo dir = new DirectoryInfo(this.WebPage.FolderPath);

            if (dir.Exists)
            {

                if (dir.GetFiles().Length == 0) Directory.Delete(this.WebPage.FolderPath);
            }


            this.WebPage.PageVersions.Remove(this);



        }

        ///// <summary>
        ///// This page version should be hidden from any search results for the search terms listed.
        ///// </summary>
        //public List<string> IgnoreSearchTerms = new List<string>();

        ///// <summary>
        ///// This page version should be flagged as concerning for the search terms listed
        ///// </summary>
        //public List<string> ConcernedWithSearchTerms = new List<string>();

        public string VersionDateSummary()
        {
            var s = "Checked valid on:\r\n";
            foreach(var d in this.WebPageCheckedAsValid)
            {
                s += "  " + d.ToShortDateString() + " " + d.ToShortTimeString() + "\r\n";
            }

            if (this.WebPageChangedFromThisVersion == null) s += "Up to date on last check";
            else s += "This file had changed when we checked on " + ((DateTime)WebPageChangedFromThisVersion).ToShortDateString() + " " + ((DateTime)WebPageChangedFromThisVersion).ToShortTimeString() + "\r\n";

            return s;
        }

        public WebPageVersion SentencesAreSharedWith()
        {

            foreach(var otherPage in this.WebPage.Website.WebPages.Where(x => x.PageVersions.Count > 0).Where(x => x != this.WebPage))
            {
                var latestV = otherPage.PageVersions.Last();

                bool differenceFound = false;

                if (this.Sentences().Length != latestV.Sentences().Length)
                {
                    differenceFound = true;
                }
                else
                {
                    for (int i = 0; i < this.Sentences().Length; i++)
                    {
                        if (this.Sentences()[i] != latestV.Sentences()[i])
                        {
                            differenceFound = true;
                            break;
                        }
                    }
                }
                if (!differenceFound) return latestV;

            }

            return null;
        }


        public string SentencesFilePath
        {
            get
            {
                return Path.Combine(WebPage.FolderPath, Index + ".sentences.txt");
            }
        }
        string[] sentences = null;
        public string[] Sentences()
        {
            if(sentences == null)
            {
                var path = SentencesFilePath;
                if (!File.Exists(path))
                {
                    sentences = FindSentences(this.LoadHtml());
                    var strSentences = "";
                    foreach(var sent in sentences)
                    {
                        strSentences += sent;
                        if (sent != sentences.Last()) strSentences += "\r\n";
                    }
                    Directory.CreateDirectory(this.WebPage.FolderPath);
                    File.WriteAllText(path, strSentences);
                }
                else
                {
                    var strSentences = "";
                    if (File.Exists(path))  strSentences = File.ReadAllText(path);
                    sentences = strSentences.Split('\r').Select(x => x.Trim()).ToArray();
                    
                }


            }
            return sentences;
        }

        /// <summary>
        /// Takes HTML, removes the links, and extracts the sentences of 25 letter or more.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        static string[] FindSentences(string html)
        {

            var h1 = new HtmlAgilityPack.HtmlDocument();
            h1.LoadHtml(html);

            var body = h1.DocumentNode.SelectSingleNode("//body");
            if (body == null) body = h1.DocumentNode;

            var links = body.SelectNodes("//a");
            if (links != null)
            {
                for (int l = links.Count - 1; l >= 0; l--)
                {
                    links[l].ParentNode.RemoveChild(links[l]);
                }
            }

            var s = HttpUtility.HtmlDecode(body.InnerText);

            s = s.Replace("\r", "");

            var lines = s.Split('\n').ToList();

            var AnyWordChar = new Regex(@"\w");
            var MultilpleWhiteSpaceChars = new Regex(@"\W+");

            for (int i = lines.Count - 1; i >= 0; i--)
            {

                if (!AnyWordChar.Match(lines[i]).Success)
                {
                    lines.RemoveAt(i);
                }
                else
                {
                    lines[i] = lines[i].Trim();
                    MultilpleWhiteSpaceChars.Replace(lines[i], " ");

                    if (lines[i].IndexOf(".") != -1)
                    {
                        var sublines = lines[i].Split('.');
                        lines.RemoveAt(i);
                        for (int sli = sublines.Length - 1; sli >= 0; sli--)
                        {
                            if (AnyWordChar.Match(sublines[sli]).Success)
                            {
                                lines.Insert(i, sublines[sli].Trim());
                            }

                        }
                    }

                }
            }

            for (int i = lines.Count - 1; i >= 0; i--)
            {
                if (lines[i].Length < 25) lines.RemoveAt(i);
            }

            return lines.ToArray();
        }

    }


}
