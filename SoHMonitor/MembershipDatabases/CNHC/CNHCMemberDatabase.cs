using Newtonsoft.Json;
using ShysterWatch.MembershipDatabases.GoSC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ShysterWatch.MembershipDatabases.CNHC
{
    public class CNHCMemberDatabase : MembershipDatabase
    {

        public override List<string> GetCatgeories()
        {
            if (categories == null)
            {
                categories = Clinics.SelectMany(c => c.Members).SelectMany(m => m.Disciplines).Distinct().OrderBy(x => x).ToList();
            }
            return categories;
        }

        public override List<ShysterWatch.Member> GetMemberBySourceUrl(string SourceUrl)
        {
            return Clinics.Where(x => x.Website == SourceUrl).ToList<ShysterWatch.Member>();
        }

        public List<CNHCMember> Members = new List<CNHCMember>();


        [XmlIgnore]
        public List<CNHCMember> RelevantMembers
        {
            get
            {
                var relevantMembers = Members.Where(m => m.IsRelevant).ToList();
                return relevantMembers;
            }
        }



        [XmlIgnore]
        public List<Clinic> RelevantClinics {
            get
            {
                var relevantClinics = new List<Clinic>();

                foreach (var c in Clinics)
                {
                    foreach (var m in c.Members)
                    {

                        if (m.IsRelevant)
                        {


                            relevantClinics.Add(c);
                            break;
                        }

                    }

                }

                return relevantClinics;

            }
        }

        
        public List<Clinic> Clinics = new List<Clinic>();

        [XmlIgnore]
        public override List<string> GetAllSourceUrls
        {
            get
            {
                var x1 = Clinics.Where(x => x.Website != "").Select(x => x.Website).ToList();
                return x1;
            }
        }

        public void Save()
        {
            SoHMonitor.Utilities.Save(this, typeof(CNHCMemberDatabase), FilePath);
        }

        public static CNHCMemberDatabase Load()
        {
            
            var db = (CNHCMemberDatabase)SoHMonitor.Utilities.Load(typeof(CNHCMemberDatabase), FilePath);
            if (db == null) db = new CNHCMemberDatabase();


            foreach (var clinic in db.Clinics) clinic.Db = db;

            return db;
        }

    }


    public class Clinic : ShysterWatch.Member
    {
        public string ListedWebsite;
        public string Website = "";
        public string Phone;
        public string Mobile;
        public string Address;
        public string Postcode;


        public Website WebsiteObj {
            get
            {
                var sites = Sys.CurrentGroup.WebsiteDb.Websites.Where(w => w.SourceUrl == this.Website);
                if (sites.Count() == 0) return null;
                return sites.First();

            }
        }
        private IEnumerable<string> disciplines = null;
        public IEnumerable<string> Disciplines
        {
            get
            {
                if (disciplines == null) disciplines = this.Members.SelectMany(w => w.Disciplines).Distinct().OrderBy(d => d);
                return disciplines;
            }
        }

        [XmlIgnore]
        public CNHCMemberDatabase Db;

        public List<string> MemberNumbers = new List<string>();

        [XmlIgnore]
        public List<CNHCMember> Members
        {
            get
            {
                if (Db == null) return new List<CNHCMember>();
                return Db.Members.Where(x => MemberNumbers.Contains(x.MembershipNumber)).ToList();
            }
        }

        HashSet<CNHCMember> _MembersHashset = new HashSet<CNHCMember>();
        public HashSet<CNHCMember> MembersHashset
        {
            get
            {
                if(_MembersHashset == null)
                {
                    _MembersHashset= new HashSet<CNHCMember>(Members);
                }
                return _MembersHashset;
            }
        }

        public override List<object> ExcelOutputFields
        {
            get
            {
                List<object> fields = new List<object>();
                fields.Add(Postcode);

                var members = Members;
                fields.Add(String.Join(", ", members.Select(x => x.MembershipNumber)));
                fields.Add(String.Join(", ", members.Select(x => x.Name)));
                fields.Add(String.Join(", ", String.Join("|", Categories())));

                return fields;
            }
        }

        public override List<string> Categories()
        {
            return this.Members.SelectMany(x => x.Disciplines).Distinct().ToList();

        }
    }

    public class CNHCMember
    {
        public string Name;
        public string MembershipNumber;
        public List<string> Disciplines = new List<string>();

        public Clinic Clinic
        {
            get
            {
                var clinics = ((CNHCMemberDatabase)Sys.CurrentGroup.MembershipDb).Clinics.Where(c => c.Members.Contains(this));
                if(clinics.Count() < 1) return null;
                return clinics.First();
            }
        }

        public static string[] IrrelevantDisciplines = { "Sports Therapy", "Sports Massage", "Massage Therapy", "Nutritional Therapy", "Yoga Therapy", "Hypnotherapy", "" };


        [XmlIgnore]
        public bool IsRelevant
        {
            get
            {


                foreach (var d in this.Disciplines)
                {
                    if (!IrrelevantDisciplines.Contains(d))
                    {
                        return true;
                    }
                }

                return false;
            }


        }


    }


}
