using Platinum.Core.Types;

namespace Platinum.Service.OfferDetailsFetcher.Factory
{
    public abstract class OfferDetailsFetcherFactory
    {
        public abstract IOfferDetailsFetcher GetOfferDetailsFetcher(int countOfTasks);
    }
}