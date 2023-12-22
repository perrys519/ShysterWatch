using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace ShysterWatch
{
    public abstract class MembershipDatabase
    {
        /// <summary>
        /// Gets the member detail from the URL in the membership database.
        /// </summary>
        /// <param name="SourceUrl"></param>
        /// <returns></returns>
        public abstract List<Member> GetMemberBySourceUrl(string SourceUrl);

        [XmlIgnore]
        public abstract List<string> GetAllSourceUrls
        {
            get;
        }


        [XmlIgnore]
        public static string Filename => "Members.xml";

        [XmlIgnore]
        public static string FilePath => Path.Combine(Sys.CurrentGroup.FolderPath, Filename);


        [XmlIgnore]
        internal List<string> categories;

        public virtual List<string> GetCatgeories()
        {
            if (categories == null) categories = new List<string>();
            return categories;
        }

    }


}
