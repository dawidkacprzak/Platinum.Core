using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Platinum.Core.ApiIntegration;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Model;
using Platinum.Core.OfferListController;
using Platinum.Core.Types;
using Platinum.Core.Types.Exceptions;

namespace Platinum.Service.UrlTaskIvoker
{
    public class AllegroTaskInvoker : IUrlTaskInvoker
    {
        Logger _logger = LogManager.GetCurrentClassLogger();
        private List<string> activeBrowsers;
        private List<string> activeTasksId;

        private const int TASKS_PER_RUN = 10;
        private int FinishedTasks = 0;

        private static object getTaskLock = new object();

        public async Task Run()
        {
            try
            {
                activeTasksId = new List<string>();
                List<string> browsers = GetBrowsers().ToList();
                activeBrowsers = new List<string>();

                foreach (string browser in browsers)
                {
                    try
                    {
                        ResetBrowser(browser);
                        activeBrowsers.Add(browser);
                        _logger.Info("Active browser: " + browser);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }
                }

                Task[] tasks = new Task[activeBrowsers.Count];

                for (int i = 0; i < tasks.Length; i++)
                {
                    tasks[i] = InvokeTask(activeBrowsers.ElementAt(i));
                    tasks[i].Start();
                }

                _logger.Info("Created " + tasks.Length + " tasks");
                Task.WaitAll(tasks);

                await Task.Delay(50000);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }


        public Task InvokeTask(string host)
        {
            return new Task(() =>
            {
                while (FinishedTasks < TASKS_PER_RUN)
                {
                    KeyValuePair<KeyValuePair<int, int>, IEnumerable<WebsiteCategoriesFilterSearch>> task;

                    task = GetOldestTask();


                    using (IBaseOfferListController ctrl = new AllegroOfferListController(host))
                    {
                        try
                        {
                            _logger.Info("Started task #" + task.Key.Value);
                            ctrl.StartFetching(false, new OfferCategory(OfferWebsite.Allegro, task.Key.Key),
                                task.Value.ToList());

                            PopTaskFromQueue(task.Key.Value);
                            _logger.Info("Finished task #" + task.Key.Value);
                            FinishedTasks++;
                        }
                        catch (Exception ex)
                        {
                            _logger.Info(ex);
                        }
                    }
                }
            });
        }

        public void PopTaskFromQueue(int taskId)
        {
            lock (getTaskLock)
            {
                using (Dal db = new Dal())
                {
                    try
                    {
                        db.BeginTransaction();
                        db.ExecuteNonQuery($"DELETE FROM allegroUrlFetchTask WHERE Id = {taskId}");
                        db.ExecuteNonQuery(
                            $"DELETE FROM allegroUrlFetchTaskParameter where AllegroUrlFetchTaskId = {taskId}");
                        db.CommitTransaction();
                        activeTasksId.Remove(taskId.ToString());
                        _logger.Info("Removed task #" + taskId + " from queue");
                    }
                    catch (DalException ex)
                    {
                        db.RollbackTransaction();
                        _logger.Error(ex);
                    }
                }
            }
        }

        public IEnumerable<string> GetBrowsers()
        {
            using (Dal db = new Dal())
            {
                using (DbDataReader reader = db.ExecuteReader("SELECT Host from browsers WITH(NOLOCK);"))
                {
                    if (!reader.HasRows)
                    {
                        throw new TaskInvokerException("No browsers found");
                    }

                    while (reader.Read())
                    {
                        _logger.Info("Fetched browser: " + reader.GetString(0));
                        yield return reader.GetString(0);
                    }
                }
            }
        }

        public void ResetBrowser(string host)
        {
            try
            {
                PlatinumBrowserRestClient client = new PlatinumBrowserRestClient(host);
                client.ResetBrowser();
            }
            catch (RequestException ex)
            {
                throw new TaskInvokerException($"Cannot reset browser {host}", ex);
            }
        }

        public KeyValuePair<KeyValuePair<int, int>, IEnumerable<WebsiteCategoriesFilterSearch>> GetOldestTask()
        {
            lock (getTaskLock)
            {
                int taskId;
                int categoryId;
                List<WebsiteCategoriesFilterSearch> taskFilters = new List<WebsiteCategoriesFilterSearch>();
                using (Dal db = new Dal())
                {
                    string taskQuery = $@"SELECT TOP 1 allegroUrlFetchTask.* FROM allegroUrlFetchTask WITH (NOLOCK) 
                                                INNER JOIN websiteCategories on websiteCategories.ID = allegroUrlFetchTask.CategoryId
                                                WHERE websiteCategories.websiteId = {(int) OfferWebsite.Allegro} ";
                    if (activeTasksId.Count > 0)
                    {
                        taskQuery += $@" AND allegroUrlFetchTask.Id NOT IN ({string.Join(",", activeTasksId)}) ";
                    }

                    taskQuery += " ORDER BY Id";
                    using (DbDataReader taskReader =
                        db.ExecuteReader(taskQuery)
                    )
                    {
                        if (!taskReader.HasRows)
                        {
                            throw new TaskInvokerException("Cannot get olders task. Not found any.");
                        }
                        else
                        {
                            taskReader.Read();
                            taskId = taskReader.GetInt32(taskReader.GetOrdinal("Id"));
                            categoryId = taskReader.GetInt32(taskReader.GetOrdinal("CategoryId"));
                        }
                    }

                    using (DbDataReader filterReader = db.ExecuteReader(
                        $@"select * from allegroUrlFetchTaskParameter with(nolock)
                                                                             WHERE AllegroUrlFetchTaskId = {taskId}"))
                    {
                        if (filterReader.HasRows)
                        {
                            while (filterReader.Read())
                            {
                                WebsiteCategoriesFilterSearch filter = new WebsiteCategoriesFilterSearch()
                                {
                                    Id = filterReader.GetInt32(filterReader.GetOrdinal("Id")),
                                    SearchNumber = 0,
                                    Argument = filterReader.GetString(filterReader.GetOrdinal("Name")),
                                    Value = filterReader.GetString(filterReader.GetOrdinal("Values")),
                                    WebsiteCategoryId = categoryId,
                                    TaskId = taskId
                                };
                                taskFilters.Add(filter);
                            }
                        }
                    }

                    activeTasksId.Add(taskId.ToString());
                }

                KeyValuePair<int, int> categoryIdWithTaskId = new KeyValuePair<int, int>(categoryId, taskId);
                return new KeyValuePair<KeyValuePair<int, int>, IEnumerable<WebsiteCategoriesFilterSearch>>(
                    categoryIdWithTaskId, taskFilters);
            }
        }
    }
}