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
            IPlatinumBrowserRestClient client = new PlatinumBrowserRestClient();

            Assert.DoesNotThrow(() => { client.InitBrowser(); });
            client.CloseBrowser();
        }


        [TestCase("https://allegro.pl")]
        [TestCase("https://google.pl")]
        [TestCase("")]
        public void OpenPageBrowserNotInitiated(string url)
        {
            IPlatinumBrowserRestClient client = new PlatinumBrowserRestClient();
            client.CloseBrowser();
            RequestException ex = Assert.Throws<RequestException>(() => client.Open("0",url));
            Assert.That(ex, Is.Not.Null);
        }


        [Test]
        public void GetSiteSourceNotInitiated()
        {
            IPlatinumBrowserRestClient client = new PlatinumBrowserRestClient();
            client.DeInit();
            client.CloseBrowser();
            RequestException ex = Assert.Throws<RequestException>(() => client.CurrentSiteSource("0"));
            Assert.That(ex,Is.Not.Null);
        }

        [Test]
        public void GetSiteSourceInitiated()
        {
            IPlatinumBrowserRestClient client = new PlatinumBrowserRestClient();
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
            IPlatinumBrowserRestClient client = new PlatinumBrowserRestClient();
            client.DeInit();
            RequestException ex = Assert.Throws<RequestException>(() => client.ResetBrowser());
            Assert.That(ex,Is.Not.Null);
        }

        [Test]
        public void ResetBrowserInitiated()
        {
            IPlatinumBrowserRestClient client = new PlatinumBrowserRestClient();
            client.InitBrowser();
            client.ResetBrowser();
            client.CloseBrowser();
        }

        [Test]
        public void ClosePageNotInitiated()
        {
            IPlatinumBrowserRestClient client = new PlatinumBrowserRestClient();
            client.ClosePage("0");
            client.CloseBrowser();
        }

        [Test]
        public void ClosePageInitiated()
        {
            IPlatinumBrowserRestClient client = new PlatinumBrowserRestClient();
            client.InitBrowser();
            string pageId = client.CreatePage();
            client.Open(pageId,"https://google.pl");
            client.ClosePage(pageId);
            client.CloseBrowser();
        }

        [Test]
        public void CloseBrowserNotInitiated()
        {
            IPlatinumBrowserRestClient client = new PlatinumBrowserRestClient();
            client.CloseBrowser();
        }

        [Test]
        public void CloseBrowserInitiated()
        {
            IPlatinumBrowserRestClient client = new PlatinumBrowserRestClient();
            client.InitBrowser();
            client.CloseBrowser();
        }

        [Test]
        public void CloseBrowserInitiatedAndPageOpened()
        {
            IPlatinumBrowserRestClient client = new PlatinumBrowserRestClient();
            client.InitBrowser();
            string pageId = client.CreatePage();
            client.Open(pageId,"https://google.pl");
            client.CloseBrowser();
        }

        [Test]
        public void RefreshPageBrowserNotInitiated()
        {
            IPlatinumBrowserRestClient client = new PlatinumBrowserRestClient();
            client.RefreshPage("0");
            client.CloseBrowser();
        }

        [Test]
        public void RefreshPageBrowserInitiated()
        {
            IPlatinumBrowserRestClient client = new PlatinumBrowserRestClient();
            client.InitBrowser();
            string pageId = client.CreatePage();
            client.RefreshPage(pageId);
            client.CloseBrowser();
        }

        [Test]
        public void RefreshPageInitiatedAndPageOpened()
        {
            IPlatinumBrowserRestClient client = new PlatinumBrowserRestClient();
            client.InitBrowser();
            string pageId = client.CreatePage();
            client.Open(pageId,"https://google.pl");
            client.RefreshPage(pageId);
            client.CloseBrowser();
        }
    }
}