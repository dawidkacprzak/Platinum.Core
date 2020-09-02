using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using Platinum.Core.Types.Exceptions;

namespace Platinum.Core.ApiIntegration
{
    public class HttpClientInstance : RestClient
    {
        private static HttpClient client;
        public string LastResponse { get; set; }
        public string LastRequestedUrl { get; set; }
        public Exception LastException;
        private int retry = 0;
        public HttpClientInstance()
        {
            client = new HttpClient();
        }
        public void OpenUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new RequestException("Cannot open website with empty url");
            }
            string response = string.Empty;
            try
            {
                response = client.GetStringAsync(url).GetAwaiter().GetResult();
                retry = 0;
                System.Diagnostics.Debug.WriteLine("Req success");
                LastResponse = response;
                LastRequestedUrl = url;
            }
            catch (HttpRequestException ex)
            {
                Thread.Sleep(5000*retry);
                retry++;
                LastException = ex;
                LastResponse = string.Empty;
                LastRequestedUrl = url;
                System.Diagnostics.Debug.WriteLine("Req fail");

                throw;
            }
            catch (Exception ex)
            {
                LastException = ex;
                LastResponse = string.Empty;
                LastRequestedUrl = url;
                System.Diagnostics.Debug.WriteLine("Req fail");

                throw;
            }
        }

        public string GetTitleFromLastWebsite()
        {
            if (LastException != null)
            {
                 return Regex.Match(LastResponse, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>",
                    RegexOptions.IgnoreCase).Groups["Title"].Value;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}