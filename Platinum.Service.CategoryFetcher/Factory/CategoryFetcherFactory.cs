using Platinum.Core.Types;
using RestSharp;

namespace Platinum.Service.CategoryFetcher.Factory
{
    public abstract class CategoryFetcherFactory
    {
        public abstract ICategoryFetcher GetFetcher();
        public abstract ICategoryFetcher GetFetcher(IRest client);

    }
}