using SoHMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShysterWatch;

namespace SoHMonitor
{
    public class CharityCommissionMemberDatabase : MembershipDatabase
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

        public List<CharityCommissionMember> Members = new List<CharityCommissionMember>();

        public static CharityCommissionMemberDatabase Load()
        {
            var db = (CharityCommissionMemberDatabase)Utilities.Load(typeof(CharityCommissionMemberDatabase), FilePath);
            if (db == null) return new CharityCommissionMemberDatabase();
            return db;
        }

        public void Save()
        {
            Utilities.Save(this, typeof(CharityCommissionMemberDatabase), FilePath);
        }
    }
}
