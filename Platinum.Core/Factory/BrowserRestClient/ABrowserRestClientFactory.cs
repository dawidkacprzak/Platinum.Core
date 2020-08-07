using Platinum.Core.Types;

namespace Platinum.Core.Factory.BrowserRestClient
{
    public abstract class ABrowserRestClientFactory : IBrowserRestClientFactory
    {
        public abstract IBrowserRestClient GetBrowser(string host);
    }
}