using System.Collections.Generic;
using System.Device.Location;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System;
using System.Linq;

namespace ShysterWatch
{



    /// <summary>
    /// Represents a single member of the SoH, found in any scrape
    /// </summary>
    public class SohMember
    {

        public string Id;

        public DateTime FirstSeen;
        public DateTime LastSeen;
        public DateTime? NoticedDeleted = null;

        public List<SohClinic> Clinics = new List<SohClinic>();
    }


}
