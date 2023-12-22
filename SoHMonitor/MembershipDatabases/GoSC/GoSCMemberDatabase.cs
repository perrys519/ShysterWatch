using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ShysterWatch.MembershipDatabases.GoSC
{
    public class GoSCMemberDatabase : MembershipDatabase
    {
        public override List<string> GetAllSourceUrls
        {
            get
            {
                return Clinics.Where(x => x.Website != "").Select(x => x.Website).ToList();
            }
        }
        public override List<Member> GetMemberBySourceUrl(string SourceUrl)
        {
            return Clinics.Where(x => x.Website == SourceUrl).ToList<ShysterWatch.Member>();
        }

        public void Save()
        {
            SoHMonitor.Utilities.Save(this, typeof(GoSCMemberDatabase), FilePath);
        }

        public static GoSCMemberDatabase Load()
        {

            var db = (GoSCMemberDatabase)SoHMonitor.Utilities.Load(typeof(GoSCMemberDatabase), FilePath);
            if (db == null) db = new GoSCMemberDatabase();


            foreach (var clinic in db.Clinics) clinic.Db = db;

            return db;
        }


        public List<GoSCMember> Members = new List<GoSCMember>();


        public List<Clinic> Clinics = new List<Clinic>();
    }



    public class Clinic : ShysterWatch.Member
    {
        public string Name;
        public string Website;
        public string Phone;
        public string Address;
        public string Postcode;
        public string Email;




        [XmlIgnore]
        public GoSCMemberDatabase Db;

        public List<string> MemberUrlIds = new List<string>();


        public List<GoSCMember> Members
        {
            get
            {
                if (Db == null) return new List<GoSCMember>();
                return Db.Members.Where(x => MemberUrlIds.Contains(x.UrlId)).ToList();
            }
        }

        public override List<object> ExcelOutputFields
        {
            get
            {
                List<object> fields = new List<object>();
                fields.Add(Name);
                fields.Add(Website);
                fields.Add(Phone);
                fields.Add(Email);

                var members = Members;
                fields.Add(String.Join(", ", members.Select(x => x.UrlId)));
                fields.Add(String.Join(", ", members.Select(x => x.Name)));

                return fields;
            }
        }

        public string UrlId;
    }

    public class GoSCMember
    {
        public string Name;
        public string UrlId;

    }


}
