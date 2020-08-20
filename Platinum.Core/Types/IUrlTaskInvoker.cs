using System.Collections.Generic;
using System.Threading.Tasks;
using Platinum.Core.Factory.BrowserRestClient;
using Platinum.Core.Model;

namespace Platinum.Core.Types
{
    public interface IUrlTaskInvoker
    {
        Task Run();

        /// <summary>
        /// KeyValuePair<<CategoryId,TaskId>, List of filter parameters>
        /// </summary>
        KeyValuePair<KeyValuePair<int,int>, IEnumerable<WebsiteCategoriesFilterSearch>> GetOldestTask(IDal db);
        Task InvokeTask();
        void PopTaskFromQueue(IDal db,int taskId);
        Task[] GetUrlFetchingTasks(int maxTaskCount);
    }
}