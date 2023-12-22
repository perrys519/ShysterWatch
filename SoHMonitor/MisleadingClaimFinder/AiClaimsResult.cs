using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ShysterWatch.MisleadingClaimFinder
{
    public class AiClaimsResult
    {
        public bool Success = false;
        public string Reason = "";
        public int PromptTokens = 0;
        public int CompletionTokens = 0;
        public int TotalTokens = 0;
        public string MessageContent = "";
        public bool DeserializationError = false;
        public decimal Cost
        {
            get
            {
                return (decimal)0.01/ (decimal)1000 * (decimal)PromptTokens + (decimal)0.03/ (decimal)1000 * (decimal)CompletionTokens;
            }
        }
        public FunctionCallResponseObj FunctionCallResponse;


        public static AiClaimsResult Load(string path)
        {
            if(!File.Exists(path)) return null;
            var json = File.ReadAllText(path);

            try
            {
                AiClaimsResult result = JsonConvert.DeserializeObject<AiClaimsResult>(json);
                return result;
            }
            catch
            {
                AiClaimsResult result = new AiClaimsResult();
                result.DeserializationError = true;
                return result;
            }
        }


    }

    public class FunctionCallResponseObj
    {
        public List<AiDetectedMisleadingClaim> misleadingClaims;
    }
    public class AiDetectedMisleadingClaim
    {
        public string claim = "";
        public string reasoning = "";
    }
}
