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
            IBrowserClient client = new PlatinumBrowserClient();
            Assert.IsInstanceOf<PlatinumBrowserClient>(client);
        }
        
        [TestCase("https://google.pl")]
        [TestCase("https://allegro.pl")]
        public void ConstructorArgumentTestPass(string url)
        {
            PlatinumBrowserClient client = new PlatinumBrowserClient(url);
            Assert.AreEqual(client.ApiUrl,url);
            Assert.IsInstanceOf<PlatinumBrowserClient>(client);
        }
    }
}