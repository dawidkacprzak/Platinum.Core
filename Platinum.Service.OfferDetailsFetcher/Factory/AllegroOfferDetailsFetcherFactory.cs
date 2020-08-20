using Platinum.Core.Types;

namespace Platinum.Service.OfferDetailsFetcher.Factory
{
    public class AllegroOfferDetailsFetcherFactory : OfferDetailsFetcherFactory
    {
        public override IOfferDetailsFetcher GetOfferDetailsFetcher(int countOfTasks)
        {
            return new AllegroOfferDetailsFetcher(countOfTasks);
        }
    }
}