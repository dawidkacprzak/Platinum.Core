using NUnit.Framework;
using Platinum.Core.Types.Exceptions;
using Platinum.Service.UrlTaskIvoker;

namespace Platinum.Tests.Integration
{
    [TestFixture]
    public class UrlTaskInvokerTest
    {
        [Test]
        public void ResetNotExistBrowser()
        {
            AllegroTaskInvoker invoker = new AllegroTaskInvoker();
            RequestException ex = Assert.Throws<RequestException>(() => invoker.ResetBrowser("http://notExistBrowser.pl3"));
            Assert.That(ex.Message,Contains.Substring("Cannot reset browser"));
        }
    }
}