﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Factory.BrowserRestClient;
using Platinum.Core.Model;
using Platinum.Core.OfferListController;
using Platinum.Core.Types;
using Platinum.Core.Types.Exceptions;

namespace Platinum.Service.UrlTaskInvoker
{
    public class AllegroTaskInvoker : IUrlTaskInvoker
    {
        readonly Logger logger = LogManager.GetCurrentClassLogger();
        private List<string> activeTasksId;

        private const int TASKS_PER_RUN = 30;
        private int finishedTasks = 0;

        private static readonly object getTaskLock = new object();
        List<string> activeBrowsers = new List<string>();


        public AllegroTaskInvoker()
        {
            lock (getTaskLock)
            {
                activeTasksId = new List<string>();
            }
        }

        public async Task Run(IBrowserRestClientFactory aBrowserRestClientFactory, IDal dal)
        {
            try
            {
                logger.Info("Service iteration started");

                List<string> allBrowsers = GetBrowsers(dal).ToList();
                foreach (string browser in allBrowsers)
                {
                    try
                    {
                        ResetBrowser(aBrowserRestClientFactory.GetBrowser(browser), browser);
                        logger.Info("Active browser: " + browser);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }
                }

                Task[] tasks = GetUrlFetchingTasks(activeBrowsers);
                for (int i = 0; i < tasks.Count(); i++)
                    tasks[i].Start();


                logger.Info("Created " + tasks.Length + " tasks");
                Task.WaitAll(tasks);
                logger.Info("Finished all tasks. Waiting 5s for next iteration");
                await Task.Delay(5000);
                logger.Info("Next iteration...");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public Task InvokeTask(string host)
        {
            return new Task(() =>
            {
                while (finishedTasks < TASKS_PER_RUN)
                {
                    KeyValuePair<KeyValuePair<int, int>, IEnumerable<WebsiteCategoriesFilterSearch>> task;

                    task = GetOldestTask();


                    using (IBaseOfferListController ctrl = new AllegroOfferListController(host))
                    {
                        try
                        {
                            logger.Info("Started task #" + task.Key.Value);
                            ctrl.StartFetching(false, new OfferCategory(EOfferWebsite.Allegro, task.Key.Key),
                                task.Value.ToList());
                            using (IDal db = new Dal())
                            {
                                PopTaskFromQueue(db, task.Key.Value);
                            }

                            logger.Info("Finished task #" + task.Key.Value);
                            finishedTasks++;
                        }
                        catch (Exception ex)
                        {
                            logger.Info(ex);
                            logger.Info("Timeout task #" + task.Key.Value + " - BREAK");
                            break;
                        }
                    }
                }

                logger.Info("Finished task for host: " + host);
            });
        }

        public Task[] GetUrlFetchingTasks(IEnumerable<string> activeBrowsers)
        {
            List<string> browsers = activeBrowsers.ToList();
            Task[] tasks = new Task[browsers.Count()];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = InvokeTask(browsers.ElementAt(i));
            }

            return tasks;
        }

        public void PopTaskFromQueue(IDal db, int taskId)
        {
            lock (getTaskLock)
            {
                try
                {
                    db.BeginTransaction();
                    db.ExecuteNonQuery($"DELETE FROM allegroUrlFetchTask WHERE Id = {taskId}");
                    db.ExecuteNonQuery(
                        $"DELETE FROM allegroUrlFetchTaskParameter where AllegroUrlFetchTaskId = {taskId}");
                    db.CommitTransaction();
                    activeTasksId.Remove(taskId.ToString());
                    logger.Info("Removed task #" + taskId + " from queue");
                }
                catch (DalException ex)
                {
                    db.RollbackTransaction();
                    logger.Error(ex);
                }
                catch (Exception ex)
                {
                    db.RollbackTransaction();
                    logger.Error(ex);
                }
            }
        }

        public IEnumerable<string> GetBrowsers(IDal db)
        {
            using (DbDataReader reader = db.ExecuteReader("SELECT Host from browsers WITH(NOLOCK);"))
            {
                if (!reader.HasRows)
                {
                    throw new TaskInvokerException("No browsers found");
                }

                while (reader.Read())
                {
                    logger.Info("Fetched browser: " + reader.GetString(0));
                    yield return reader.GetString(0);
                }
            }
        }

        public void ResetBrowser(IBrowserRestClient client, string host)
        {
            try
            {
                client.ResetBrowser();
                activeBrowsers.Add(host);
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
                                                WHERE websiteCategories.websiteId = {(int) EOfferWebsite.Allegro} ";
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