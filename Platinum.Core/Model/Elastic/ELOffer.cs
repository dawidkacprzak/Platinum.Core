using System;
using System.Collections.Generic;

namespace Platinum.Core.Model.Elastic
{
    public class ELOffer
    {
        public string uri { get; set; }
        public float quanity { get; set; }
        public float price { get; set; }
        public int offerId { get; set; }
        public DateTime createdDate { get; set; }
        public List<KeyValuePair<string,string>> attributes { get; set; }
    }
}