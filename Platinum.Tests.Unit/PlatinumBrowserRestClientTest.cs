using NUnit.Framework;
using Platinum.Core.ApiIntegration;
using Platinum.Core.Types;

namespace Platinum.Tests.Unit
{
    public class PlatinumBrowserRestClientTest
    {
        [Test]
        public void ConstructorNoArgumentTestPass()
        {
            IPlatinumBrowserRestClient client = new PlatinumBrowserRestClient();
            Assert.IsInstanceOf<PlatinumBrowserRestClient>(client);
        }
        
        [TestCase("https://google.pl")]
        [TestCase("https://allegro.pl")]
        public void ConstructorArgumentTestPass(string url)
        {
            PlatinumBrowserRestClient client = new PlatinumBrowserRestClient(url);
            Assert.AreEqual(client.ApiUrl,url);
            Assert.IsInstanceOf<PlatinumBrowserRestClient>(client);
        }
    }
}