using Platinum.Core.ApiIntegration;
using Platinum.Core.Types;

namespace Platinum.Service.CategoryFetcher.Factory
{
    public class AllegroCategoryFetcherFactory : CategoryFetcherFactory
    {
        public override ICategoryFetcher GetFetcher() => new AllegroCategoryFetcher(new RestClient());
        public override ICategoryFetcher GetFetcher(IRest client)
        {
            return new AllegroCategoryFetcher(client);
        }
    }
}