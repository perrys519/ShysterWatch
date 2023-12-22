using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ShysterWatch
{


    
    public class SohMemberDatabase : MembershipDatabase
    {
        public override List<Member> GetMemberBySourceUrl(string SourceUrl)
        {
            List<Member> members = (from sohMember in Members
                            from clinic in sohMember.Clinics
                            where clinic.Website == SourceUrl
                            select clinic).ToList<SohClinic>().ToList<Member>();

            return members;
        }

        public override List<string> GetAllSourceUrls
        {
            get
            {
                return (from sohMember in Members
                        from clinic in sohMember.Clinics
                        where clinic.Website != ""
                        select clinic.Website).ToList();
            }
        }

        public List<SohMember> Members = new List<SohMember>();
        
        public SohMember GetById(string id)
        {
            return Members.Where(x => x.Id == id).FirstOrDefault();
        }

        public bool Save()
        {
            return Save(this, System.IO.Path.Combine(Sys.CurrentGroup.FolderPath, ShysterWatch.Properties.Settings.Default.MemberDataFilename));
        }
        public static bool Save(SohMemberDatabase obj, string filepath)
        {
            //try
            {

                System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(SohMemberDatabase));
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

        public static SohMemberDatabase Load()
        {
            string filename = System.IO.Path.Combine(Sys.CurrentGroup.FolderPath, ShysterWatch.Properties.Settings.Default.MemberDataFilename);
            if (File.Exists(filename)) return Load(filename);

            else return new SohMemberDatabase();
        }

        public static SohMemberDatabase Load(string filepath)
        {


            System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(SohMemberDatabase));
            System.IO.StreamReader file = new System.IO.StreamReader(filepath);
            SohMemberDatabase obj = (SohMemberDatabase)reader.Deserialize(file);
            file.Close();

            return obj;
        }

    }


}
