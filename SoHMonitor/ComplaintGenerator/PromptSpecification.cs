using System;
using System.Collections.Generic;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.Builders;
using OpenAI.ObjectModels.SharedModels;
using System.Linq;

namespace ShysterWatch.ComplaintGenerator
{
    public class PromptSpecification
    {
        public String PromptTemplate;

        public string Prompt(string text)
        {
            return PromptTemplate.Replace("{text}", text);
        }

        public string Model = "";
        public string Name = "Name Required";

        public override string ToString()
        {
            return Name;
        }

        public List<FunctionDefinition> FunctionDefinitions;

        int _promptReferenceNumber=int.MinValue;
        public int PromptReferenceNumber
        {
            get
            {
                if(_promptReferenceNumber == int.MinValue)
                {
                    _promptReferenceNumber = PromptSpecifications.Where(x => x.Value == this).First().Key;
                }

                return _promptReferenceNumber;
            }
        }


        PromptSpecification()
        {
            IDictionary<string, PropertyDefinition> misleadingClaimObj = new Dictionary<string, PropertyDefinition>();
            misleadingClaimObj.Add("claim", PropertyDefinition.DefineString("The exact text of the claim found"));
            misleadingClaimObj.Add("reasoning", PropertyDefinition.DefineString("An explanation of why this claim is misleading or false."));

            var MisleadingClaimObject = PropertyDefinition.DefineObject(misleadingClaimObj, null, null, null, null);

            var MisleadingClaimsArray = PropertyDefinition.DefineArray(MisleadingClaimObject);

            var fn1 = new FunctionDefinitionBuilder("FlagAsIrrelevant", "Report that this text is not unrelated to alternative health.")
                .Validate()
                .Build();

            var fn2 = new FunctionDefinitionBuilder("ReportClaim", "Reports a misleading or false claim.")
                .AddParameter("misleadingClaims", MisleadingClaimsArray)
                .Validate()
                .Build();

            var fn3 = new FunctionDefinitionBuilder("ReportNoFalseClaims", "Report that this text contains no false claims.")
                .Validate()
                .Build();

            this.FunctionDefinitions = new List<FunctionDefinition> { fn1, fn2, fn3 };
        }


        private static Dictionary<int, PromptSpecification> _promptSpecifications = null;

        public static Dictionary<int, PromptSpecification> PromptSpecifications {
            get
            {
                if(_promptSpecifications == null)
                {
                    _promptSpecifications= new Dictionary<int, PromptSpecification>();
                    _promptSpecifications[2] = FalseMisleadingHealthClaims2;


                }
                return _promptSpecifications;
            }
        }



        private static PromptSpecification FalseMisleadingHealthClaims2
        {
            get
            {
                var p = new PromptSpecification();
                //p.PromptReferenceNumber = 2;
                p.Name = "False/Misleading Health Claims v2";
                p.Model = "gpt-4-1106-preview";
                p.PromptTemplate = @"Upon receiving the following piece of INPUT TEXT from an alternative medicine website:

--- BEGIN INPUT TEXT ---
{text}
--- END INPUT TEXT ---

Proceed as follows without additional commentary:

1. Determine if the overall content is related to alternative medicine practices, therapies, or claims.

2. If the content is unrelated to alternative medicine, invoke the FlagAsIrrelevant() function.

3. If the content is related, carefully review the text for any explicit or implied claims about the effectiveness of treatments in curing, treating, or managing illnesses or diseases.

4. Compile a list of EXACT quotations of any claims that are scientifically unsupported or have been refuted by established medical research. For each quote, also prepare a brief explanation of why it is considered false or misleading.

5. Once all quotes and their explanations have been compiled, call the ReportClaim() function, providing the list of quotes and corresponding reasoning.

6. If there are no false or misleading claims to report, use the ReportNoFalseClaims() function to indicate that no such claims were found.

Act on these directives succinctly.";




                return p;
            }
        }

    }
}
