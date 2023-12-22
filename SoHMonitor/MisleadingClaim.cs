using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShysterWatch
{


    public class MisleadingClaimsList
    {
        public List<MisleadingClaim> Claims = new List<MisleadingClaim>();
        public DateTime Created;
        public int CompletionTokens = 0;
        public int PromptTokens = 0;
        public int TotalTokens = 0;
    }

    public class MisleadingClaim
    {
        public MisleadingClaim() { }

    }
}
