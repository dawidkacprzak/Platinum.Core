using System.Threading;
using NUnit.Framework;
using Platinum.Core.ApiIntegration;
using Platinum.Core.Types;
using Platinum.Core.Types.Exceptions;

namespace Platinum.Tests.Integration
{
    [TestFixture]
    public class SharpBrowserClientTest
    {
        [TearDown]
        public void TearDown()
        {
            Thread.Sleep(1000);
        }

        [Test]
        public void InitBrowserNoException()
        {
            IBrowserClient client = new SharpBrowserClient();

            Assert.DoesNotThrow(() => { client.InitBrowser(); });
            client.CloseBrowser();
        }


        [TestCase("https://allegro.pl")]
        [TestCase("https://google.pl")]
        [TestCase("")]
        public void OpenPageBrowserNotInitiated(string url)
        {
            IBrowserClient client = new SharpBrowserClient();
            client.CloseBrowser();
            RequestException ex = Assert.Throws<RequestException>(() => client.Open("0",url));
            Assert.That(ex, Is.Not.Null);
        }


        [Test]
        public void GetSiteSourceNotInitiated()
        {
            IBrowserClient client = new SharpBrowserClient();
            client.DeInit();
            client.CloseBrowser();
            RequestException ex = Assert.Throws<RequestException>(() => client.CurrentSiteSource("0"));
            Assert.That(ex,Is.Not.Null);
        }

        [Test]
        public void GetSiteSourceInitiated()
        {
            IBrowserClient client = new SharpBrowserClient();
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
            IBrowserClient client = new SharpBrowserClient();
            client.DeInit();
            Assert.DoesNotThrow(() => client.ResetBrowser());
        }

        [Test]
        public void ResetBrowserInitiated()
        {
            IBrowserClient client = new SharpBrowserClient();
            client.InitBrowser();
            client.ResetBrowser();
            client.CloseBrowser();
        }

        [Test]
        public void ClosePageNotInitiated()
        {
            IBrowserClient client = new SharpBrowserClient();
            client.ClosePage("0");
            client.CloseBrowser();
        }

        [Test]
        public void ClosePageInitiated()
        {
            IBrowserClient client = new SharpBrowserClient();
            client.InitBrowser();
            string pageId = client.CreatePage();
            client.Open(pageId,"https://google.pl");
            client.ClosePage(pageId);
            client.CloseBrowser();
        }

        [Test]
        public void CloseBrowserNotInitiated()
        {
            IBrowserClient client = new SharpBrowserClient();
            client.CloseBrowser();
        }

        [Test]
        public void CloseBrowserInitiated()
        {
            IBrowserClient client = new SharpBrowserClient();
            client.InitBrowser();
            client.CloseBrowser();
        }

        [Test]
        public void CloseBrowserInitiatedAndPageOpened()
        {
            IBrowserClient client = new SharpBrowserClient();
            client.InitBrowser();
            string pageId = client.CreatePage();
            client.Open(pageId,"https://google.pl");
            client.CloseBrowser();
        }

        [Test]
        public void RefreshPageBrowserNotInitiated()
        {
            IBrowserClient client = new SharpBrowserClient();
            client.RefreshPage("0");
            client.CloseBrowser();
        }

        [Test]
        public void RefreshPageBrowserInitiated()
        {
            IBrowserClient client = new SharpBrowserClient();
            client.InitBrowser();
            string pageId = client.CreatePage();
            client.RefreshPage(pageId);
            client.CloseBrowser();
        }

        [Test]
        public void RefreshPageInitiatedAndPageOpened()
        {
            IBrowserClient client = new SharpBrowserClient();
            client.InitBrowser();
            string pageId = client.CreatePage();
            client.Open(pageId,"https://google.pl");
            client.RefreshPage(pageId);
            client.CloseBrowser();
        }
    }
}