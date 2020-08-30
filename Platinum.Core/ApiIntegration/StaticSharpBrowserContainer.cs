using System.Collections.Generic;
using PuppeteerSharp;

namespace Platinum.Core.ApiIntegration
{
    public static class StaticSharpBrowserContainer
    {
        public static Browser browser;
        public static Dictionary<string, Page> pages;

        public static void Init()
        {
            new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultRevision);
            if (browser == null)
            {
                var launchOptions = new LaunchOptions
                {
                    Headless = true
                };
                StaticSharpBrowserContainer.browser = Puppeteer.LaunchAsync(launchOptions).GetAwaiter().GetResult();
            }

            if (pages == null)
                pages = new Dictionary<string, Page>();
        }
    }
}