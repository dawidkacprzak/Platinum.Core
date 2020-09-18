using System;
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

        public void InsertOffer(string offer)
        {
            var response = client.Index(new ELBufforedOffers(offer), i => i.Index("buffered_offers"));
        }

        public void InsertOfferDetails(OfferDetails offerDetails)
        {
            lock (padlock)
            {
                IndexResponse status = client.Index(offerDetails, i => i.Index("offer_details"));
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
#if RELEASE
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
#endif
            return false;
        }
    }
}