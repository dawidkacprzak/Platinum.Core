using System.Collections.Generic;
using System.Threading.Tasks;
using Platinum.Core.Model;

namespace Platinum.Core.Types
{
    public interface IOfferDetailsFetcher
    {
        IEnumerable<Offer> GetLastNotProcessedOffers(IDal dal, int count);
        public void SetOffersAsInProcess(IDal dal, IEnumerable<Offer> offers);
        public void SetOfferAsProcessed(IDal dal, Offer offer);
        public void SetOfferAsUnprocessed(IDal dal, Offer offer);
        public Task CreateTaskForProcessOrder(IDal dal, Offer offer, IOfferDetailsParser parser);
        public void Run(IDal dal);
    }
}