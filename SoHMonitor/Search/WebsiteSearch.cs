using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace ShysterWatch.Search
{

    class WindowsSearch {
        public DirectoryInfo TargetFolder;

        public List<string> SearchTerms;

        OleDbConnection connection;

        public OleDbDataReader Run()
        {
            connection = new OleDbConnection(@"Provider=Search.CollatorDSO;Extended Properties=""Application=Windows""");
            connection.Open();

            Debug.WriteLine(Query);

            var command = new OleDbCommand(Query, connection);

            return command.ExecuteReader();
        }


        public string Query {

            get
            {
                var s = @"
            SELECT 
                System.ItemPathDisplay
            FROM SystemIndex 
            WHERE scope ='file:" + TargetFolder.FullName + @"'
                AND System.ItemName LIKE '%linkfree.txt'
";

                if (SearchTerms.Count() > 0)
                {


                    s += @"
                AND(";

                    int count = 0;
                    foreach (var term in SearchTerms)
                    {
                        if(count != 0) s += @"
                  OR ";
                        count++;

                        s += @"
                     contains ('""" + term.Replace("\"", "") + @"""')";

                    }

                    s += @"
                )";
                }

                s += @"
            ";

                return s;
            }


        }
    }


    public class WebsiteSearch
    {
        public List<string> SearchTerms;
        public string SearchText;

        public DateTime EffectiveDate;

        public SearchResults Run(string searchText, DateTime effectiveDate)
        {

            var search = new WindowsSearch();
            EffectiveDate = effectiveDate;

            SearchText = searchText;

            SearchTerms = SearchText.Split('\n').ToList();
            SearchTerms = SearchTerms.Select(x => x.Trim()).Where(x => x != "").ToList();

            search.TargetFolder = new DirectoryInfo(WebPage.WebsiteFolderPath);


            search.SearchTerms = SearchTerms;

            var results = new SearchResults(search.Run(), EffectiveDate);
            results.WebsiteSearch = this;


            return results;
        }

    }



    public class SearchResults
    {


        public List<SearchResult> AllResults = new List<SearchResult>();

        bool surpressResults = true;
        public bool SurpressResults {
            get
            {
                return surpressResults;
            }
            set
            {
                surpressResults = value;
                results = null;
                groupByWebsite = null;
            }
        }


        public List<string> LimitToCategories;

        List<Website> websites = null;
        public List<Website> Websites
        {
            get
            {
                if(websites == null) websites = (from result in Results select result.Website).Distinct().ToList();
                return websites;
            }
        }

        List<SearchResult> results = null;
        public List<SearchResult> Results
        {
            get
            {
                if(results == null)
                {
                    if (!SurpressResults) results = AllResults;
                    else
                    {
//                        results = AllResults.Where(x => !x.WebPage.IgnoreSearchTerms.Contains(this.WebsiteSearch.SearchText) && !x.WebPageVersion.IgnoreSearchTerms.Contains(this.WebsiteSearch.SearchText)).ToList();

                        var SavedSearch = Sys.CurrentGroup.SearchHistory["Website"].SavedSearches.Find(x => x.SearchText == this.WebsiteSearch.SearchText);
                        if (SavedSearch == null) results = AllResults;
                        else
                        {



                        results = AllResults.Where(x =>
                        {
                            var siteAugTupe = SavedSearch.GetAugmentationType(x.WebSiteUniqueId, "", 0);
                            if (siteAugTupe == SoHMonitor.Search.SearchResultAugmentationType.IgnoreForever) return false;

                            var augType = SavedSearch.GetAugmentationType(x.WebSiteUniqueId, x.WebPageUniqueId, x.WebPageVersion.Index);
                            if (augType == SoHMonitor.Search.SearchResultAugmentationType.IgnoreForever) return false;
                            if (augType == SoHMonitor.Search.SearchResultAugmentationType.IgnoreThisVersion) return false;
                            return true;
                        }).ToList();
                        }

                    }

                    if (LimitToCategories != null)
                    {
                        results = results.Where(x => x.IsInAtLeastOneCategory(LimitToCategories)).ToList();
                    }

                }
                return results;
            }
        }

        public void SaveToExcelFile()
        {

            Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
            string myPath = Path.Combine(Sys.RootFolder, @"Templates\SearchExportTemplate.xlsx");
            Workbook workbook = excelApp.Workbooks.Open(myPath, null, true);

            // Get Worksheet
            Microsoft.Office.Interop.Excel.Worksheet worksheet = excelApp.Worksheets[1];


            Range dataStartPoint = workbook.Names.Item("data_start").RefersToRange;

            workbook.Names.Item("search_terms").RefersToRange.Value = this.WebsiteSearch.SearchText;
            workbook.Names.Item("search_date").RefersToRange.Value = DateTime.Now.Date;
            workbook.Names.Item("search_time").RefersToRange.Value = DateTime.Now;

            int firstCol = dataStartPoint.Column;
            int row = dataStartPoint.Row;

            foreach (var result in this.Results)
            {
                int col = firstCol;

                excelApp.Cells[row, col++] = result.Website.SourceUrl;
                excelApp.Cells[row, col++] = result.CachedHtmlFile.FullName;
                excelApp.Cells[row, col++] = result.WebPage.Url;

                if (result.WebPageVersion.WebPageCheckedAsValid.Count > 0)
                {

                    excelApp.Cells[row, col++] = result.WebPageVersion.WebPageCheckedAsValid.First();
                    excelApp.Cells[row, col++] = result.WebPageVersion.WebPageCheckedAsValid.Last();

                    excelApp.Cells[row, col++] = result.WebPageVersion.Index;
                    excelApp.Cells[row, col++] = result.WebPageVersion.WebPageChangedFromThisVersion;
                }

                var members = Sys.CurrentGroup.MembershipDb.GetMemberBySourceUrl(result.Website.SourceUrl);
                foreach (var clinic in members)
                {
                    foreach(var xlField in clinic.ExcelOutputFields)
                    {
                        excelApp.Cells[row, col++] = xlField;
                    }

                    //excelApp.Cells[row, col++] = clinic.MemberName;
                    //excelApp.Cells[row, col++] = clinic.MemberPostcode;
                }






                //string sentences = "";
                //foreach(var s in result.WebPageVersion.Sentences())
                //{
                //    if (sentences != "") sentences += "\r\n";
                //    sentences += s;
                //}
                //excelApp.Cells[row, col++] = sentences;



                row++;
            }




            excelApp.Visible = true;

            var saveLocation = Path.Combine(Sys.RootFolder, @"DataExports\" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".xlsx");

            workbook.SaveAs(saveLocation);
            


        }



        public WebsiteSearch WebsiteSearch;

        public void Add(SearchResult result)
        {
            result.Searchresults = this;
            AllResults.Add(result);
        }

        List<ResultGroup> groupByWebsite = null;
        public List<ResultGroup> GroupByWebsite
        {
            get
            {
                if (groupByWebsite == null)
                {
                    var results = Results;

                    groupByWebsite = new List<ResultGroup>();

                    foreach (var site in results.Select(x => x.Website).Distinct())
                    {
                        ResultGroup group = new ResultGroup();
                        group.Website = site;
                        group.Results = new List<SearchResult>();

                        foreach (var result in results.Where(x => x.Website == site))
                        {
                            group.Results.Add(result);
                        }

                        groupByWebsite.Add(group);
                    }
                }

                return groupByWebsite;

            }
        }

        DateTime EffectiveDate;

        public SearchResults(OleDbDataReader oleDbDataReader, DateTime effectiveDate)
        {
            EffectiveDate = effectiveDate;


            while (oleDbDataReader.Read())
            {
             


                var searchResult = new SearchResult((string)oleDbDataReader[0]);
                //Need to implement the effective date here

                //exclude a result where we couldn't find the record.
                if(searchResult.WebPage == null)
                {
                    continue;
                }


                var webPageVersion = searchResult.WebPageVersion;

                if (webPageVersion == null) continue;

                //Exclude a result that had moved on before the effective date
                if ((webPageVersion.WebPageChangedFromThisVersion != null) && (webPageVersion.WebPageChangedFromThisVersion < EffectiveDate))
                {
                    continue;
                }


                //Exclude a result that wasn't found before the effective date
                if ((webPageVersion.WebPageCheckedAsValid.Count > 0) && (webPageVersion.WebPageCheckedAsValid.First() > EffectiveDate))
                {
                    continue;
                }



                //if(searchResult.WebPageVersion.)

                Add(searchResult);
            }
        }
    }


    public struct ResultGroup
    {
        public Website Website;
        public List<SearchResult> Results;
    }


    public class SearchResult
    {

        public SearchResults Searchresults;

        public string FilePathFound;
        public SearchResult(string filepathFound)
        {
            FilePathFound = filepathFound;

        }

        public bool IsInAtLeastOneCategory(List<string> categories)
        {
            foreach(var member in this.Members)
            {
                if (member.IsInOneCategory(categories)) return true;
            }
            return false;
        }

        public List<Member> Members => Sys.CurrentGroup.MembershipDb.GetMemberBySourceUrl(Website.SourceUrl);

        public List<string> RelevantSentences
        {
            get
            {
                var v = WebPageVersion;
                var sentences = v.Sentences();

                List<string> relevantOnes = sentences.Where(x =>
                {
                    foreach(var phrase in Searchresults.WebsiteSearch.SearchTerms)
                    {
                        if (x.ToLower().IndexOf(phrase.ToLower()) != -1) return true;
                    }
                    return false;

                }).ToList();

                return relevantOnes;

            }
        }


        private WebPageVersion webPageVersion = null;
        public WebPageVersion WebPageVersion
        {
            get
            {
                if(webPageVersion == null)
                    webPageVersion = WebPage.PageVersions.Find(x => x.Index == FileIndex);
                return webPageVersion;
            }
        }

        Website website;
        public Website Website
        {
            get
            {
                if(website == null) website = Sys.CurrentGroup.WebsiteDb.WebsiteFindByUniqueId(WebSiteUniqueId);
                return website;
            }
        }

        WebPage webPage = null;
        public WebPage WebPage
        {
            get
            {
                if(webPage == null) webPage = Sys.CurrentGroup.WebsiteDb.WebPageFindByUniqueIds(this.WebSiteUniqueId, this.WebPageUniqueId);
                return webPage;
            }
        }


        public FileInfo FileFoundInfo
        {
            get
            {
                return new FileInfo(FilePathFound);
            }
        }

        public string WebPageUniqueId
        {
            get
            {
                return FileFoundInfo.Directory.Name;
            }
        }

        public string WebSiteUniqueId
        {
            get
            {
                return FileFoundInfo.Directory.Parent.Name;
            }
        }


        public FileInfo CachedHtmlFile
        {
            get
            {
                var s = Path.Combine(FileFoundInfo.Directory.FullName, FileIndex.ToString() + ".html");
                return new FileInfo(s);
            }
        }

        public int FileIndex
        {
            get
            {
                string NoExt = FileFoundInfo.Name.Substring(0, FileFoundInfo.Name.IndexOf("."));

                int i = int.Parse(NoExt);


                return i;
            }
        }

        

    }



}
