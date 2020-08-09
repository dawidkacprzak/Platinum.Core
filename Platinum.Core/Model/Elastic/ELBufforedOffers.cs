using System;

namespace Platinum.Core.Model.Elastic
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    public class ELBufforedOffers
    {
        public string uri { get; set; }
        public DateTime time_stamp { get; set; }
        public ELBufforedOffers(string url)
        {
            this.uri = url;
            this.time_stamp = DateTime.Now;
        }

        public ELBufforedOffers(Offer offer)
        {
            this.uri = offer.Uri;
            this.time_stamp = DateTime.Now;
        }
    }
}