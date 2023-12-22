using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShysterWatch
{
    public class CharityCommissionMember : Member
    {

        public string CharityName;
        public int CharityNumber;
        public string WebsiteUrl;

        public override List<object> ExcelOutputFields
        {
            get
            {
                List<object> fields = new List<object>();
                fields.Add(CharityName);
                fields.Add(CharityNumber);
                fields.Add(WebsiteUrl);
                return fields;
            }
        }
    }
}
