using Platinum.Core.Model;

namespace Platinum.Core.Types
{
    public interface IOfferDetailsParser
    {
        public OfferDetails GetPageDetails(string pageUrl, Offer offer);
    }
}