using System;
using System.Collections.Generic;
using System.Linq;

namespace ShysterWatch
{


    public class SohClinic : Member
    {


        public override List<object> ExcelOutputFields
        {
            get
            {
                List<object> fields = new List<object>();

                fields.Add(Name);
                fields.Add(Postcode);

                return fields;
            }
        }

        public bool DataIsSameAs(SohClinic v2)
        {
            if (Id != v2.Id) return false;
            if (Name != v2.Name) return false;
            if (Address1 != v2.Address1) return false;
            if (Address2 != v2.Address2) return false;
            if (Address3 != v2.Address3) return false;
            if (Address4 != v2.Address4) return false;
            if (City != v2.City) return false;
            if (Country != v2.Country) return false;
            if (Postcode != v2.Postcode) return false;
            if (Country != v2.Country) return false;
            if (Email != v2.Email) return false;
            if (PhoneNumber != v2.PhoneNumber) return false;
            if (Website != v2.Website) return false;

            return true;
        }


        public string Id;
        

        /// <summary>
        /// The first time that this data was seen
        /// </summary>
        public DateTime TimeFirstSeen;

        /// <summary>
        /// The last time this data was seen
        /// </summary>
        public DateTime TimeLastSeen;

        /// <summary>
        /// The time when we discovered that this data had changed
        /// </summary>
        public DateTime? TimeNoticeRemoved;

        public List<DateTime> DataVerified = new List<DateTime>();

        public string Name;
        public string Qualified;
        public string Address1;
        public string Address2;
        public string Address3;
        public string Address4;
        public string City;
        public string County;
        public string Postcode;
        public string Country;
        public string Email;
        public string PhoneNumber;
        public string Website;
        



    }


}
