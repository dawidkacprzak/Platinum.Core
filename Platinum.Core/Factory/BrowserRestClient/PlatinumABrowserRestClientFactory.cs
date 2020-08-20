using System.Diagnostics.CodeAnalysis;
using Platinum.Core.ApiIntegration;
using Platinum.Core.Types;

namespace Platinum.Core.Factory.BrowserRestClient
{
    
    [ExcludeFromCodeCoverage]
    public class PlatinumABrowserRestClientFactory : ABrowserRestClientFactory
    {
        public override IBrowserClient GetBrowser(string host)
        {
            return new PlatinumBrowserClient(host);
        }
    }
}