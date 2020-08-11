using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using Nest;
using Newtonsoft.Json;
using NLog;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Model;
using Platinum.Core.Types;
using Platinum.Core.Types.Exceptions;
using RestSharp;
using static Platinum.Core.AllegroAPIIntegration.AllegroApiConfiguration;
using RestClient = Platinum.Core.ApiIntegration.RestClient;

namespace Platinum.Service.CategoryFetcher
{
    public class AllegroCategoryFetcher : ICategoryFetcher
    {
        private IRest client;
        private string accessToken;
        private int MAX_CATEGORY_ID = 500000;
        readonly private Logger logger = LogManager.GetCurrentClassLogger();

        public AllegroCategoryFetcher(IRest client)
        {
            this.client = client;
        }

        public void Run(IDal db)
        {
            for (int i = 0; i < MAX_CATEGORY_ID; i++)
            {
                try
                {
                    OfferCategory cat = GetCategoryById(i);
                    if (cat != null)
                    {
                        UpdateCategoryInDb(db, cat);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            }
        }

        public void SetIndexCategoryFetchLimit(int limit)
        {
            MAX_CATEGORY_ID = limit;
        }

        public bool CategoryIdExistInDb(IDal db, int websiteCategoryId)
        {
            int count = (int) db.ExecuteScalar(
                $"SELECT COUNT(*) from websiteCategories with(nolock) where websiteId={(int) EOfferWebsite.Allegro} AND websiteCategoryId={websiteCategoryId}");
            return count > 0;
        }

        public void UpdateCategoryInDb(IDal db, OfferCategory category)
        {
            if (!CategoryIdExistInDb(db, category.WebsiteCategoryId))
            {
                db.ExecuteNonQuery(
                    $"INSERT INTO websiteCategories VALUES({(int) EOfferWebsite.Allegro},'{category.CategoryName}','{category.CategoryUrl}',{category.WebsiteCategoryId})");
            }
        }


        public OfferCategory GetCategoryById(int id)
        {
            Thread.Sleep(50);
            IRestResponse response = CreateRequest("sale/categories/" + id);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                logger.Info("Not found category id: " + id);
                return null;
            }

            AllegroCategoryModel category = JsonConvert.DeserializeObject<AllegroCategoryModel>(response.Content);
            return new OfferCategory(EOfferWebsite.Allegro, int.Parse(category.id), category.name);
        }

        private IRestResponse CreateRequest(string route, bool tokenRequested = false)
        {
            string url = ALLEGRO_API_BASE_URL + route;
            RestRequest request = new RestRequest(new Uri(url));
            request.AddHeader("accept", "application/vnd.allegro.public.v1+json");
            request.AddHeader("Authorization", $"Bearer {accessToken}");
            try
            {
                IRestResponse response = client.Get(request);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    SetAccessToken();
                    return CreateRequest(route, true);
                }
                else
                {
                    return response;
                }
            }
            catch (RequestException ex)
            {
                return HandleRequestError(ex, route, tokenRequested);
            }
        }

        private IRestResponse HandleRequestError(RequestException ex, string route, bool tokenRequested)
        {
            if (ex.Response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                Thread.Sleep(80000);
            }

            if (ex.Response.StatusCode == HttpStatusCode.NotFound)
            {
                return ex.Response;
            }

            if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
            {
                SetAccessToken();
                return CreateRequest(route, true);
            }

            if (!tokenRequested)
            {
                SetAccessToken();
                return CreateRequest(route, true);
            }
            else throw ex;
        }

        private void SetAccessToken()
        {
            string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{CLIENT_ID}:{CLIENT_SECRET}"));
            RestRequest request = new RestRequest(new Uri("https://allegro.pl/auth/oauth/token"));
            request.AddParameter("grant_type", "client_credentials");
            request.AddHeader($"Authorization", $"Basic {base64}");
            IRestResponse response = client.Post(request);
            AllegroClientToken token = JsonConvert.DeserializeObject<AllegroClientToken>(response.Content);
            this.accessToken = token.access_token;
        }
    }
}