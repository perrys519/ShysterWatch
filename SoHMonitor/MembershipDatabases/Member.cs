using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ShysterWatch
{
    public abstract class Member
    {

        public virtual List<string> Categories()
        {

            return null;
        }

        public bool IsInOneCategory(List<string> searchCategories)
        {
            foreach(var cat in Categories())
            {
                if (searchCategories.Contains(cat)) return true;
            }

            return false;
        }


        public abstract List<object> ExcelOutputFields
        {
            get;
        } 

    }


}
