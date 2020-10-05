using System;
using System.Collections.Generic;
using Nest;
using Platinum.Core.Types;

namespace Platinum.Core.Model
{
    public class OfferDetails : Offer
    {
        public Dictionary<string,string> Attributes { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        public OfferDetails(Offer offer) : base(offer.Id,offer.WebsiteId,offer.Uri,offer.UriHash,offer.CreatedDate,offer.WebsiteCategoryId, offer.Processed)
        {

        }
    }
}