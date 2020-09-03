using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Platinum.ClientAPI.Model.Oponeo
{
    public class OfferDetails
    {
        [JsonProperty("attributes")]
        public Dictionary<string,string> Attributes { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("price")]
        public float Price { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("websiteId")]
        public long WebsiteId { get; set; }

        [JsonProperty("uri")]
        public Uri Uri { get; set; }

        [JsonProperty("createdDate")]
        public DateTimeOffset CreatedDate { get; set; }

        [JsonProperty("websiteCategoryId")]
        public long WebsiteCategoryId { get; set; }

        [JsonProperty("processed")]
        public long Processed { get; set; }
        [JsonProperty("UriHash")]
        public string UriHash { get; set; }
        public double PriceForOne { get; set; }
        public int QuantityInOffer { get; set; }

    }
}
