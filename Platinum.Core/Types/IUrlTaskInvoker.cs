using System.Collections.Generic;
using System.Threading.Tasks;
using Platinum.Core.Model;

namespace Platinum.Core.Types
{
    public interface IUrlTaskInvoker
    {
        Task Run();
        IEnumerable<string> GetBrowsers();
        void ResetBrowser(string hosts);
        
        /// <summary>
        /// KeyValuePair<<CategoryId,TaskId>, List of filter parameters>
        /// </summary>
        KeyValuePair<KeyValuePair<int,int>, IEnumerable<WebsiteCategoriesFilterSearch>> GetOldestTask();
        Task InvokeTask(string host);
        void PopTaskFromQueue(int taskId);
    }
}