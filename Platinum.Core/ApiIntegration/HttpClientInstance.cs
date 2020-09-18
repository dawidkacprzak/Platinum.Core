using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using NLog;
using Platinum.Core.Types.Exceptions;

namespace Platinum.Core.ApiIntegration
{
    public class HttpClientInstance : RestClient
    {
        private HttpClient client;
        public string LastResponse { get; set; }
        public string LastRequestedUrl { get; set; }
        public Exception LastException;
        private static int retry = 0;
        readonly private Logger logger = LogManager.GetCurrentClassLogger();

        private int CalcRetryTimeout
        {
            get
            {
                if (retry <= 10)
                {
                    return retry * 5000;
                }
                else
                {
                    return 5000 * 5;
                }
            }
        }

        public HttpClientInstance()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("viewport-width","1920");
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-GB"));
            client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue("en-US"));
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.102 Safari/537.36");
            
        }

        public void OpenUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new RequestException("Cannot open website with empty url");
            }

            logger.Info($"Request to: {url}");
            string response = string.Empty;
            try
            {
                response = client.GetStringAsync(url).GetAwaiter().GetResult();
                LastResponse = response;
                LastRequestedUrl = url;
                retry -= 1;
                if (retry < 0) retry = 0;
            }
            catch (HttpRequestException ex)
            {
                Thread.Sleep(CalcRetryTimeout);
                retry += 2;
                LastException = ex;
                LastResponse = string.Empty;
                LastRequestedUrl = url;
                logger.Info($"Request to: {url} failed");

                throw;
            }
            catch (Exception ex)
            {
                Thread.Sleep(CalcRetryTimeout);
                retry += 2;
                LastException = ex;
                LastResponse = string.Empty;
                LastRequestedUrl = url;
                logger.Info($"Request to: {url} failed");

                throw;
            }
        }

        public string GetTitleFromLastWebsite()
        {
            if (LastException == null)
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