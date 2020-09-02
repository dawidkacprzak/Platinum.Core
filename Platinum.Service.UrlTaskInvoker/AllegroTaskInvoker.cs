using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Platinum.Core.ApiIntegration;
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
        public List<string> ActiveTasksId;

        private static readonly object getTaskLock = new object();
        readonly private Logger logger = LogManager.GetCurrentClassLogger();
        public static int MAX_TASKS_PER_RUN = 55;
        public static int MAX_CONCURRENT_TASKS = 5;
        public static int CURRENT_TASK_COUNT = 0;

        public AllegroTaskInvoker()
        {
            try
            {
                MAX_CONCURRENT_TASKS = int.Parse(Program.NumberOfTasksArg);
            }
            catch (Exception)
            {
                MAX_CONCURRENT_TASKS = 3;
            }

            logger.Info("Number of max tasks: " + MAX_CONCURRENT_TASKS);
            lock (getTaskLock)
            {
                StaticSharpBrowserContainer.Init();
                ActiveTasksId = new List<string>();
            }
        }

        [ExcludeFromCodeCoverage]
        public async Task Run()
        {
            try
            {
                CURRENT_TASK_COUNT = 0;
                logger.Info("Service iteration started");

                using (SemaphoreSlim concurrencySemaphore = new SemaphoreSlim(MAX_CONCURRENT_TASKS))
                {
                    List<Task> tasks = new List<Task>();
                    for (int i = 0; i <= MAX_TASKS_PER_RUN; i++)
                    {
                        concurrencySemaphore.Wait();
                        Thread.Sleep(2500);
                        var t = Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                InvokeTask();
                            }
                            finally
                            {
                                concurrencySemaphore.Release();
                            }
                        });

                        tasks.Add(t);
                    }

                    Task.WaitAll(tasks.ToArray());
                }

                logger.Info("Waiting for task end");
                ;


                logger.Info("Next iteration...");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        [ExcludeFromCodeCoverage]
        public void InvokeTaskTest(string host)
        {
            using (IBaseOfferListController ctrl = new HttpAllegroOfferListController())
            {
                while (CURRENT_TASK_COUNT <= MAX_TASKS_PER_RUN)
                {
                    KeyValuePair<KeyValuePair<int, int>, IEnumerable<WebsiteCategoriesFilterSearch>> task;

                    using (IDal db = new Dal())
                    {
                        task = GetOldestTask(db);
                    }

                    logger.Info("Fetched oldest task " + task.Key.Value);

                    try
                    {
                        logger.Info("Started task #" + task.Key.Value);
                        ctrl.StartFetching(false, new OfferCategory(EOfferWebsite.Allegro, task.Key.Key),
                            task.Value.ToList());

                        logger.Info("Finished task #" + task.Key.Value);
                    }
                    catch (Exception ex)
                    {
                        logger.Info(ex);
                        logger.Info("Timeout task #" + task.Key.Value + " - BREAK");
                    }
                    finally
                    {
                        CURRENT_TASK_COUNT++;
                        using (IDal db = new Dal())
                        {
                            PopTaskFromQueue(db, task.Key.Value);
                        }
                    }
                }
            }
        }

        [ExcludeFromCodeCoverage]
        public void InvokeTask()
        {
            logger.Info("Invoke task - loop started");

            KeyValuePair<KeyValuePair<int, int>, IEnumerable<WebsiteCategoriesFilterSearch>> task;

            using (IDal db = new Dal())
            {
                logger.Info("Invoke task - attempt to get task");

                task = GetOldestTask(db);
            }

            logger.Info("Fetched oldest task " + task.Key.Value);

            try
            {
                using (HttpAllegroOfferListController ctrl = new HttpAllegroOfferListController())
                {
                    logger.Info("Started task #" + task.Key.Value);
                    ctrl.StartFetching(false, new OfferCategory(EOfferWebsite.Allegro, task.Key.Key),
                        task.Value.ToList());
                    using (IDal db = new Dal())
                    {
                        PopTaskFromQueue(db, task.Key.Value);
                    }
                }

                logger.Info("Finished task #" + task.Key.Value);
            }
            catch (Exception ex)
            {
                logger.Info(ex);
                logger.Info("Timeout task #" + task.Key.Value + " - BREAK");
            }
            finally
            {
                CURRENT_TASK_COUNT++;
            }
        }
        
        public void PopTaskFromQueue(IDal db, int taskId)
        {
            lock (getTaskLock)
            {
                try
                {
                    db.ExecuteNonQuery($"DELETE FROM allegroUrlFetchTask WHERE Id = {taskId}");
                    db.ExecuteNonQuery(
                        $"DELETE FROM allegroUrlFetchTaskParameter where AllegroUrlFetchTaskId = {taskId}");
                    ActiveTasksId.Remove(taskId.ToString());
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

        public KeyValuePair<KeyValuePair<int, int>, IEnumerable<WebsiteCategoriesFilterSearch>> GetOldestTask(IDal db)
        {
            logger.Info("Started task get - bef lock");
            lock (getTaskLock)
            {
                logger.Info("Started task get");

                int taskId;
                int categoryId;
                List<WebsiteCategoriesFilterSearch> taskFilters = new List<WebsiteCategoriesFilterSearch>();

                string taskQuery = $@"
                UPDATE allegroUrlFetchTask set Processed = {(int) EUrlFetchTaskProcessed.InProcess}
                OUTPUT inserted.Id
                WHERE allegroUrlFetchTask.Id IN (
                SELECT TOP 1 allegroUrlFetchTask.Id FROM allegroUrlFetchTask WITH (NOLOCK) 
                                            INNER JOIN websiteCategories on websiteCategories.ID = allegroUrlFetchTask.CategoryId
                                            WHERE websiteCategories.websiteId = {(int) EOfferWebsite.Allegro}
                                            AND allegroUrlFetchTask.Processed = {(int) EUrlFetchTaskProcessed.NotProcessed}";
                if (ActiveTasksId.Count > 0)
                {
                    taskQuery += $@" AND allegroUrlFetchTask.Id NOT IN ({string.Join(",", ActiveTasksId)}) ";
                }

                int updatedTaskId = -1;
                taskQuery += " ORDER BY Id)";
                using (DbDataReader taskReader =
                    db.ExecuteReader(taskQuery)
                )
                {
                    if (!taskReader.HasRows)
                    {
                        logger.Info("no rows");

                        throw new TaskInvokerException("Cannot get olders task. Not found any.");
                    }
                    else
                    {
                        logger.Info("rows and read");

                        taskReader.Read();
                        updatedTaskId = taskReader.GetInt32(0);
                    }
                }

                using (DbDataReader reader =
                    db.ExecuteReader($"SELECT * FROM allegroUrlFetchTask WITH (NOLOCK) WHERE ID = {updatedTaskId}"))
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        taskId = reader.GetInt32(reader.GetOrdinal("Id"));
                        categoryId = reader.GetInt32(reader.GetOrdinal("CategoryId"));
                    }
                    else
                    {
                        throw new TaskInvokerException("Cannot get olders task. Not found any.");
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

                ActiveTasksId.Add(taskId.ToString());

                KeyValuePair<int, int> categoryIdWithTaskId = new KeyValuePair<int, int>(categoryId, taskId);
                return new KeyValuePair<KeyValuePair<int, int>, IEnumerable<WebsiteCategoriesFilterSearch>>(
                    categoryIdWithTaskId, taskFilters);
            }
        }
    }
}