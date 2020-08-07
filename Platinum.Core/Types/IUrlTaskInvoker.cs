using System.Collections.Generic;
using System.Threading.Tasks;
using Platinum.Core.Factory.BrowserRestClient;
using Platinum.Core.Model;

namespace Platinum.Core.Types
{
    public interface IUrlTaskInvoker
    {
        Task Run(IBrowserRestClientFactory browserRestClientFactory, IDal dal);
        IEnumerable<string> GetBrowsers(IDal dal);
        void ResetBrowser(IBrowserRestClient client, string hosts);
        
        /// <summary>
        /// KeyValuePair<<CategoryId,TaskId>, List of filter parameters>
        /// </summary>
        KeyValuePair<KeyValuePair<int,int>, IEnumerable<WebsiteCategoriesFilterSearch>> GetOldestTask();
        Task InvokeTask(string host);
        void PopTaskFromQueue(IDal db,int taskId);
        Task[] GetUrlFetchingTasks(IEnumerable<string> activeBrowsers);
    }
}