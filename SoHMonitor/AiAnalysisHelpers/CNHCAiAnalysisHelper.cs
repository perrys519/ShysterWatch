using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenQA.Selenium.DevTools.V94.Storage;
using ShysterWatch.MembershipDatabases.CNHC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;

namespace ShysterWatch.AiAnalysisHelpers
{
    public class CNHCAiAnalysisHelper : AiAnalysisHelper
    {

        public class WebPageAndClaims
        {
            public string Url;
            public DateTime Downloaded;
            public List<MisleadingClaimFinder.AiDetectedMisleadingClaim> FalseClaims = new List<MisleadingClaimFinder.AiDetectedMisleadingClaim>();
        }

        public class WebsiteInfo
        {
            public string SourceUrl;
            public List<CNHCMember> Registrants = new List<CNHCMember>();
            public List<WebPageAndClaims> WebPages = new List<WebPageAndClaims>();
        }

        public override async Task<bool> ExportData(SendMessage messageDelegate)
        {
            await Task.Run(() => {

                messageDelegate("Exporting Data..");
                var outputPath = Path.Combine(Sys.CurrentGroup.FolderPath, "dataExport.js");



                var relevantClinics = ((CNHCMemberDatabase)Sys.CurrentGroup.MembershipDb).RelevantClinics;



                List<WebsiteInfo> websiteInfoList = new List<WebsiteInfo>();
                foreach (var c in relevantClinics)
                {
                    if (c.WebsiteObj != null)
                    {
                        var wsi = new WebsiteInfo
                        {
                            SourceUrl = c.WebsiteObj.SourceUrl
                        };

                        foreach (var member in c.Members)
                        {
                            if (member.IsRelevant)
                            {
                                wsi.Registrants.Add(member);
                            }
                        }

                        List<WebPageAndClaims> webPages = new List<WebPageAndClaims>();

                        foreach (var p in c.WebsiteObj.WebPages)
                        {
                            if (p.ActiveVersion != null)
                            {
                                if (p.ActiveVersion.GetAiClaimsResult(PromptSpecification.PromptReferenceNumber) != null)
                                {
                                    if (p.ActiveVersion.GetAiClaimsResult(PromptSpecification.PromptReferenceNumber).FunctionCallResponse?.misleadingClaims != null)
                                    {
                                        WebPageAndClaims wpc = new WebPageAndClaims
                                        {
                                            Url = p.Url,
                                            FalseClaims = p.ActiveVersion.GetAiClaimsResult(PromptSpecification.PromptReferenceNumber).FunctionCallResponse.misleadingClaims
                                        };
                                        webPages.Add(wpc);
                                    }
                                }
                            }
                        }

                        if (webPages.Count > 0)
                        {
                            wsi.WebPages = webPages;
                            websiteInfoList.Add(wsi);
                        }

                    }



                }

                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new CustomContractResolver(),
                    Formatting = Formatting.Indented
                };

                using (StreamWriter file = File.CreateText(outputPath))
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    JsonSerializer serializer = JsonSerializer.Create(settings); //new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(writer, websiteInfoList);
                }


                messageDelegate($"Done! Saved to {outputPath}");


                OutputHtmlReport(websiteInfoList, messageDelegate);
            });


