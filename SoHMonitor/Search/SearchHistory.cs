using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoHMonitor;
using System.IO;
using System.Xml.Serialization;

namespace SoHMonitor.Search
{

    public class SearchContext
    {
        public string Name;

        public List<SavedSearch> SavedSearches = new List<SavedSearch>();
    }


    public class SearchHistory
    {

        public List<SearchContext> SearchContext = new List<Search.SearchContext>();
        public SearchContext this[string name]
        {
            get
            {
                var context = SearchContext.Where(x => x.Name == name).FirstOrDefault();
                if(context == null)
                {
                    context = new SearchContext();
                    context.Name = name;
                    SearchContext.Add(context);
                }
                return context;
            }


        }


        public bool Save()
        {

            var start = DateTime.Now;
            var tmp = Save(this, System.IO.Path.Combine(ShysterWatch.Sys.CurrentGroup.FolderPath, XmlFilename));
            return tmp;
        }
        public static bool Save(SearchHistory obj, string filepath)
        {
            //try
            {
                foreach (var c in obj.SearchContext)
                {
                    foreach (var s in c.SavedSearches)
                    {
                        s.CopyDictionaryToLists();
                    }
                }

                System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(SearchHistory));
                System.IO.FileStream file = System.IO.File.Create(filepath);


                writer.Serialize(file, obj);
                file.Close();
                return true;
            }
            //catch
            {

            }

            //return false;
        }


        [XmlIgnore]
        public String RootFolderPath;

        const string XmlFilename = "SearchHistory.xml";

        public static SearchHistory Load(string rootFolderPath)
        {
            SearchHistory db;

            string filename = System.IO.Path.Combine(rootFolderPath, XmlFilename);
            if (File.Exists(filename)) db = LoadFile(filename);
            else db = new SearchHistory();

            db.RootFolderPath = rootFolderPath;


            return db;

        }

        static SearchHistory LoadFile(string filepath)
        {


            System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(SearchHistory));
            System.IO.StreamReader file = new System.IO.StreamReader(filepath);
            SearchHistory obj = (SearchHistory)reader.Deserialize(file);
            file.Close();

            foreach (var c in obj.SearchContext)
            {
                foreach (var s in c.SavedSearches)
                {
                    s.CopyListsToDictionary();
                }
            }

            return obj;
        }



    }


    public class SavedSearch
    {
        public string SearchText;


        Dictionary<string, SearchResultAugmentation> SearchResultAugmentations = new Dictionary<string, SearchResultAugmentation>();

        public List<string> dictionaryKeys;
        public List<SearchResultAugmentation> dictionaryValues;
        
        internal void CopyDictionaryToLists()
        {
            dictionaryKeys = new List<string>();
            dictionaryValues = new List<SearchResultAugmentation>();

            foreach (string key in SearchResultAugmentations.Keys)
            {
                dictionaryKeys.Add(key);
                dictionaryValues.Add(SearchResultAugmentations[key]);
            }
        }

        internal void CopyListsToDictionary()
        {
            SearchResultAugmentations = new Dictionary<string, SearchResultAugmentation>();
            for(int i=0; i<dictionaryKeys.Count; i++)
            {
                SearchResultAugmentations.Add(dictionaryKeys[i], dictionaryValues[i]);
            }
        }



        public void SetAugmentation(string websiteUniqueId, string webpageUniqueId, int version, SearchResultAugmentationType augmentationType)
        {


            var key = websiteUniqueId + "|" + webpageUniqueId;
            if(augmentationType != SearchResultAugmentationType.IgnoreForever) key += "|" + version;


            if (SearchResultAugmentations.ContainsKey(key))
            {
                var a = SearchResultAugmentations[key];
                a.AugmentationType = augmentationType;
            }
            else
            {
                var a = new SearchResultAugmentation()
                {
                    AugmentationType = augmentationType,
                    Version = version
                };
                SearchResultAugmentations.Add(key, a);
            }
        }


        public SearchResultAugmentationType GetAugmentationType(string websiteUniqueId)
        {
            return GetAugmentationType(websiteUniqueId, "", 0);
        }

        public SearchResultAugmentationType GetAugmentationType(string websiteUniqueId, string webpageUniqueId, int version)
        {


            var key = websiteUniqueId + "|" + webpageUniqueId;
            key += "|" + version;

            if (SearchResultAugmentations.ContainsKey(key))
            {
                var a = SearchResultAugmentations[key];
                return a.AugmentationType;
            }


            key = websiteUniqueId + "|" + webpageUniqueId;
            if (SearchResultAugmentations.ContainsKey(key))
            {
                var a = SearchResultAugmentations[key];
                return a.AugmentationType;
            }
            
            return SearchResultAugmentationType.None;
            
        }


    }


    public enum SearchResultAugmentationType
    {
        IgnoreForever,
        IgnoreThisVersion,
        Concerning,
        None
    }

    public class SearchResultAugmentation
    {

        public int? Version;
        public SearchResultAugmentationType AugmentationType;

    }



}
