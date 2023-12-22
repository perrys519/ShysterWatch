using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenQA.Selenium;
using CoreTweet;
using System.Diagnostics;
using System.IO;
using AngleSharp;
using NReadability;
using System.Web.UI.WebControls;
using OpenAI;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.ResponseModels;
using System.Threading;
using Newtonsoft.Json;
using OpenQA.Selenium.DevTools.V85.Target;

namespace ShysterWatch.ComplaintGenerator
{



    public class Tools
    {


        public delegate void SendMessage(string message);

        static int PriorTokens = 0;
        static DateTime PriorTime = DateTime.Now;

        public async static Task<bool> ExtractAndSaveClaimsToTreat(WebPageVersion webPageVersion, SendMessage messageDelegate, PromptSpecification promptSpecification)
        {
            //var text2 = ShysterWatch.ComplaintGenerator.Tools.ExtractTextFromHtml(webPageVersion.SavedHtml);


            var response = await ShysterWatch.ComplaintGenerator.Tools.ExtractClaimsToTreat(webPageVersion, messageDelegate, promptSpecification);




            string debugText = "";

            if ((response.Choices == null))
            {

                debugText = $@"{{
  Success:false,
  Reason:""Response from ChatGPT failed: {response.Error.Message}""
}}";
            }
            else
            {
                PriorTime = DateTime.Now;
                PriorTokens = response.Usage.TotalTokens;


                var functionCallName = "";
                var functionCallArguments = "";
                if (response.Choices[0].Message.FunctionCall != null)
                {
                    functionCallName = response.Choices[0].Message.FunctionCall.Name;
                    functionCallArguments = response.Choices[0].Message.FunctionCall.Arguments;
                }
                else
                {
                    //handle the bug whereby ChatGPT4 puts the function call within ```plaintext
                    var msgContent = response.Choices[0].Message.Content;


                    //Regex regexFunction = new Regex(@"```plaintext\W*functions\.(?<functionName>[A-Za-z0-9\-_]*)\((?<arguments>[\s\S]*)\)\W*```");
                    Regex regexFunction = new Regex(@"\W*functions\.(?<functionName>[A-Za-z0-9\-_]*)\((?<arguments>[\s\S]*)\)\W*```");
                    var functionMatch = regexFunction.Match(msgContent);

                    if (functionMatch.Success)
                    {
                        functionCallName = functionMatch.Groups["functionName"].Value;
                        functionCallArguments = functionMatch.Groups["arguments"].Value;
                    }

                }



                if (functionCallName != "")
                {

                    switch (functionCallName)
                    {
                        case "ReportClaim":
                            {


                                var claims = new MisleadingClaimsList();




                                debugText += $@"{{
  Success:true,
  PromptTokens:{response.Usage.PromptTokens},
  CompletionTokens:{response.Usage.CompletionTokens},
  TotalTokens:{response.Usage.TotalTokens},
  FunctionCallResponse:{functionCallArguments},
  MessageContent:{JsonConvert.SerializeObject(response.Choices[0].Message.Content)}
}}
";
                            }

                                break;



                        case "ReportNoFalseClaims":
                            {
                                debugText = $@"{{Success:false,
  Reason:""No false claims found"",
  PromptTokens:{response.Usage.PromptTokens},
  CompletionTokens:{response.Usage.CompletionTokens},
  TotalTokens:{response.Usage.TotalTokens},
  MessageContent:{JsonConvert.SerializeObject(response.Choices[0].Message.Content)}
}}";

                            }
                            break;
                        case "FlagAsIrrelevant":
                            {
                                debugText = $@"{{Success:false,
  Reason:""Text Irrelevant"",
  PromptTokens:{response.Usage.PromptTokens},
  CompletionTokens:{response.Usage.CompletionTokens},
  TotalTokens:{response.Usage.TotalTokens},
  MessageContent:{JsonConvert.SerializeObject(response.Choices[0].Message.Content)}
}}";

                            }
                            break;

                        default:
                            {
                                messageDelegate($"Error: functionCallname={functionCallName} ");
                            }
                            break;
                    }




                }
                else
                {
                    messageDelegate("Error: functionCallName is empty.");
                }




            }

            if(debugText == "")
            {
                messageDelegate($"Error: debugText is empty. .");

                //throw new Exception("Problem! Empty text about to be written to json file.");
                return false;
            }

            File.WriteAllText(webPageVersion.ChatGPT35ExtractedClaimsFilePath(promptSpecification.PromptReferenceNumber), debugText);
            

            return true;

        }


        public async static Task<ChatCompletionCreateResponse> ExtractClaimsToTreat(WebPageVersion webPageVersion, SendMessage messageDelegate, PromptSpecification promptSpec)
        {
            var prompt = "";
            var text = webPageVersion.LoadInnerText();
            var uText = text.ToUpper();
            List<string> disciplinesPresent = new List<string>();


          {
//                prompt = @"INPUT TEXT:
//```
//" + text + @"
//```

//Given the above piece of INPUT TEXT from an alternative medicine website, find EXACT quotes that make false or misleading claims related to helping with illness or disease. 

//Report these claims using the ReportClaim function. If there are none, reply ""NONE""";



//                prompt = @"INPUT TEXT:
//```
//" + text + @"
//```

//Given the above piece of INPUT TEXT from an alternative medicine website, first verify that the text is relevant to alternative medicine. If it is not, call the FlagAsIrrelevant function.

//If so, find EXACT quotes that make false or misleading claims related to helping with illness or disease. 

//Report these claims using the ReportClaim function. If there are none, call ReportNoFalseClaims.";


//                prompt = $@"Upon receiving the following piece of INPUT TEXT from an alternative medicine website:

//--- BEGIN INPUT TEXT ---
//{text}
//--- END INPUT TEXT ---

//Proceed as follows without additional commentary:

//1. Determine if the overall content is related to alternative medicine practices, therapies, or claims.

//2. If the content is unrelated to alternative medicine, invoke the FlagAsIrrelevant() function.

//3. If the content is related, carefully review the text for any explicit or implied claims about the effectiveness of treatments in curing, treating, or managing illnesses or diseases.

//4. Compile a list of EXACT quotations of any claims that are scientifically unsupported or have been refuted by established medical research. For each quote, also prepare a brief explanation of why it is considered false or misleading.

//5. Once all quotes and their explanations have been compiled, call the ReportClaim() function, providing the list of quotes and corresponding reasoning.

//6. If there are no false or misleading claims to report, use the ReportNoFalseClaims() function to indicate that no such claims were found.

//Act on these directives succinctly.";

                prompt = promptSpec.Prompt(text);

                File.WriteAllText(webPageVersion.ChatGPT35ExtractedClaimsFilePath(promptSpec.PromptReferenceNumber) + " - prompt.txt", prompt);
            }




            //Debug.WriteLine(prompt);

            var response = await GetChatGPTResponse(prompt, messageDelegate, promptSpec);
            return response;
        }

        public static int PromptTokensUsedThisSession = 0;
        public static int CompletionTokensUsedThisSession = 0;


        public static Decimal CostThisSession
        {
            get
            {
                return (((Decimal)0.01 * (Decimal)PromptTokensUsedThisSession + (Decimal)0.03 * (Decimal)CompletionTokensUsedThisSession)) / (Decimal)1000;
            }
        }


        public static async Task<ChatCompletionCreateResponse> GetChatGPTResponse(string prompt, SendMessage messageDelegate, PromptSpecification promptSpec, float temperature=0F)
        {



            

            //IDictionary<string, PropertyDefinition> misleadingClaimObj = new Dictionary<string, PropertyDefinition>();
            //misleadingClaimObj.Add("claim", PropertyDefinition.DefineString("The exact text of the claim found"));
            //misleadingClaimObj.Add("reasoning", PropertyDefinition.DefineString("An explanation of why this claim is misleading or false."));

            //var MisleadingClaimObject = PropertyDefinition.DefineObject(misleadingClaimObj, null, null, null, null);

            //var MisleadingClaimsArray = PropertyDefinition.DefineArray(MisleadingClaimObject);

            //var fn1 = new FunctionDefinitionBuilder("FlagAsIrrelevant", "Report that this text is not unrelated to alternative health.")
            //    .Validate()
            //    .Build();

            //var fn2 = new FunctionDefinitionBuilder("ReportClaim", "Reports a misleading or false claim.")
            //    .AddParameter("misleadingClaims", MisleadingClaimsArray)
            //    .Validate()
            //    .Build();

            //var fn3 = new FunctionDefinitionBuilder("ReportNoFalseClaims", "Report that this text contains no false claims.")
            //    .Validate()
            //    .Build();

            //var functionDefinitions = new List<FunctionDefinition> { fn1, fn2, fn3 };

            ChatMessage message = new ChatMessage("user", prompt);
            ChatMessage[] chatMessages = new ChatMessage[1];



            var existingDelayMins = (DateTime.Now - PriorTime).TotalMinutes;
            var delayNeededMins = (double)PriorTokens / (double)300000;
            int extraDelayNeededMs = (int)Math.Ceiling((delayNeededMins - existingDelayMins) * 60000);
            if (extraDelayNeededMs > 0) await Task.Delay(extraDelayNeededMs);

            ChatCompletionCreateResponse completionResult = null;



            var done = false;
            while(!done)
            {

                //messageDelegate($"Using Org {openAiOrganizationKeyPtr}" + (openAiOrganizationKeyPtr == 1 ? $" until {switchToAiOrg0After.ToString()}." : ""));


                var openAIService = new OpenAIService(new OpenAiOptions()
                {
                    ApiKey = Properties.Settings.Default.OpenAIApiKey

                });

                chatMessages[0] = message;

                try
                {
                    completionResult = await openAIService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest()
                    {
                        Messages = chatMessages,
                        Model = promptSpec.Model,
                        Temperature = temperature,
                        MaxTokens = 4000,
                        Functions = promptSpec.FunctionDefinitions
                    }) ;
                }catch(System.Threading.Tasks.TaskCanceledException)
                {
                    messageDelegate("Task Cancelled Error (probably timed out).");
                }catch(Exception e)
                {
                    var msg = "" + e.InnerException.Message;
                    if(msg.IndexOf("The remote name could not be resolved") == 0)
                    {
                        messageDelegate("DNS Error (probably disconnected from Internet). Waiting 10 seconds.");
                        await Task.Delay(10000);
                    }
                    else
                    {
                        throw e;
                    }


                }
                if (completionResult != null)
                {
                    if ((completionResult.Successful == false) && (completionResult.Error.Code == "rate_limit_exceeded"))
                    {
                        int delay = 1;

                        Regex regex = new Regex(@"Please try again in ((?<mins>\d+)m){0,1}(?<seconds>\d+\.\d+)s\.");
                        Match match = regex.Match(completionResult.Error.Message);

                        if (match.Success)
                        {
                            // Convert the match to a double, round it, and then to an integer
                            double seconds = double.Parse(match.Groups["seconds"].Value);
                            double mins = 0;
                            if (match.Groups["mins"].Success)
                            {
                                mins = double.Parse(match.Groups["mins"].Value);
                            }

                            delay = (int)Math.Ceiling(seconds) + 60 * (int)Math.Ceiling(mins);

                            if(completionResult.Error.Message.IndexOf("on tokens per day (TPD): Limit") != 0)
                            {
                                //If it's a TPD error limit, sleep for 30 mins extra as will easily catch up.
                                delay += 30 * 60;
                            }

                        }

                        messageDelegate($"ChatGPT Error: {completionResult.Error.Message}");



                    }
                    else
                    {
                        done = true;
                    }
                }
            }


            if(completionResult == null)
            {
                messageDelegate("Error: completionResult is NULL.");
                throw new Exception();
            }



            var response = "";
            if (completionResult.Successful)
            {
                response = (completionResult.Choices.First().Message.Content);
 
            }
            else
            {
                response = "ERROR: " + completionResult.Error.ToString();
                messageDelegate(response);
            }

            if (completionResult.Successful == true)
            {
                PriorTokens = completionResult.Usage.TotalTokens;
                PriorTime = DateTime.Now;

                if ((completionResult.Usage != null) && (completionResult.Usage.CompletionTokens != null))
                {
                    PromptTokensUsedThisSession += (int)completionResult.Usage.PromptTokens;
                    CompletionTokensUsedThisSession += (int)completionResult.Usage.CompletionTokens;
                }
            }

            //response = response.Replace("\n", "\r\n");
            return completionResult;
        }

    }
}
