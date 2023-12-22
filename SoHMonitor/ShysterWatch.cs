using ShysterWatch.AiAnalysisHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShysterWatch
{



    /// <summary>
    /// Represents a list of shysters, their websites, twitter accounts etc. All
    /// the data within one of the folders beneath route level
    /// </summary>
    public abstract class ShysterGroup
    {
        public string FolderName;

        public override string ToString()
        {
            return FolderName;
        }

        public string FolderPath => Path.Combine(ShysterWatch.Properties.Settings.Default.RootFolder, FolderName);

        protected MembershipDatabase membershipDatabase;
        public abstract MembershipDatabase MembershipDb
        {
            get;
        }


        protected MembershipDatabaseScraper scraper;
        public abstract MembershipDatabaseScraper Scraper
        {
            get;
        }

        protected AiAnalysisHelper aiAnalysisHelper;
        public abstract AiAnalysisHelper AiAnalysisHelper
        {
            get;
        }

        protected SoHMonitor.Search.SearchHistory searchHistory;
        public SoHMonitor.Search.SearchHistory SearchHistory
        {
            get
            {
                if(searchHistory == null)
                {
                    searchHistory = SoHMonitor.Search.SearchHistory.Load(FolderPath);
                }
                return searchHistory;
            }

        }

        WebSiteDownloader websiteDownloader = null;
        public WebSiteDownloader WebsiteDownloader
        {
            get
            {
                if(websiteDownloader == null)
                {
                    websiteDownloader = new WebSiteDownloader();
                }
                return websiteDownloader;
            }
        }


        WebsiteDb websiteDb;
        public WebsiteDb WebsiteDb
        {
            get
            {
                if (websiteDb == null)
                {
                    websiteDb = WebsiteDb.Load(FolderPath);
                }
                return websiteDb;
            }
        }



    }



    class GeneralChiropracticCouncilGroup : ShysterGroup
    {

        public override AiAnalysisHelper AiAnalysisHelper => throw new NotImplementedException();
        public override MembershipDatabase MembershipDb
        {
            get
            {
                if (membershipDatabase == null)
                {
                    membershipDatabase = SoHMonitor.UkChiroMemberDatabase.Load();
                }
                return membershipDatabase;
            }
        }


        public override MembershipDatabaseScraper Scraper
        {
            get
            {
                if (scraper == null)
                {
                    scraper = new GCCScraper();
                }
                return scraper;
            }
        }

    }

    class CharityCommissionGroup : ShysterGroup
    {
        public override AiAnalysisHelper AiAnalysisHelper => throw new NotImplementedException();
        public override MembershipDatabase MembershipDb
        {
            get
            {
                if (membershipDatabase == null)
                {
                    membershipDatabase = SoHMonitor.CharityCommissionMemberDatabase.Load();
                }
                return membershipDatabase;
            }
        }


        public override MembershipDatabaseScraper Scraper
        {
            get
            {
                if (scraper == null)
                {
                    scraper = new Scrapers.CharityCommissionScraper();
                }
                return scraper;
            }
        }

    }

    class GoSCGroup : ShysterGroup
    {
        public override AiAnalysisHelper AiAnalysisHelper => throw new NotImplementedException();
        public override MembershipDatabase MembershipDb
        {
            get
            {
                if (membershipDatabase == null)
                {
                    membershipDatabase = ShysterWatch.MembershipDatabases.GoSC.GoSCMemberDatabase.Load();
                }
                return membershipDatabase;
            }
        }


        public override MembershipDatabaseScraper Scraper
        {
            get
            {
                if (scraper == null)
                {
                    scraper = new Scrapers.GoSCScraper();
                }
                return scraper;
            }
        }
    }



    class CNHCGroup : ShysterGroup
    {
        public override AiAnalysisHelper AiAnalysisHelper
        {
            get
            {
                if(aiAnalysisHelper == null)
                {
                    aiAnalysisHelper = new CNHCAiAnalysisHelper();
                }
                return aiAnalysisHelper;
            }
        }
        public override MembershipDatabase MembershipDb
        {
            get
            {
                if (membershipDatabase == null)
                {
                    membershipDatabase = ShysterWatch.MembershipDatabases.CNHC.CNHCMemberDatabase.Load();
                }
                return membershipDatabase;
            }
        }


        public override MembershipDatabaseScraper Scraper
        {
            get
            {
                if (scraper == null)
                {
                    scraper = new Scrapers.CNHCScraper();
                }
                return scraper;
            }
        }

    }


    class SocietyOfHomeopathsGroup : ShysterGroup
    {
        public override AiAnalysisHelper AiAnalysisHelper => throw new NotImplementedException();
        public override MembershipDatabase MembershipDb
        {
            get
            {
                if(membershipDatabase == null)
                {
                    membershipDatabase = SohMemberDatabase.Load();
                }
                return membershipDatabase;
            }
        }


        public override MembershipDatabaseScraper Scraper
        {
            get
            {
                if (scraper == null)
                {
                    scraper = new SoHXmlFileScraper();
                }
                return scraper;
            }
        }

    }



    public class Sys
    {


        static ShysterGroup currentGroup;
        public static ShysterGroup CurrentGroup
        {
            get
            {
                if (currentGroup == null)
                {
                    //ShysterWatch.Properties.Settings.Default.SelectedDataSource = "Society of Homeopaths";
                    currentGroup = Groups.Find(x => x.FolderName == ShysterWatch.Properties.Settings.Default.SelectedDataSource);
                }
                return currentGroup;
            }
        }
        static List<ShysterGroup> groups;
        public static List<ShysterGroup> Groups
        {
            get
            {
                if (groups == null)
                {
                    groups = new List<ShysterGroup>();

                    {
                        var g = new SocietyOfHomeopathsGroup
                        {
                            FolderName = "Society of Homeopaths"
                        };
                        groups.Add(g);
                    }

                    {
                        var g = new GeneralChiropracticCouncilGroup
                        {
                            FolderName = "General Chiropractic Council"
                        };
                        groups.Add(g);
                    }

                    {
                        var g = new CNHCGroup { FolderName = "CNHC" };
                        groups.Add(g);
                    }

                    {
                        var g = new GoSCGroup { FolderName = "GoSC" };
                        groups.Add(g);
                    }
                    {
                        var g = new CharityCommissionGroup { FolderName = "Charity Commission" };
                        groups.Add(g);
                    }
                }

                return groups;
            }
        }




        /// <summary>
        /// The path to the root folder where all the data is stored.
        /// </summary>
        public static string RootFolder
        {
            get
            {
                return ShysterWatch.Properties.Settings.Default.RootFolder;
            }
            set
            {
                ShysterWatch.Properties.Settings.Default.RootFolder = value;
                ShysterWatch.Properties.Settings.Default.Save();
            }
        }

        

        static SohMemberDatabase membershipDb = null;
        public static SohMemberDatabase MembershipDb
        {
            get
            {
                if (membershipDb == null)
                {
                    membershipDb = SohMemberDatabase.Load();
                }
                return membershipDb;
            }
        }



    }
}
