using System;
using System.Collections.Generic;
using Nest;
using Platinum.Core.Model.Elastic;

namespace Platinum.Service.OfferDetailsFetcher
{
    public class AllegroOfferFetch
    {
        private ElasticClient _client;
        
        public void Run()
        {


        }
    }
}


/* this._client = client;
 ELOffer offer = new ELOffer()
 {
     attributes = new List<KeyValuePair<string, string>>()
     {
         new KeyValuePair<string, string>("test", "test"),
         new KeyValuePair<string, string>("t", "342")
     },
     price = 15.5f,
     quanity = 4f,
     uri = "https://test.pl",
     createdDate = DateTime.Now,
     offerId = 1
                 var x = _client.Index(offer, p => p.Index("offer"));

 };*/