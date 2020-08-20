using Newtonsoft.Json;
using NLog;
using Platinum.Core.Types;
using RestSharp;

namespace Platinum.Core.ApiIntegration
{
    public class PlatinumBrowserRestClient : RestClient, IBrowserRestClient
    {
        readonly private Logger logger = LogManager.GetCurrentClassLogger();
        public string ApiUrl { get; set; }

        public PlatinumBrowserRestClient()
        {
            ApiUrl = "http://localhost:3001";
        }

        public PlatinumBrowserRestClient(string apiUrl)
        {
            this.ApiUrl = apiUrl;
        }
        public void InitBrowser()
        {
            Get(new RestRequest(ApiUrl + "/init"));
        }

        public string CreatePage()
        {
            logger.Info("Append to create page");
            string pageId = Get(new RestRequest(ApiUrl + "/createPage")).Content;
            return JsonConvert.DeserializeObject<MessageResponse>(pageId).message;
        }

        public void Open(string pageId,string url)
        {
            logger.Info("Append to open: " + ApiUrl + "/open?url="+System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(url))+"&pageid="+pageId);
            Get(new RestRequest(ApiUrl + "/open?url="+System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(url))+"&pageid="+pageId));
        }

        public void ClosePage(string pageId)
        {
            Get(new RestRequest(ApiUrl + "/closepage?pageid="+pageId));
        }
        
        public void RefreshPage(string pageId)
        {
            Get(new RestRequest(ApiUrl + "/refresh?pageid="+pageId));
        }
        
        public void CloseBrowser()
        {
            Get(new RestRequest(ApiUrl + "/closebrowser"));
        }

        public string CurrentSiteSource(string pageId)
        {
            IRestResponse response = Get(new RestRequest(ApiUrl + "/currentSiteSource?pageid="+pageId));
            return response.Content;
        }

        public string CurrentSiteHeader(string pageId)
        {
            IRestResponse response = Get(new RestRequest(ApiUrl + "/currentSiteHeader?pageid="+ pageId));
            return response.Content;
        }
        public void ResetBrowser()
        {
            Get(new RestRequest(ApiUrl + "/resetBrowser"));
        }
        public void DeInit()
        {
            Get(new RestRequest(ApiUrl + "/test/deinit"));
        }

        private class MessageResponse
        {
            [JsonProperty("message")]
            public string message { get; set; }
        }
    }
}