using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Mvc;
using Nest;
using Newtonsoft.Json;
using NLog;
using Platinum.ClientAPI.Auth;
using Platinum.ClientAPI.Model.Oponeo;
using Platinum.Core.ElasticIntegration;

namespace Platinum.ClientAPI.Controllers.Clients.Client_2
{
    [ApiController]
    [Route("[controller]")]
    [BasicAuth("oskar")]
    public class Client2Controller
    {
        ElasticClient client;
        Logger _logger = LogManager.GetCurrentClassLogger();

        public Client2Controller()
        {
            ConnectionSettings settings = new ConnectionSettings(new Uri(ElasticConfiguration.ELASTIC_HOST));
            settings.DisableDirectStreaming();
            settings.GlobalHeaders(new System.Collections.Specialized.NameValueCollection()
            {
                {"authorization", "opnsdgsd353sapgqejpg"},
                {"http_authorization", "opnsdgsd353sapgqejpg"}
            });
            client = new ElasticClient(settings);
        }

        [HttpGet("{pageId}")]
        public string GetOffers(int pageId)
        {
            QueryContainer producentQuery = new QueryContainer();
            List<QueryContainer> titleQueries = new List<QueryContainer>();
            
            
            IRequestConfiguration conf = new RequestConfiguration();
            conf.Headers = new System.Collections.Specialized.NameValueCollection();
            conf.Headers.Add(new System.Collections.Specialized.NameValueCollection()
            {
                {"authorization","opnsdgsd353sapgqejpg"},
                {"http_authorization","opnsdgsd353sapgqejpg"}
            });
            
            SearchRequest request = new SearchRequest<OfferDetails>("2_cat16520")
            {
                Size = 100,
                From = pageId * 100
            };
            
            request.RequestConfiguration = conf;
            var k = client.Search<OfferDetails>(request);
            for (int x = 0; x < k.Documents.Count; x++)
            {
                k.Documents.ElementAt(x).Id = k.Hits.ElementAt(x).Source.Id;
            }
            
            for (int i = 0; i < k.Documents.Count; i++)
            {
                foreach (var attr in k.Documents.ElementAt(i).Attributes)
                {
                        k.Documents.ElementAt(i).PriceForOne = k.Documents.ElementAt(i).Price;
                        k.Documents.ElementAt(i).QuantityInOffer = 1;
                    
                }
            }


            return JsonConvert.SerializeObject(k.Documents);
        }
    }
}