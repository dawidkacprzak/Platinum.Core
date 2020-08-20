using Platinum.Core.Model;

namespace Platinum.Core.Types
{
    public interface IOfferDetailsParser : IBrowserClient
    {
        public OfferDetails GetPageDetails(string pageUrl, Offer offer);
    }
}