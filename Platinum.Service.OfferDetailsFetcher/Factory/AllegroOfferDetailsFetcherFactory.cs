using Platinum.Core.Types;

namespace Platinum.Service.OfferDetailsFetcher.Factory
{
    public class AllegroOfferDetailsFetcherFactory : OfferDetailsFetcherFactory
    {
        public override IOfferDetailsFetcher GetOfferDetailsFetcher(string port, int countOfTasks)
        {
            return new AllegroOfferDetailsFetcher(port,countOfTasks);
        }
    }
}