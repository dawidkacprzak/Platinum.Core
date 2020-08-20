using System;
using System.Collections.Generic;
using Platinum.Core.Types;
using Platinum.Core.Types.Exceptions;
using PuppeteerSharp;

namespace Platinum.Core.ApiIntegration
{
    public class SharpBrowserClient : IBrowserClient
    {
        public void InitBrowser()
        {
            if (StaticSharpBrowserContainer.browser == null)
            {
                StaticSharpBrowserContainer.Init();
            }

            if (StaticSharpBrowserContainer.pages == null)
                StaticSharpBrowserContainer.pages = new Dictionary<string, Page>();
        }

        public string CreatePage()
        {
            InitBrowserIfNotInitied();

            Page page = StaticSharpBrowserContainer.browser.NewPageAsync().GetAwaiter().GetResult();
            string uniquePageId = Guid.NewGuid().ToString();
            StaticSharpBrowserContainer.pages.Add(uniquePageId, page);
            return uniquePageId;
        }

        public void Open(string pageId, string url)
        {
            if (StaticSharpBrowserContainer.browser == null)
            {
                throw new RequestException("Browser is not initied");
            }

            if (StaticSharpBrowserContainer.pages == null || !StaticSharpBrowserContainer.pages.ContainsKey(pageId))
                return;

            StaticSharpBrowserContainer.pages[pageId].GoToAsync(url).GetAwaiter().GetResult();
        }

        public string CurrentSiteSource(string pageId)
        {
            if (StaticSharpBrowserContainer.browser == null)
            {
                throw new RequestException("Browser is not initied");
            }

            if (StaticSharpBrowserContainer.pages == null || !StaticSharpBrowserContainer.pages.ContainsKey(pageId))
                return string.Empty;
            return StaticSharpBrowserContainer.pages[pageId].EvaluateExpressionAsync<string>("document.body.innerHTML").GetAwaiter()
                .GetResult();
        }

        public string CurrentSiteHeader(string pageId)
        {
            InitBrowserIfNotInitied();

            if (StaticSharpBrowserContainer.pages == null || !StaticSharpBrowserContainer.pages.ContainsKey(pageId))
                return string.Empty;
            return StaticSharpBrowserContainer.pages[pageId].EvaluateExpressionAsync<string>("document.head.innerHTML").GetAwaiter()
                .GetResult();
        }

        public void ResetBrowser()
        {
            InitBrowserIfNotInitied();

            foreach (KeyValuePair<string, Page> page in StaticSharpBrowserContainer.pages) page.Value.CloseAsync().GetAwaiter().GetResult();

            StaticSharpBrowserContainer.browser?.CloseAsync().GetAwaiter().GetResult();
            StaticSharpBrowserContainer.browser = Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = false,
                Args = new[]
                {
                    "--no-sandbox"
                }
            }).GetAwaiter().GetResult();
            StaticSharpBrowserContainer.pages = new Dictionary<string, Page>();
        }

        public void RefreshPage(string pageId)
        {
            InitBrowserIfNotInitied();

            if (StaticSharpBrowserContainer.pages == null || !StaticSharpBrowserContainer.pages.ContainsKey(pageId))
                return;
            StaticSharpBrowserContainer.pages[pageId].ReloadAsync().GetAwaiter().GetResult();
        }

        public void ClosePage(string pageId)
        {
            if (string.IsNullOrEmpty(pageId)) return;
            InitBrowserIfNotInitied();
            if (StaticSharpBrowserContainer.pages == null || !StaticSharpBrowserContainer.pages.ContainsKey(pageId))
            {
            }
            else
            {
                StaticSharpBrowserContainer.pages[pageId].CloseAsync().GetAwaiter().GetResult();
                StaticSharpBrowserContainer.pages.Remove(pageId);
            }
        }

        public void CloseBrowser()
        {
            if (StaticSharpBrowserContainer.pages != null)
                foreach (KeyValuePair<string, Page> page in StaticSharpBrowserContainer.pages)
                    page.Value.CloseAsync().GetAwaiter().GetResult();

            StaticSharpBrowserContainer.browser?.CloseAsync().GetAwaiter().GetResult();
            StaticSharpBrowserContainer.pages = new Dictionary<string, Page>();
        }

        public void DeInit()
        {
            CloseBrowser();
            if (StaticSharpBrowserContainer.browser != null)
            {
                StaticSharpBrowserContainer.browser.Dispose();
                StaticSharpBrowserContainer.browser = null;
            }
        }

        private void InitBrowserIfNotInitied()
        {
            if (StaticSharpBrowserContainer.browser == null) InitBrowser();
        }
    }
}