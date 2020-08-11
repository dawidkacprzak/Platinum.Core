﻿using System;
using Nest;
using Platinum.Core.Model;
using Platinum.Core.Model.Elastic;

namespace Platinum.Core.ElasticIntegration
{
    public class BufforController
    {
        private static ElasticClient client;
        private static BufforController instance;

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
         
            client.Index(new ELBufforedOffers(offer), i => i.Index("buffered_offers"));
       
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
    }
}