            return true;
        }


        public void OutputHtmlReport(List<WebsiteInfo> websiteInfoList, SendMessage messageDelegate)
        {
            messageDelegate("Exporting to HTML report..");
            var outputPath = Path.Combine(Sys.CurrentGroup.FolderPath, "dataExport.html");

            StreamWriter w = new StreamWriter(outputPath);

            w.Write($@"
<html>
<head>
<style>

	*{{
		font-family: Arial;
		font-size: 10pt;
	}}


    h1{{
		font-size: 14pt;
       color:#256EA3;
    }}
    h2{{
		font-size: 12pt;
       color:#256EA3;
    }}
	body:{{
        background-color: white;    
    }}
	thead td{{
		background-color: #DFA693;
		width: 50%;
	}}
	td{{
        vertical-align:top;
		padding: 2px;
		page-break-inside: avoid;
	}}

    div.break{{
        dpage-break-after:always; 
    counter-reset: page 1; /* This sets the first page number to 0 */

                }}	

    td.even{{
        background-color: #eeeeee;
    }}

	td.registrantInfo{{
		background-color: lightgreen ;
        page-break-after: avoid;
	}}
	td.urlInfo{{
		background-color:#A5D3EB;
        page-break-after: avoid;
	}}
	td.claimInfo{{
	}}

table {{
    width: 100%; /* Adjust as needed */
}}
td{{
	}}
.page-link:after {{
  content: """" target-counter(attr(href), page);
}}
a{{
  text-decoration:none;
  color:#000;
}}

a.visibleLink{{
  text-decoration:underline;
  color:#256EA3;
}}

@page {{margin-bottom: 10mm; 
}}

@page {{
    @bottom-center {{
        content: """" counter(page);
    }}
}}
div.index{{
    column-count: 2; /* Adjust the number of columns as needed */
    column-gap: 10px; /* Adjust the gap size as needed */
}}
</style>
</head>
<body>
<p>First page</p>
<div class=break></div>
<h1>CNHC Registrant Website Claims</h1>
<p>This report details a list of AI detected claims and the reasoning given by the AI for selecting them. The AI was asked to look for false and misleading claims related to the effectiveness of treatments in curing, treating, or managing illnesses or diseases.</p>
<p>Printing this out is not a good idea.</p>
<h2>Index by Name</h2>
<div class=index>
");
            var registrants = websiteInfoList.SelectMany(x => x.Registrants).Distinct().OrderBy(x => x.Name).ToList();

            var isFirst = true;
            foreach (var r in registrants)
            {
                if (!isFirst) w.Write("<br/>");
                w.Write($@"
<a  class=""visibleLink"" href=""#{HttpUtility.HtmlAttributeEncode(r.MembershipNumber)}"">{HttpUtility.HtmlEncode(r.MembershipNumber)}</a> {HttpUtility.HtmlEncode(r.Name)} <a href=""#{HttpUtility.HtmlAttributeEncode(r.MembershipNumber)}"" class=""page-link"">p</a>
");
                isFirst = false;

            }


            w.Write($@"
</div>
<h2 style=""page-break-before:always;"">Found Claims and Explanations</h2>
<table>

<thead>
	<tr>
		<td>Found Claim</td>
		<td>AI Explanation</td>
</thead>
<tbody>
");




            foreach (WebsiteInfo webPage in websiteInfoList)
            {
                var totalClaims = webPage.WebPages.Sum(wp => wp.FalseClaims.Count);

                w.Write($@"
<tr>
    <td colspan=2 class=registrantInfo>");


                var isFirstRegistrant = true;
                foreach (var r in webPage.Registrants)
                {
                    if (!isFirstRegistrant) w.Write("<br />");
                    w.Write($@"
<a id=""{HttpUtility.HtmlEncode(r.MembershipNumber)}"" name=""{HttpUtility.HtmlEncode(r.MembershipNumber)}""></a>{HttpUtility.HtmlEncode(r.MembershipNumber)} : {HttpUtility.HtmlEncode(r.Name)}
");

                    isFirstRegistrant = false;
                }
                w.Write($@"
<br />{totalClaims} Found Claims
    </td>
</tr>
");

                foreach (var wp in webPage.WebPages)
                {
                    w.Write($@"
<tr>
    <td class=urlInfo colspan=2><a href=""{HttpUtility.HtmlAttributeEncode(wp.Url)}"" target=_blank>{HttpUtility.HtmlEncode(wp.Url)}</a></td>
</tr>");
                    var evenClass = "";
                    foreach (var claim in wp.FalseClaims)
                    {
                        w.Write($@"
<tr>
    <td class='claimInfo {evenClass}'>{HttpUtility.HtmlEncode(claim.claim)}</a></td>
    <td class='claimInfo {evenClass}'>{HttpUtility.HtmlEncode(claim.reasoning)}</a></td>
</tr>");

                        if (evenClass == "") evenClass = "even";
                        else evenClass = "";
                    }


                }





            }


            w.Write($@"
</tbody>
</table>
</body>
</html>");

            w.Close();


            messageDelegate($"Done! Saved to {outputPath}");
        }


        public override List<Website> GetWebsiteObjectsForAiProcessing() {

            var members = ((CNHCMemberDatabase)Sys.CurrentGroup.MembershipDb).RelevantMembers;
            var clinics = ((CNHCMemberDatabase)Sys.CurrentGroup.MembershipDb).RelevantClinics;

            var websiteObjects = clinics.Where(x => x.Website != null).Select(c => c.WebsiteObj).Where(x => x != null).Distinct().ToList();

            return websiteObjects;
        }

        private List<Website> _relevantWebsites = null;
        public override List<Website> RelevantWebsites
        {
            get
            {
                if (_relevantWebsites == null)
                {
                    var cnhcDb = ((CNHCMemberDatabase)Sys.CurrentGroup.MembershipDb);

                    var relevantClinics = cnhcDb.RelevantClinics;

                    _relevantWebsites = relevantClinics.Where(x => x.Website != null).Select(c => c.WebsiteObj).Where(x => x != null).Distinct().ToList();
                }

                return _relevantWebsites;
            }
        }


        public override void CreateReport(SendMessage messageDelegate)
        {
            string reportFilePath = Path.Combine(ShysterWatch.Sys.CurrentGroup.FolderPath, @"report.html");

            //Tmp_ErrorFind();
            //return;

            messageDelegate("Started Report");







            var cnhcDb = ((CNHCMemberDatabase)Sys.CurrentGroup.MembershipDb);


            var relevantClinics = cnhcDb.RelevantClinics;




            decimal costEstimateToComplete = 0;
            int tokenEstimateToComplete = 0;
            if (AssessedWebPages.Count() > 0)
            {
                costEstimateToComplete = (decimal)TotalCostSoFar / (decimal)AssessedWebPages.Count() * (decimal)StudyWebPages.Count();
                tokenEstimateToComplete = (int)Math.Round((double)TotalTokensSoFar / (double)AssessedWebPages.Count() * (double)StudyWebPages.Count());

            }












            double histogramGroupSize = 5;
            var webpageNoOfClaimsHistogram = WebsitesAndTotalClaims.GroupBy(h => (int)Math.Ceiling((double)h.total/histogramGroupSize)).Select(g => (claimGroup: g.Key, numberOfWebsites: g.Count(), groupFrom:((g.Key-1)*histogramGroupSize)+1, groupTo: g.Key*histogramGroupSize)).OrderBy(h => h.claimGroup);


            var proportionOfRelevantContentWebsitesWithFalseClaims = (double)RelevantWebsitesWithFalseClaims.Count() / (double)RelevantContentWebsites.Count();


            var mostWooPageVersions = RelevantWebPageVersions.OrderByDescending(x => x.FalseClaims(PromptSpecification.PromptReferenceNumber).Count).Take(20);

            var totalClaims = RelevantWebPageVersionsWithFalseClaims.Select(wpv => wpv.FalseClaims(PromptSpecification.PromptReferenceNumber).Count()).Sum();

            var totalClaimsPerRelevantPage = (double)totalClaims / (double)RelevantWebPageVersions.Count();

            var totalClaimsPerRelevantContentWebsite = (double)totalClaims / (double)RelevantContentWebsites.Count();


            messageDelegate("Generated stats");
            var webPagesHtml = "";
            webPagesHtml = $@"<html>
    <head>
        <style type=text/css>
/* Reset some default browser styles */
body, h1, h2, ul, li {{
  margin: 0;
  padding: 0;
}}

body {{
  font-family: Arial, sans-serif;
  font-size: 16px;
  line-height: 1.6;
  color: #333;
  background-color: #fff;
}}

td{{
    vertical-align:top;
}}
tr.even-False{{
    background-color: #ddd;
}}
td.tableTitle{{
    background-color:lightblue;
}}
thead tr td{{
    background-color: darkgray;
    color:white;
}}


/* Container for report content */
.container {{
  max-width: 800px;
  margin: 20px auto;
  padding: 20px;
  background-color: #fff;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
}}

/* Typography */
h1, h2 {{
  font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif;
  font-weight: bold;
  line-height: 1.2;
}}

h1 {{
  font-size: 1.5rem;
  margin-bottom: 20px;
}}

h2 {{
  font-size: 1.2rem;
  margin-top: 10px;
  margin-bottom: 10px;
}}

ul {{
  list-style-type: none;
  padding-left: 20px;
}}

li {{
  margin-bottom: 10px;
}}
*{{
    font-size:10pt;
}}
a {{
  color: #0070f3;
  text-decoration: none;
}}

a:hover {{
  text-decoration: underline;
}}

i{{text - decoration:underline;
	color: #833C0B;
}}
        </style>
    </head>
    <body>
";

            File.WriteAllText(reportFilePath, webPagesHtml);
            webPagesHtml = "";


            webPagesHtml += $@"
<h1>Summary</h1>

<table cellspacing=0>
    <tr>
        <td>Complementary and Natural Healthcare Council (CNHC) registrants:</td>
        <td>6217</td>
    </tr>
    <tr>
        <td>CNHC registrants found during web scrape:</td>
        <td>{cnhcDb.Members.Count}</td>
    </tr>
    <tr>
        <td>Registrants found with relevant disciplines (<i>Relevant Registrants</i>):</td>
        <td>{cnhcDb.RelevantMembers.Count}</td>
    </tr>
    <tr>
        <td>Clinics with which CNHC registrants were associated, found during web scrape (<i>Found Clinics</i>):</td>
        <td>{cnhcDb.Clinics.Count}</td>
    </tr>
    <tr>
        <td><i>Found Clinics</i> who have at least one <i>Relevant Registrant</i> (<i>Relevant Clinics</i>):</td>
        <td>{relevantClinics.Count}</td>
    </tr>
    <tr>
        <td><i>Relevant Clinics</i> which list Web Sites:</td>
        <td>{relevantClinics.Where(c => c.ListedWebsite != "").Count()}</td>
    </tr>
    <tr>
        <td><i>Relevant Clinics</i> with valid Web Sites URLs:</td>
        <td>{relevantClinics.Where(c => c.Website != "").Count()}</td>
    </tr>

    <tr>
        <td>Distinct, Valid Websites from <i>Relevant Clinics</i> (<i>Distinct Websites</i>):</td>
        <td>{RelevantWebsites.Count()}</td>
    </tr>
    <tr>
        <td><i>Distinct Websites</i> where at least one page was able to be downloaded (<i>Operating Websites</i>):</td>
        <td>{OperatingWebsites.Count()}</td>
    </tr>
    <tr>
        <td>Web Pages downloaded from <i>Operating Websites</i> (<i>Downloaded Web Pages</i>):</td>
        <td>{DownloadedWebPages.Count()}</td>
    </tr>

    <tr>
        <td><i>Downloaded Web Pages</i> with malware detected:</td>
        <td>{DownloadedWebPagesWithMalwareDetected.Count()}</td>
    </tr>

    <tr>
        <td><i>Downloaded Web Pages</i> without malware found (<i>Web Pages</i>):</td>
        <td>{DownloadedWebPagesWithoutMalwareDetected.Count()}</td>
    </tr>


    <tr>
        <td><i>Web Pages</i> once limited to 30 per website (<i>Study Web Pages</i>):</td>
        <td>{StudyWebPages.Count()}</td>
    </tr>
    <tr>
        <td><i>Study Web Pages</i> not assessed by AI because first five pages contained irrelevant content:</td>
        <td>{StudyWebPages.Count() - AssessedWebPages.Count()}</td>
    </tr>
    <tr>
        <td><i>Study Web Pages</i> assessed by AI (<i>Assessed Web Pages</i>):</td>
        <td>{AssessedWebPages.Count()}</td>
    </tr>
    <tr>
        <td><i>Study Web Pages</i> assessed by AI as having irrelevant content:</td>
        <td>{IrrelevantWebPageVersions.Count()}</td>
    </tr>
    <tr>
        <td><i>Study Web Pages</i> assessed by AI as having relevant content (<i>Relevant Pages</i>):</td>
        <td>{RelevantWebPageVersions.Count()}</td>
    </tr>
    <tr>
        <td><i>Relevant Pages</i> with no <i>Found Claims</i>:</td>
        <td>{RelevantWebPageVersionsWithoutFalseClaims.Count()}</td>
    </tr>
    <tr>
        <td><i>Relevant Pages</i> with deserialization problems:</td>
        <td>{RelevantWebPageVersionsWithDeserializationProblems.Count()}</td>
    </tr>


    <tr>
        <td><i>Relevant Pages</i> with <i>Found Claims</i>:</td>
        <td>{RelevantWebPageVersionsWithFalseClaims.Count()}</td>
    </tr>
    <tr>
        <td>% of <i>Relevant Pages</i> with <i>Found Claims</i>:</td>
        <td>{Math.Round((double)RelevantWebPageVersionsWithFalseClaims.Count() / (double)RelevantWebPageVersions.Count() * (double)100)}%</td>
    </tr>
    <tr>
        <td>Total number of <i>Found Claims</i>:</td>
        <td>{totalClaims}</td>
    </tr>
    <tr>
        <td>Average number of <i>Found Claims</i> per <i>Relevant Page</i>:</td>
        <td>{Math.Round(totalClaimsPerRelevantPage, 3)}</td>
    </tr>

    <tr>
        <td><i>Operating Websites</i> that have relevant content (<i>Relevant Websites</i>)</td>
        <td>{RelevantContentWebsites.Count()}</td>
    </tr>

    <tr>
        <td>Average number of <i>Found Claims</i> per <i>Relevant Website</i>:</td>
        <td>{Math.Round(totalClaimsPerRelevantContentWebsite, 3)}</td>
    </tr>
    <tr>
        <td><i>Relevant Websites</i> with <i>Found Claims</i></td>
        <td>{RelevantWebsitesWithFalseClaims.Count()}</td>
    </tr>


    <tr>
        <td>Proportion of <i>Relevant Websites</i> that have <i>Found Claims</i>:</td>
        <td>{Math.Round(proportionOfRelevantContentWebsitesWithFalseClaims * 100)}%</td>
    </tr>


</table>

<h2>Sandkey Diagrams</h2>
<p>Data for https://sankeymatic.com/build/</p>
<pre>
Found Clinics [{IrrelevantContentWebsites.Count()}] Irrelevant Clinics
Found Clinics [{relevantClinics.Count()}] Relevant Clinics
Relevant Clinics [{relevantClinics.Where(c => c.Website == "").Count()}] Invalid URLs
Relevant Clinics [{relevantClinics.Where(c => c.Website != "").Count()}] Valid Urls
Valid Urls [{relevantClinics.Where(c => c.Website != "").Count() - RelevantWebsites.Count()}] Duplicate Websites
Valid Urls [{RelevantWebsites.Count()}] Distinct Websites
Distinct Websites [{RelevantWebsites.Count() - OperatingWebsites.Count()}] Non Operating Websites
Distinct Websites [{OperatingWebsites.Count()}] Operating Websites
Operating Websites [{OperatingWebsites.Count() - RelevantContentWebsites.Count()}] Irrelevant Websites
Operating Websites [{RelevantContentWebsites.Count()}] Relevant Websites
Relevant Websites [{RelevantWebsitesWithFalseClaims.Count()}] Relevant Websites with Found Claims
Relevant Websites [{RelevantContentWebsites.Count()- RelevantWebsitesWithFalseClaims.Count()}] Relevant Websites without Found Claims
</pre>
<p>....</p>

<pre>
Downloaded Web Pages [{DownloadedWebPagesWithMalwareDetected.Count()}] Malware Detected
Downloaded Web Pages [{DownloadedWebPagesWithoutMalwareDetected.Count()}] Web Pages
Web Pages [{DownloadedWebPagesWithoutMalwareDetected.Count() - StudyWebPages.Count()}] Excess Pages
Web Pages [{StudyWebPages.Count()}] Study Web Pages
Study Web Pages [{StudyWebPages.Count() - AssessedWebPages.Count()}] Pages not assessed due to 5 pages of irrelevant content
Study Web Pages [{AssessedWebPages.Count()}] Assessed Web Pages
Assessed Web Pages [{IrrelevantWebPageVersions.Count()}] Irrelevant Pages
Assessed Web Pages [{RelevantWebPageVersions.Count()}] Relevant Pages
Relevant Pages [{RelevantWebPageVersionsWithoutFalseClaims.Count()}] Without Found Claims
Relevant Pages [{RelevantWebPageVersionsWithFalseClaims.Count()}] With Found Claims

</pre>

";

            File.AppendAllText(reportFilePath, webPagesHtml);


            webPagesHtml = "";
            webPagesHtml += @"
<h2>Number of <i>Found Claims</i> histogram</h2>
<pre># Found Claims	# Websites
";
            foreach(var (claimGroup, numberOfWebsites, groupFrom, groupTo) in webpageNoOfClaimsHistogram)
            {
                webPagesHtml += $@"" + (groupFrom < 0 ? "" : groupFrom + "-") + $@"{groupTo}	{numberOfWebsites}
";
            }
            webPagesHtml += @"
</pre>";

            File.AppendAllText(reportFilePath, webPagesHtml);
  

            webPagesHtml = "";

            messageDelegate("Generated stats table");

            webPagesHtml += $@"


    <ul>
        <li>{AssessedWebPageVersions.Count()} web pages were inspected.</li>
        <li>${TotalCostSoFar} spent so far.</li>


        <li>Estimate ${Math.Round((double)costEstimateToComplete, 2)}  / {tokenEstimateToComplete} token for all {StudyWebPages.Count()} web pages.
    </ul>

";

            File.AppendAllText(reportFilePath, webPagesHtml);
            webPagesHtml = "";

            messageDelegate("Generated predictions list");

            webPagesHtml += $@"
<h1>Websites that AI flagged as having irrelevant content</h1>
<table>
";
            int count = 1;
            foreach (var w in IrrelevantContentWebsites)
            {
                webPagesHtml += $@"
    <tr>
        <td>{count}</td>
        <td><a href='{HttpUtility.HtmlAttributeEncode(w.SourceUrl)}' target='_blank'>{HttpUtility.HtmlEncode(w.SourceUrl)}</td>
        <td>{HttpUtility.HtmlEncode(w.UniqueId)}</td>
    </tr>
";
                count++;
            }


            webPagesHtml += $@"
</table>
";


            File.AppendAllText(reportFilePath, webPagesHtml);
            webPagesHtml = "";
            messageDelegate("Generated predictions list");

            


            webPagesHtml += $@"

<h1>Highest Number of Claims per Wesbite</h1>
<table cellpadding=0 cellspacing=0>
    <thead>
    <tr>
        <td>Url</td>
        <td># of claims</td>
    </tr>
    </thead>
    <tbody>
";
            foreach (var (total, website) in WebsitesAndTotalClaims.OrderByDescending(x => x.total).Take(20)) 
            {

                webPagesHtml += $@"
    <tr>
        <td><a href=""{HttpUtility.HtmlAttributeEncode(website.SourceUrl)}"" target=misleadingClaimsPage>{System.Net.WebUtility.HtmlEncode(website.SourceUrl)}</a></td>
        <td>{total}</td>
    </tr>
";

            }

            webPagesHtml += $@"
    </tbody>
</table>
";

            File.AppendAllText(reportFilePath, webPagesHtml);
            webPagesHtml = "";

            messageDelegate("Generated highest claims per website table");



            webPagesHtml += $@"

<h1>Highest Number of Claims per Page</h1>
<table cellpadding=0 cellspacing=0>
    <thead>
    <tr>
        <td>Url</td>
        <td># of claims</td>
    </tr>
    </thead>
    <tbody>
";
            foreach (var pv in mostWooPageVersions)
            {

                webPagesHtml += $@"
    <tr>
        <td><a href=""{pv.WebPage.Url}"" target=misleadingClaimsPage>{System.Net.WebUtility.HtmlEncode(pv.WebPage.Url)}</a></td>
        <td>{pv.GetAiClaimsResult(2).FunctionCallResponse.misleadingClaims.Count}</td>
    </tr>
";

            }

            webPagesHtml += $@"
    </tbody>
</table>
";

            File.AppendAllText(reportFilePath, webPagesHtml);
            webPagesHtml = "";

            messageDelegate("Generated highest claims per page table");






            webPagesHtml += $@"



<h1>By Relevant Discipline</h1>
<table cellpadding=0 cellspacing=0>
    <thead>
    <tr>
        <td>Discipline</td>
        <td># Practitioners</td>
        <td># Clinics</td>
        <td># Downloaded Websites</td>
        <td># Downloaded Webpages</td>
        <td># Irrelevant Pages</td>
        <td># Relevant Pages</td>
        <td># Relevant Pages without Claims</td>
        <td># Relevant Pages with Claims</td>
        <td># Claims</td>
        <td># Claims per page</td>
        <td># Claims per downloaded website</td>
        <td>% of Relevant Content Websites with <i>Found Claims</i></td>
    </tr>
    </thead>
    <tbody>
";

            var Disciplines = cnhcDb.Members.SelectMany(m => m.Disciplines).Distinct().OrderBy(x => x).ToList();
            var RelevantDisciplines = Disciplines.Where(x => !CNHCMember.IrrelevantDisciplines.Contains(x)).Distinct().OrderBy(x => x).ToList();


            foreach (var disc in RelevantDisciplines)
            {

                messageDelegate("In loop for " + disc);

                var discClinics = relevantClinics.Where(c => c.Disciplines.Contains(disc));
                var discMembers = cnhcDb.RelevantMembers.Where(c => c.Disciplines.Contains(disc));


                var websitesWhereAtLeastOnePageDownloadedHash = new HashSet<Website>(OperatingWebsites);
                var discWebsites = discClinics.Select(c => c.WebsiteObj).Where(w => websitesWhereAtLeastOnePageDownloadedHash.Contains(w));

                var websitesWithRelevantContentHash = new HashSet<Website>(RelevantContentWebsites);
                var discRelevantContentWebsites = discWebsites.Where(w => websitesWithRelevantContentHash.Contains(w));

                var discWebsitesHash = new HashSet<Website>(discWebsites);
                var discWebPages = DownloadedWebPages.Where(p => discWebsitesHash.Contains(p.Website)).ToList();

                //
                var discWebPagesHash = new HashSet<WebPage>(discWebPages);
                var discIrrelevantWebPageVersions = IrrelevantWebPageVersions.Where(pv => discWebPagesHash.Contains(pv.WebPage));
                var discRelevantWebPageVersions = RelevantWebPageVersions.Where(pv => discWebPagesHash.Contains(pv.WebPage));

                //
                var discRelevantWebPageVersionsWithoutClaims = RelevantWebPageVersionsWithoutFalseClaims.Where(pv => discWebPagesHash.Contains(pv.WebPage));

                var discRelevantWebPageVersionsWithClaims = RelevantWebPageVersionsWithFalseClaims.Where(pv => discWebPagesHash.Contains(pv.WebPage)).ToList();


                var discTotalClaims = discRelevantWebPageVersionsWithClaims.Select(wpv => wpv.GetAiClaimsResult(PromptSpecification.PromptReferenceNumber)?.FunctionCallResponse?.misleadingClaims?.Count()).Sum();

                var discTotalClaimsPerRelevantPage = (double)discTotalClaims / (double)discRelevantWebPageVersions.Count();
                var discTotalClaimsPerDownloadedWebsite = (double)discTotalClaims / (double)discWebsites.Count();

                var discWebsitesWithClaims = discRelevantWebPageVersionsWithClaims.Select(pv => pv.WebPage.Website).Distinct();

                webPagesHtml += $@"
    <tr>
        <td>{disc}</td>
        <td>{discMembers.Count()}</td>
        <td>{discClinics.Count()}</td>
        <td>{discWebsites.Count()}</td>
        <td>{discWebPages.Count()}</td>
        <td>{discIrrelevantWebPageVersions.Count()}</td>
        <td>{discRelevantWebPageVersions.Count()}</td>
        <td>{discRelevantWebPageVersionsWithoutClaims.Count()}</td>
        <td>{discRelevantWebPageVersionsWithClaims.Count()}</td>
        <td>{discTotalClaims}</td>
        <td>{(double.IsNaN(discTotalClaimsPerRelevantPage) ? "-" : Math.Round(discTotalClaimsPerRelevantPage, 2).ToString())}</td>
        <td>{(double.IsNaN(discTotalClaimsPerDownloadedWebsite) ? "-" : Math.Round(discTotalClaimsPerDownloadedWebsite, 2).ToString())}</td>
        <td>{discWebsitesWithClaims.Count()} / {discRelevantContentWebsites.Count()}<br/>{Math.Round((double)100 * (double)discWebsitesWithClaims.Count() / (double)discRelevantContentWebsites.Count(), 0)}%</td>
    </tr>
";
            }



            webPagesHtml += $@"
    </tbody>
</table>";
            File.AppendAllText(reportFilePath, webPagesHtml);
            webPagesHtml = "";

            messageDelegate("Generated by discipline table");

            return;
#pragma warning disable CS0162 // Unreachable code detected
            webPagesHtml += $@"



<h1>Claims</h1>
<table>
    <thead>
        <tr>
            <td>Url</td>
            <td>Claim</td>
            <td>Reasoning</td>
        </tr>
    </thead>
    <tbody>
";
#pragma warning restore CS0162 // Unreachable code detected

            foreach (var website in RelevantWebsitesWithFalseClaims)
            {
                //find clinic
                //list relevant practitioners
                var wsClinics = relevantClinics.Where(c => c.Website == website.SourceUrl);
                List<CNHCMember> wsMembers = new List<CNHCMember>();
                foreach (var wsClinic in wsClinics)
                {
                    foreach (var m in wsClinic.Members)
                    {
                        if (m.IsRelevant)
                        {
                            wsMembers.Add(m);
                        }
                    }
                }

                if (wsMembers.Count > 0)
                {
                    webPagesHtml += $@"
    <tr>
        <td colspan=4>
";
                    var isFirst = true;
                    foreach (var m in wsMembers)
                    {
                        if (!isFirst) webPagesHtml += "<br />";
                        webPagesHtml += $@"{HttpUtility.HtmlEncode(m.MembershipNumber)} | {HttpUtility.HtmlEncode(m.Name)}";
                        isFirst = false;
                    }

                    webPagesHtml += $@"

        </td>
    </tr>
";
                }

            }


            foreach (var webPageVersion in RelevantWebPageVersionsWithFalseClaims)
            {
                webPagesHtml += $@"
    <tr>
        <td colspan=3 class=tableTitle><a href=""{webPageVersion.WebPage.Url}"" target=misleadingClaimsPage>{System.Net.WebUtility.HtmlEncode(webPageVersion.WebPage.Url)}</a></td>
    </tr>
";
                var isEven = true;
                foreach (var claim in webPageVersion.GetAiClaimsResult(PromptSpecification.PromptReferenceNumber).FunctionCallResponse.misleadingClaims)
                {
                    webPagesHtml += $@"
    <tr class=""even-{isEven}"">
        <td>&nbsp;&nbsp;&nbsp;</td>
        <td>{System.Net.WebUtility.HtmlEncode(claim.claim)}</td>
        <td>{System.Net.WebUtility.HtmlEncode(claim.reasoning)}</td>
    </tr>
";
                    isEven = !isEven;
                }


            }
            webPagesHtml += $@"
        </td>
    </tr>
    </tbody>
</table>
";


            messageDelegate($@"Detected {totalClaims} in {RelevantWebPageVersionsWithFalseClaims.Count()} pages.");
            File.AppendAllText(reportFilePath, webPagesHtml);
            webPagesHtml = "";
            messageDelegate("Finished main claims list");


            webPagesHtml += $@"
<h2>Pages where no claims were detected</h2>

<table>
";
            foreach (var wpv in AssessedWebPageVersions.Where(pv => (pv.GetAiClaimsResult(PromptSpecification.PromptReferenceNumber)?.Success == false) && (pv.GetAiClaimsResult(PromptSpecification.PromptReferenceNumber).Reason == "")))
            {
                webPagesHtml += $@"
    <tr>
        <td>{HttpUtility.HtmlEncode(wpv.WebPage.Url)}</td>
        <td>{HttpUtility.HtmlEncode(wpv.WebPage.FolderPath)}</td>
    </tr>
";
            }


            webPagesHtml += $@"
</table>

<table>
";


            webPagesHtml = $@"
{webPagesHtml}
    </body>
 </html>";

            File.AppendAllText(reportFilePath, webPagesHtml);
            webPagesHtml = "";

            messageDelegate("Done");

            return;



        }

    }

    internal class CustomContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (ShouldIgnoreProperty(member))
            {
                property.Ignored = true;
            }

            return property;
        }


        static HashSet<string> PropertiesToIgnore = null;

        private bool ShouldIgnoreProperty(MemberInfo member)
        {
            if(PropertiesToIgnore == null)
            {
                PropertiesToIgnore = new HashSet<string>
                {
                    "Clinic",
                    "Members",
                    "PageVersions",
                    "Website",
                    "AllowDownload",
                    "FolderPath",
                    "AllowDownload",
                    "IsTooSimilarToAnotherPage",
                    "Lastchecked",
                    "WebPageIsNotHtml",
                    "WebPageDeleted",
                    "WebPageChangedFromThisVersion",
                    "SavedHtml",
                    "Filename",
                    "HtmlFilePath",
                    "InnerTextFilePath",
                    "LinkFreeInnerTextFilePath",
                    "SentencesFilePath",
                    "SimplifiedUrl",
                    "MessageContent",
                    "FunctionCallResponse"
                };

            }


            // Add logic here to decide whether to ignore the property
            // For example, ignoring a specific property name:
            if (PropertiesToIgnore.Contains(member.Name)) return true;
            return false;
        }
    }
}
