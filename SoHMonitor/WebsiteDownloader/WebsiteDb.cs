using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ShysterWatch
{
    public class WebsiteDb
    {

        public static string FixLineBreaks(string input)
        {
            return input.Replace("\n", "\r\n");
        }

        public List<Website> Websites = new List<Website>();

        //public List<string> SavedSearches = new List<string>();


        private Dictionary<string, Website> byUniqueId = null;
        public Website WebsiteFindByUniqueId(string uniqueId)
        {
            if (byUniqueId == null)
            {
                byUniqueId = new Dictionary<string, Website>();
                Websites.ForEach(x => byUniqueId.Add(x.UniqueId, x));
            }
            if (!byUniqueId.ContainsKey(uniqueId)) return null;

            return byUniqueId[uniqueId];
        }

        private Dictionary<string, WebPage> webPageByUniqueId = null;
        public WebPage WebPageFindByUniqueIds(string websiteUniqueId, string webPageUniqueId)
        {
            if (webPageByUniqueId == null)
            {
                webPageByUniqueId = new Dictionary<string, WebPage>();

                Websites.ForEach(x =>
                {
                    x.WebPages.ForEach(p => webPageByUniqueId.Add(x.UniqueId + "|" + p.UniqueId, p));
                });
            }
            string key = websiteUniqueId + "|" + webPageUniqueId;
            if (!webPageByUniqueId.ContainsKey(key)) return null;
            return webPageByUniqueId[key];
        }


        //public static WebsiteDb websiteDb = null;
        //public static WebsiteDb Db
        //{
        //    get
        //    {
        //        if (websiteDb == null)
        //        {
        //            websiteDb = WebsiteDb.Load();
        //        }
        //        return websiteDb;
        //    }
        //}


        public bool Save()
        {

            var start = DateTime.Now;
            Debug.WriteLine("Saving");
            var tmp = Save(this, System.IO.Path.Combine(Sys.CurrentGroup.FolderPath, ShysterWatch.Properties.Settings.Default.WebsiteDataFilename));
            Debug.WriteLine("Saving finished in " + (DateTime.Now-start).TotalSeconds + " seconds.");
            return tmp;
        }
        public static bool Save(WebsiteDb obj, string filepath)
        {
            //try
            {

                System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(WebsiteDb));
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


        [NonSerialized]
        public String RootFolderPath;

        public static WebsiteDb Load(string rootFolderPath)
        {
            WebsiteDb db;

            string filename = System.IO.Path.Combine(rootFolderPath, ShysterWatch.Properties.Settings.Default.WebsiteDataFilename);
            if (File.Exists(filename)) db = LoadFile(filename);
            else db =new WebsiteDb();

            db.RootFolderPath = rootFolderPath;


            return db;

        }

        static WebsiteDb LoadFile(string filepath)
        {
            if (!File.Exists(filepath)) return new WebsiteDb();

            System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(WebsiteDb));
            System.IO.StreamReader file = new System.IO.StreamReader(filepath);
            WebsiteDb obj = (WebsiteDb)reader.Deserialize(file);
            file.Close();


            foreach (var ws in obj.Websites)
            {
                ws.WebsiteDb = obj;
                foreach (var wp in ws.WebPages)
                {
                    wp.Website = ws;

                    foreach (var wpv in wp.PageVersions)
                    {
                        wpv.WebPage = wp;
                    }

                }
            }

            return obj;
        }


    }


}
