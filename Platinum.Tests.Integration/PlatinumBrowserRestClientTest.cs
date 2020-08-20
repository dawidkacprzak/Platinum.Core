using System.Threading;
using NUnit.Framework;
using Platinum.Core.ApiIntegration;
using Platinum.Core.Types;
using Platinum.Core.Types.Exceptions;

namespace Platinum.Tests.Integration
{
    [TestFixture]
    public class PlatinumBrowserRestClientTest
    {
        [TearDown]
        public void TearDown()
        {
            Thread.Sleep(1000);
        }

        [Test]
        public void InitBrowserNoException()
        {
            IBrowserClient client = new PlatinumBrowserClient();

            Assert.DoesNotThrow(() => { client.InitBrowser(); });
            client.CloseBrowser();
        }


        [TestCase("https://allegro.pl")]
        [TestCase("https://google.pl")]
        [TestCase("")]
        public void OpenPageBrowserNotInitiated(string url)
        {
            IBrowserClient client = new PlatinumBrowserClient();
            client.CloseBrowser();
            RequestException ex = Assert.Throws<RequestException>(() => client.Open("0",url));
            Assert.That(ex, Is.Not.Null);
        }


        [Test]
        public void GetSiteSourceNotInitiated()
        {
            IBrowserClient client = new PlatinumBrowserClient();
            client.DeInit();
            client.CloseBrowser();
            RequestException ex = Assert.Throws<RequestException>(() => client.CurrentSiteSource("0"));
            Assert.That(ex,Is.Not.Null);
        }

        [Test]
        public void GetSiteSourceInitiated()
        {
            IBrowserClient client = new PlatinumBrowserClient();
            client.InitBrowser();
            string pageId = client.CreatePage();
            client.Open(pageId,"http://allegro.pl");
            Assert.DoesNotThrow(() =>
            {
                string response = client.CurrentSiteSource(pageId);
                Assert.NotNull(response);
                Assert.True(response.Contains("<div"));
                client.ClosePage(pageId);
                client.CloseBrowser();
            });
        }

        [Test]
        public void ResetBrowserNotInitiated()
        {
            IBrowserClient client = new PlatinumBrowserClient();
            client.DeInit();
            Assert.DoesNotThrow(() => client.ResetBrowser());
        }

        [Test]
        public void ResetBrowserInitiated()
        {
            IBrowserClient client = new PlatinumBrowserClient();
            client.InitBrowser();
            client.ResetBrowser();
            client.CloseBrowser();
        }

        [Test]
        public void ClosePageNotInitiated()
        {
            IBrowserClient client = new PlatinumBrowserClient();
            client.ClosePage("0");
            client.CloseBrowser();
        }

        [Test]
        public void ClosePageInitiated()
        {
            IBrowserClient client = new PlatinumBrowserClient();
            client.InitBrowser();
            string pageId = client.CreatePage();
            client.Open(pageId,"https://google.pl");
            client.ClosePage(pageId);
            client.CloseBrowser();
        }

        [Test]
        public void CloseBrowserNotInitiated()
        {
            IBrowserClient client = new PlatinumBrowserClient();
            client.CloseBrowser();
        }

        [Test]
        public void CloseBrowserInitiated()
        {
            IBrowserClient client = new PlatinumBrowserClient();
            client.InitBrowser();
            client.CloseBrowser();
        }

        [Test]
        public void CloseBrowserInitiatedAndPageOpened()
        {
            IBrowserClient client = new PlatinumBrowserClient();
            client.InitBrowser();
            string pageId = client.CreatePage();
            client.Open(pageId,"https://google.pl");
            client.CloseBrowser();
        }

        [Test]
        public void RefreshPageBrowserNotInitiated()
        {
            IBrowserClient client = new PlatinumBrowserClient();
            client.RefreshPage("0");
            client.CloseBrowser();
        }

        [Test]
        public void RefreshPageBrowserInitiated()
        {
            IBrowserClient client = new PlatinumBrowserClient();
            client.InitBrowser();
            string pageId = client.CreatePage();
            client.RefreshPage(pageId);
            client.CloseBrowser();
        }

        [Test]
        public void RefreshPageInitiatedAndPageOpened()
        {
            IBrowserClient client = new PlatinumBrowserClient();
            client.InitBrowser();
            string pageId = client.CreatePage();
            client.Open(pageId,"https://google.pl");
            client.RefreshPage(pageId);
            client.CloseBrowser();
        }
    }
}