﻿using System;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using Platinum.Core.Model;
using Platinum.Core.Model.Elastic;

namespace Platinum.Core.ElasticIntegration
{
    public class BufforController
    {
        private static ElasticClient client;
        private static BufforController instance;
        private static object padlock = new object();
        Logger _logger = LogManager.GetCurrentClassLogger();

        public static BufforController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BufforController();
                }

                return instance;
            }
        }

        private BufforController()
        {
            client = new ElasticClient(new ConnectionSettings(new Uri(ElasticConfiguration.ELASTIC_HOST)));
        }

        public void InsertOffer(Offer offer)
        {
#if RELEASE
            client.Index(new ELBufforedOffers(offer), i => i.Index("buffered_offers"));
#endif
        }

        public void InsertOffer(string offer)
        {
            var response = client.Index(new ELBufforedOffers(offer), i => i.Index("buffered_offers"));
            _logger.Info(JsonConvert.SerializeObject(response));
        }

        public void InsertOfferDetails(OfferDetails offerDetails)
        {
            #if RELEASE
            lock (padlock)
            {
                IndexResponse status = client.Index(offerDetails, i => i.Index("offer_details"));
            }
            #endif
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
            _logger.Info(JsonConvert.SerializeObject(searchResponse));
            return searchResponse.Documents.Count > 0;
        }
    }
}