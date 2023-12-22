using SoHMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShysterWatch;

namespace SoHMonitor
{
    public class UkChiroMemberDatabase :  MembershipDatabase
    {

        public override List<Member> GetMemberBySourceUrl(string SourceUrl)
        {
            return Members.Where(x => x.WebsiteUrl == SourceUrl).ToList<Member>();
        }

        public override List<string> GetAllSourceUrls
        {
            get
            {
                return Members.Where(x => x.WebsiteUrl != "").Select(x => x.WebsiteUrl).ToList();
            }
        }

        public List<UkChiroMember> Members = new List<UkChiroMember>();

        public static UkChiroMemberDatabase Load()
        {
            var db = (UkChiroMemberDatabase)Utilities.Load(typeof(UkChiroMemberDatabase), FilePath);
            if (db == null) return new UkChiroMemberDatabase();
            return db;
        }

        public void Save()
        {
            Utilities.Save(this, typeof(UkChiroMemberDatabase), FilePath);
        }
    }
}
