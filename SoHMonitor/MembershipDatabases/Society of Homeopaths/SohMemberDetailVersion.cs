using System;
using System.Collections.Generic;

namespace ShysterWatch
{
    /// <summary>
    /// Represents a snapshot of the data on the website at a specific time
    /// </summary>
    public class SohMemberDetailVersion
    {


        public bool DataIsSameAs(SohMemberDetailVersion v2)
        {
            if (Id != v2.Id) return false;
            if (OtherClinicIds != v2.OtherClinicIds) return false;
            if (Name != v2.Name) return false;
            if (Address != v2.Address) return false;
            if (Email != v2.Email) return false;
            if (PhoneNumber != v2.PhoneNumber) return false;
            if (Website != v2.Website) return false;
            if (MainClinic != v2.MainClinic) return false;

            return true;
        }


        public string Id;

        public string ScrapedFromFilename;
        
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
        public DateTime? TimeReplaced;

        public List<string> OtherClinicIds = new List<string>();

        public List<DateTime> DataVerified;
        public string Name;
        public string Qualified;
        public string Address;
        public string Email;
        public string PhoneNumber;
        public string Website;

        public string MainClinic;

        public bool NoLongerExists = false;



    }


}
