namespace Platinum.Core.Model.Elastic
{
    public class ELBufforedOffers
    {
        public string uri { get; set; }

        public ELBufforedOffers(string url)
        {
            this.uri = url;
        }

        public ELBufforedOffers(Offer offer)
        {
            this.uri = offer.Uri;
        }
    }
}