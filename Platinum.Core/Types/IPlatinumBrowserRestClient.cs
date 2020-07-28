using System;

namespace Platinum.Core.Types
{
    public interface IPlatinumBrowserRestClient
    {
        void InitBrowser();
        string CreatePage();
        void Open(string pageId, string url);
        string CurrentSiteSource(string pageId);
        void ResetBrowser();
        void RefreshPage(string pageId);
        void ClosePage(string pageId);
        void CloseBrowser();
        void DeInit();
    }
}