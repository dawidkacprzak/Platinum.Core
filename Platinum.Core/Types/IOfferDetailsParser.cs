using Platinum.Core.Model;

namespace Platinum.Core.Types
{
    public interface IOfferDetailsParser : IBrowserRestClient
    {
        public OfferDetails GetPageDetails(string pageUrl, Offer offer);
    }
}