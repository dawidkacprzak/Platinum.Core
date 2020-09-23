using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Nest;
using NLog;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Model;
using Platinum.Core.OfferListController;
using Platinum.Core.Types;

namespace Platinum.Service.UrlFetchTaskCreator
{
    public class AllegroFetchUrls : BackgroundService
    {
        Logger _logger = LogManager.GetCurrentClassLogger();


        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.Info("Started service");

                List<KeyValuePair<int, int>> categoryIds = GetAllCategories().ToList().OrderByDescending(x => x.Key).ToList();

                List<int> catsToRemove = new List<int>();
                foreach (var c in categoryIds)
                {
                    using (Dal db = new Dal())
                    {
                        int paramsCount = (int) db.ExecuteScalar(
                            $"SELECT isnull(MAX(searchNumber),0) FROM websiteCategoriesFilterSearch where websiteCategoriesFilterSearch.WebsiteCategoryId in (SELECT allegroUrlFetchTask.CategoryId from allegroUrlFetchTask where CategoryId={c.Key} and WebApiUserId={c.Value})");
                        if (paramsCount == 0)
                        {
                            if(!VerifyTaskCanBeStarted(c.Key,c.Value,5))
                            {
                                catsToRemove.Add(c.Key);
                            }
                        }
                        else
                        {
                            if(!VerifyTaskCanBeStarted(c.Key,c.Value,5))
                            {
                                catsToRemove.Add(c.Key);
                            }
                        }
                    } 
                }

                categoryIds = categoryIds.Where(x => !catsToRemove.Contains(x.Key)).ToList();
                _logger.Info($"Found {categoryIds.Count} category count");

                foreach (KeyValuePair<int, int> categoryId in categoryIds)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        _logger.Info("Task canceled by token");
                        return;
                    }

                    try
                    {
                        List<WebsiteCategoriesFilterSearch> arguments = GetCategoryFilters(categoryId.Key).ToList();

                        if (arguments.Count == 0)
                        {
                            AddNonFilterTaskOnQueue(categoryId.Key, categoryId.Value);
                        }
                        else
                        {
                            Dictionary<int, List<WebsiteCategoriesFilterSearch>> groupedArguments =
                                arguments.GroupBy(x => x.SearchNumber).ToDictionary(
                                    websiteCategoriesFilterSearches => websiteCategoriesFilterSearches.Key,
                                    websiteCategoriesFilterSearches => websiteCategoriesFilterSearches.ToList());

                            AddFilteredTaskOnQueue(categoryId, groupedArguments);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }
                }


                _logger.Info("Task finished");
                //   await Task.Delay(60000 * 15, stoppingToken);
            }
        }


        public IEnumerable<WebsiteCategoriesFilterSearch> GetCategoryFilters(int categoryId)
        {
            using (Dal db = new Dal())
            {
                using (DbDataReader rd =
                    db.ExecuteReader(
                        $"SELECT * from websiteCategoriesFilterSearch WITH (NOLOCK) where WebsiteCategoryId = {categoryId}")
                )
                {
                    while (rd.Read())
                    {
                        yield return new WebsiteCategoriesFilterSearch()
                        {
                            Id = rd.GetInt32(rd.GetOrdinal("Id")),
                            WebsiteCategoryId = rd.GetInt32(rd.GetOrdinal("WebsiteCategoryId")),
                            Argument = rd.GetString(rd.GetOrdinal("Argument")),
                            Value = rd.GetString(rd.GetOrdinal("Value")),
                            SearchNumber = rd.GetInt32(rd.GetOrdinal("SearchNumber")),
                        };
                    }
                }
            }
        }

        public List<KeyValuePair<int, int>> GetAllCategories()
        {
            List<KeyValuePair<int, int>> retList = new List<KeyValuePair<int, int>>();
            using (Dal db = new Dal())
            {
                using (DbDataReader reader =
                    db.ExecuteReader(
                        $@"
                                select websiteCategories.Id,WebApiUserId from websiteCategories with (nolock)
                                INNER JOIN WebApiUserWebsiteCategory on WebApiUserWebsiteCategory.WebsiteCategoryId = websiteCategories.Id
                                WHERE websiteCategories.WebsiteId = {(int) EOfferWebsite.Allegro} group by websiteCategories.Id, WebApiUserId")
                )
                {
                    while (reader.Read())
                    {
                        retList.Add(new KeyValuePair<int, int>(reader.GetInt32(reader.GetOrdinal("Id")),
                            reader.GetInt32(reader.GetOrdinal("WebApiUserId"))));
                    }
                }
            }

            return retList;
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
        public void AddNonFilterTaskOnQueue(int categoryId, int userId)
        {
            using (Dal db = new Dal())
            {
                db.ExecuteNonQuery(
                    $"INSERT INTO allegroUrlFetchTask (CategoryId,WebApiUserId) VALUES({categoryId},{userId})");
            }
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
        public void AddFilteredTaskOnQueue(KeyValuePair<int, int> categoryUser,
            Dictionary<int, List<WebsiteCategoriesFilterSearch>> arguments)
        {
            using (Dal db = new Dal())
            {
                try
                {
                    foreach (List<WebsiteCategoriesFilterSearch> arg in arguments.Values)
                    {
                        db.BeginTransaction();
                        int insertedId = (int) db.ExecuteScalar(
                            $"INSERT INTO allegroUrlFetchTask (CategoryId,WebApiUserId) OUTPUT inserted.Id VALUES({categoryUser.Key},{categoryUser.Value})");

                        foreach (var singleArg in arg)
                        {
                            db.ExecuteNonQuery(
                                $@"INSERT INTO allegroUrlFetchTaskParameter
                                VALUES('{singleArg.Argument}','{singleArg.Value}',{insertedId});");
                        }

                        db.CommitTransaction();
                    }
                }
                catch (Exception)
                {
                    db.RollbackTransaction();
                    throw;
                }
            }
        }


        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
        public bool VerifyTaskCanBeStarted(int categoryId, int webApiUserId,int maxOffers)
        {
            int taskCount = GetTaskCount(categoryId,webApiUserId);
            if (taskCount > maxOffers)
            {
                _logger.Info("Service loop skipped - task count > 10000");
                return false;
            }

            _logger.Info("Current task count: " + taskCount);
            return true;
        }

        public int GetTaskCount(int categoryId, int webApiUserId)
        {
            using (Dal db = new Dal())
            {
                return (int) db.ExecuteScalar($"SELECT COUNT(*) FROM allegroUrlFetchTask with(nolock) where CategoryId={categoryId} and WebApiUserId={webApiUserId} and Processed != 0;");
            }
        }
    }
}