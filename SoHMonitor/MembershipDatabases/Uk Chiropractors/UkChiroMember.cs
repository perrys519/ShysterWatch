using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShysterWatch
{
    public class UkChiroMember : Member
    {
        
        public string WebsiteUrl;
        public string ClinicName;
        public List<string> PractitionerNames = new List<string>();
        public string Postcode;
        public string AssociationName;
        public int MemberId;

        public override List<object> ExcelOutputFields
        {
            get
            {
                List<object> fields = new List<object>();
                fields.Add(ClinicName);
                fields.Add(Postcode);
                fields.Add(AssociationName);
                fields.Add(MemberId);
                return fields;
            }
        }
    }
}
