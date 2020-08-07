using System.Threading;
using NUnit.Framework;
using Platinum.Core.ApiIntegration;
using Platinum.Core.Types;
using Platinum.Core.Types.Exceptions;

namespace Platinum.Tests.Integration
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
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
            IBrowserRestClient client = new PlatinumBrowserRestClient();

            Assert.DoesNotThrow(() => { client.InitBrowser(); });
            client.CloseBrowser();
        }


        [TestCase("https://allegro.pl")]
        [TestCase("https://google.pl")]
        [TestCase("")]
        public void OpenPageBrowserNotInitiated(string url)
        {
            IBrowserRestClient client = new PlatinumBrowserRestClient();
            client.CloseBrowser();
            RequestException ex = Assert.Throws<RequestException>(() => client.Open("0",url));
            Assert.That(ex, Is.Not.Null);
        }


        [Test]
        public void GetSiteSourceNotInitiated()
        {
            IBrowserRestClient client = new PlatinumBrowserRestClient();
            client.DeInit();
            client.CloseBrowser();
            RequestException ex = Assert.Throws<RequestException>(() => client.CurrentSiteSource("0"));
            Assert.That(ex,Is.Not.Null);
        }

        [Test]
        public void GetSiteSourceInitiated()
        {
            IBrowserRestClient client = new PlatinumBrowserRestClient();
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
            IBrowserRestClient client = new PlatinumBrowserRestClient();
            client.DeInit();
            Assert.DoesNotThrow(() => client.ResetBrowser());
        }

        [Test]
        public void ResetBrowserInitiated()
        {
            IBrowserRestClient client = new PlatinumBrowserRestClient();
            client.InitBrowser();
            client.ResetBrowser();
            client.CloseBrowser();
        }

        [Test]
        public void ClosePageNotInitiated()
        {
            IBrowserRestClient client = new PlatinumBrowserRestClient();
            client.ClosePage("0");
            client.CloseBrowser();
        }

        [Test]
        public void ClosePageInitiated()
        {
            IBrowserRestClient client = new PlatinumBrowserRestClient();
            client.InitBrowser();
            string pageId = client.CreatePage();
            client.Open(pageId,"https://google.pl");
            client.ClosePage(pageId);
            client.CloseBrowser();
        }

        [Test]
        public void CloseBrowserNotInitiated()
        {
            IBrowserRestClient client = new PlatinumBrowserRestClient();
            client.CloseBrowser();
        }

        [Test]
        public void CloseBrowserInitiated()
        {
            IBrowserRestClient client = new PlatinumBrowserRestClient();
            client.InitBrowser();
            client.CloseBrowser();
        }

        [Test]
        public void CloseBrowserInitiatedAndPageOpened()
        {
            IBrowserRestClient client = new PlatinumBrowserRestClient();
            client.InitBrowser();
            string pageId = client.CreatePage();
            client.Open(pageId,"https://google.pl");
            client.CloseBrowser();
        }

        [Test]
        public void RefreshPageBrowserNotInitiated()
        {
            IBrowserRestClient client = new PlatinumBrowserRestClient();
            client.RefreshPage("0");
            client.CloseBrowser();
        }

        [Test]
        public void RefreshPageBrowserInitiated()
        {
            IBrowserRestClient client = new PlatinumBrowserRestClient();
            client.InitBrowser();
            string pageId = client.CreatePage();
            client.RefreshPage(pageId);
            client.CloseBrowser();
        }

        [Test]
        public void RefreshPageInitiatedAndPageOpened()
        {
            IBrowserRestClient client = new PlatinumBrowserRestClient();
            client.InitBrowser();
            string pageId = client.CreatePage();
            client.Open(pageId,"https://google.pl");
            client.RefreshPage(pageId);
            client.CloseBrowser();
        }
    }
}