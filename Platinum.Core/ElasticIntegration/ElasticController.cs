using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using Platinum.Core.Model;
using Platinum.Core.Model.Elastic;

namespace Platinum.Core.ElasticIntegration
{
    public class ElasticController
    {
        private static ElasticClient client;
        private static ElasticController instance;
        private static object padlock = new object();
        Logger _logger = LogManager.GetCurrentClassLogger();

        public static ElasticController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ElasticController();
                }
                return instance;
            }
        }

        private ElasticController()
        {
            ConnectionSettings settings = new ConnectionSettings(new Uri(ElasticConfiguration.ELASTIC_HOST));
            settings.GlobalHeaders(new System.Collections.Specialized.NameValueCollection()
            {
                {"authorization","opnsdgsd353sapgqejpg"}
            });
            client = new ElasticClient(settings);
        }

        public void InsertOffer(Offer offer)
        {

            client.Index(new ELBufforedOffers(offer), i => i.Index("buffered_offers"));

        }
        public bool InsertOffer(string offer)
        {
            var response = client.Index(new ELBufforedOffers(offer), i => i.Index("buffered_offers"));
            return response.IsValid;
        }

        public bool InsertOfferDetails(OfferDetails offerDetails, int userId,int categoryId)
        {
            lock (padlock)
            {
                string index = userId + "_cat" + categoryId;
                if (userId == 1)
                {
                    IndexResponse status = client.Index(offerDetails, i => i.Index("offer_details"));
                    return status.IsValid;
                }
                else
                {
                    var ex = client.Indices.Exists(index);
                    if(ex.Exists == false)
                    {
                        offerDetails.CreatedDate = DateTime.Now;
                        var res = client.Indices.Create(index, c => c
                            .Map<OfferDetails>(x => x.AutoMap()).Settings(s =>
                                s.NumberOfReplicas(2).NumberOfShards(2)));
                        if (!res.IsValid || !res.Acknowledged)
                        {
                            throw new Exception("Cannot create index: " + index);
                        }
                    }
                    IndexResponse status = client.Index<OfferDetails>(offerDetails, i => i.Index(index));
                    return status.IsValid;
                }
            }
        }

        public bool OfferDetailsExists(int offerId)
        {
            var searchResponse = client.Search<OfferDetails>(s => s.Index("offer_details")
                .From(0)
                .Size(10)
                .Query(q =>
                    q
                        .MatchPhrase(c => c
                            .Field(p => p.Id)
                            .Query(offerId.ToString())
                        )
                )
            );

            return searchResponse.Documents.Count > 0;
        }

        public bool OfferExistsInBuffor(string uri)
        {

            var searchResponse = client.Search<ELBufforedOffers>(s => s.Index("buffered_offers")
                .From(0)
                .Size(10)
                .Query(q =>
                    q
                        .MatchPhrase(c => c
                            .Field(p => p.uri)
                            .Query(uri)
                        )
                )
            );
            return searchResponse.Documents.Count > 0;
        }

        public long GetIndexDocumentCount(int categoryId, int userId)
        {
            try
            {
                var indicies = client.Indices.Get(Indices.All);
                string indexName = userId + "_cat" + categoryId;
                if (userId == 1)
                {
                    indexName = "offer_details";
                }
                List<string> filteredIndicies = indicies.Indices.Where(x => x.Key.Name.ToLower().Contains(indexName))
                    .Select(x => x.Key.Name).ToList();
                if (filteredIndicies.Count > 0)
                {
                    return client.Count<OfferDetails>(x => x.Index(filteredIndicies.First())).Count;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return 0;
            }
        }
    }
}