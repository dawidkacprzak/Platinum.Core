using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
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
                if (VerifyTaskCanBeStarted())
                {
                    List<int> categoryIds = GetAllCategories().ToList();

                    _logger.Info($"Found {categoryIds.Count} category count");

                    foreach (int categoryId in categoryIds)
                    {
                        if (stoppingToken.IsCancellationRequested)
                        {
                            _logger.Info("Task canceled by token");
                            return;
                        }

                        try
                        {
                            List<WebsiteCategoriesFilterSearch> arguments = GetCategoryFilters(categoryId).ToList();

                            if (arguments.Count == 0)
                            {
                                AddNonFilterTaskOnQueue(categoryId);
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
                }
                else
                {
                    _logger.Info("Task canceled");
                }

                _logger.Info("Task finished");
                await Task.Delay(60000 * 15, stoppingToken);
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

        public IEnumerable<int> GetAllCategories()
        {
            using (Dal db = new Dal())
            {
                using (DbDataReader reader =
                    db.ExecuteReader(
                        $"SELECT Id from websiteCategories WITH (NOLOCK) where websiteId = {(int) OfferWebsite.Allegro}"))
                {
                    while (reader.Read())
                    {
                        yield return reader.GetInt32(reader.GetOrdinal("Id"));
                    }
                }
            }
        }
        
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
        public void AddNonFilterTaskOnQueue(int categoryId)
        {
            using (Dal db = new Dal())
            {
                db.ExecuteNonQuery($"INSERT INTO allegroUrlFetchTask (CategoryId) VALUES({categoryId})");
            }
        }
        
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
        public void AddFilteredTaskOnQueue(int categoryId,
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
                            $"INSERT INTO allegroUrlFetchTask (CategoryId) OUTPUT inserted.Id VALUES({categoryId})");

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
        public bool VerifyTaskCanBeStarted()
        {
            int taskCount = GetTaskCount();
            if (taskCount > 50000)
            {
                _logger.Info("Service loop skipped - task count > 50000");
                return false;
            }

            _logger.Info("Current task count: " + taskCount);
            return true;
        }

        public int GetTaskCount()
        {
            using (Dal db = new Dal())
            {
                return (int) db.ExecuteScalar("SELECT COUNT(*) FROM allegroUrlFetchTask;");
            }
        }
    }
}