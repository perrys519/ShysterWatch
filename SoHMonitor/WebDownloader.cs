using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ShysterWatch
{
    public class WebDownloader
    {

    }



    class BrowserSession
    {
        CookieContainer cookieContainer = new CookieContainer();


        public StringBuilder BuildPostDataFromChromeCapture(string chromeCapture)
        {
            var postData = new StringBuilder();

            chromeCapture = chromeCapture.Replace("\r\n", "\n");
            var dataArray = chromeCapture.Split('\n');
            foreach (var postItem in dataArray)
            {
                var colonIndex = postItem.IndexOf(": ");
                if (colonIndex != -1)
                {
                    var name = postItem.Substring(0, colonIndex);
                    var value = postItem.Substring(colonIndex + 2);


                    postData.Append(String.Format("{0}={1}&", HttpUtility.UrlEncode(name), HttpUtility.UrlEncode(value)));
                }


            }

            return postData;
        }


        public WebHit CreateWebHit(string url)
        {

            return new WebHit(url, cookieContainer);
        }


        


    }

    class WebHit
    {
        public HttpWebRequest HttpWebRequest;
        //public HttpWebResponse HttpWebResponse;
        public Uri url;

        

        CookieContainer CookieContainer;

        public WebHit(string url, CookieContainer cookieContainer)
        {
            this.url = new Uri(url);
            this.CookieContainer = cookieContainer;


        }

        
        public Dictionary<string, string> PostParameters = null;


        public bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public string ChromeRequestHeaderDump = "";





        void SetUpCall()
        {
            //if (SetUpCompleted) return;

            HttpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            HttpWebRequest.CookieContainer = CookieContainer;
            HttpWebRequest.KeepAlive = false;
            

            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            if ((ChromeRequestHeaderDump == "") && false)
            {
                ChromeRequestHeaderDump = ChromeRequestHeaderDump.Replace("\n", "");
                var lines = ChromeRequestHeaderDump.Split('\r');

                string[] HeadersToAvoid = { "content-length", "accept", "content-type", "referer", "user-agent" };
                var listHeadersToAvoid = HeadersToAvoid.Select(x => x).ToList<string>();

                foreach (var line in lines)
                {

                    var colonIndex = line.IndexOf(": ", 1);
                    if (colonIndex != -1)
                    {

                        var name = line.Substring(0, colonIndex);
                        var val = line.Substring(colonIndex + 2);

                        if (name.IndexOf(":") == -1)
                        {


                            if (!listHeadersToAvoid.Contains(name))
                            {
                                //Debug.WriteLine(name + " ---> " + val);

                                if (HttpWebRequest.Headers[name] == null)
                                {
                                    HttpWebRequest.Headers.Add(name, val);
                                }


                                HttpWebRequest.Headers[name] = val;

                            }
                            else
                            {
                                if (name == "user-agent") HttpWebRequest.UserAgent = val;
                                if (name == "referer") HttpWebRequest.Referer = val;
                                if (name == "content-type") HttpWebRequest.ContentType = val;
                                if (name == "accept") HttpWebRequest.Accept = val;
                            }

                        }

                    }

                }
            }
        }


        public bool ErrorOccurred = false;

        
        

        public WebException WebException = null;


        static HttpClient _client = null;
        static HttpClient client
        {
            get
            {
                if(_client == null)
                {
                    _client = new HttpClient();

                    _client.DefaultRequestHeaders.Add(@"Pragma", @"no-cache");
                    _client.DefaultRequestHeaders.Add(@"Cache-Control", @"no-cache");
                    _client.DefaultRequestHeaders.Add(@"Accept",@"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
                    //                    _client.DefaultRequestHeaders.Add(@"Accept-Encoding","gzip, deflate, br");
                    _client.DefaultRequestHeaders.Add(@"Accept-Encoding","gzip");
                    _client.DefaultRequestHeaders.Add(@"Accept-Language","en-GB,en;q=0.9,fr-FR;q=0.8,fr;q=0.7,en-US;q=0.6");
                    _client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                //    _client.DefaultRequestHeaders.Add(@"");
                //    _client.DefaultRequestHeaders.Add(@"");



                    _client.Timeout = new TimeSpan(0, 0, 20);
                }
                return _client;
            }
        }



        public async Task<HttpResponseMessage> GetHeaders()
        {
            

            HttpRequestMessage request = new HttpRequestMessage();
            
            request.Method = HttpMethod.Head;
            request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            request.RequestUri = this.url;


            HttpResponseMessage response = await client.SendAsync(request);


            return response;

        }



        public async Task<HttpResponseMessage> Call()
        {

            HttpResponseMessage response = await client.GetAsync(url);
            return response;


            ////If there is post data, construct it
            //if (PostParameters != null)
            //{
            //    string postData = "";

            //    foreach (string key in PostParameters.Keys)
            //    {
            //        postData += HttpUtility.UrlEncode(key) + "="
            //              + HttpUtility.UrlEncode(PostParameters[key]) + "&";
            //    }


            //    HttpWebRequest.Method = "POST";


            //    byte[] data = Encoding.ASCII.GetBytes(postData);

            //    HttpWebRequest.ContentType = "application/x-www-form-urlencoded";
            //    HttpWebRequest.ContentLength = data.Length;



            //    Stream requestStream = HttpWebRequest.GetRequestStream();
            //    requestStream.Write(data, 0, data.Length);
            //    requestStream.Close();
            //}
            //else
            //{
            //    HttpWebRequest.Method = "GET";
            //}




            //try
            //{

            //    Debug.WriteLine("   -Call 3");

            //    var task = HttpWebRequest.GetResponseAsync();
            //    Debug.WriteLine("   -Call 4");

            //    try
            //    {
            //        HttpWebResponse = (HttpWebResponse)await task;
            //    }catch
            //    {
            //        Debug.WriteLine("   -Call exception!");
            //    }
            //    Debug.WriteLine("   -Call 5");



            //}
            //catch (Exception e)
            //{
            //    //WebException = e;
            //    ErrorOccurred = true;
            //    Debug.WriteLine("   -Call 6");
            //}


            //if (HttpWebResponse == null)
            //{
            //    ErrorOccurred = true;
            //    return;
            //}





        }





    }


}
