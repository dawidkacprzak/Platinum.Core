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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.Info("Started service");
                List<int> categoryIds = new List<int>();

                using (Dal db = new Dal())
                {
                    using (DbDataReader reader =
                        db.ExecuteReader(
                            $"SELECT Id from websiteCategories where websiteId = {(int) OfferWebsite.Allegro}"))
                    {
                        while (reader.Read())
                        {
                            categoryIds.Add(reader.GetInt32(reader.GetOrdinal("Id")));
                        }
                    }
                }

                _logger.Info($"Found {categoryIds.Count} category count");
                categoryIds = categoryIds.Where(x => x == 3).ToList();
                foreach (int categoryId in categoryIds)
                {
                    List<WebsiteCategoriesFilterSearch> arguments = new List<WebsiteCategoriesFilterSearch>();

                    using (Dal db = new Dal())
                    {
                        using (DbDataReader rd =
                            db.ExecuteReader(
                                $"SELECT * from websiteCategoriesFilterSearch where WebsiteCategoryId = {categoryId}"))
                        {
                            while (await rd.ReadAsync(stoppingToken))
                            {
                                arguments.Add(new WebsiteCategoriesFilterSearch()
                                {
                                    Id = rd.GetInt32(rd.GetOrdinal("Id")),
                                    WebsiteCategoryId = rd.GetInt32(rd.GetOrdinal("WebsiteCategoryId")),
                                    Argument = rd.GetString(rd.GetOrdinal("Argument")),
                                    Value = rd.GetString(rd.GetOrdinal("Value")),
                                    SearchNumber = rd.GetInt32(rd.GetOrdinal("SearchNumber")),
                                });
                            }
                        }
                    }


                    if (arguments.Count == 0)
                    {
                        _logger.Info($"Started fetching category #{categoryId} with 0 arguments");
                        using (IBaseOfferListController ctr = new AllegroOfferListController())
                        {
                            ctr.StartFetching(false, new OfferCategory(OfferWebsite.Allegro, categoryId));
                        }

                        _logger.Info($"Finished fetching category #{categoryId} with 0 arguments");
                    }
                    else
                    {
                        Dictionary<int, List<WebsiteCategoriesFilterSearch>> groupedArguments =
                            arguments.GroupBy(x => x.SearchNumber).ToDictionary(
                                websiteCategoriesFilterSearches => websiteCategoriesFilterSearches.Key,
                                websiteCategoriesFilterSearches => websiteCategoriesFilterSearches.ToList());

                        Task[] t = new Task[groupedArguments.Count + 4];
                        for (int i = 0; i < groupedArguments.Count; i++)
                        {
                            int i2 = i;
                            t[i] = new Task(() =>
                            {
                                int i1 = i2;
                                Thread.Sleep(500);
                                using (IBaseOfferListController ctr = new AllegroOfferListController())
                                {
                                    _logger.Info(
                                        $"Started fetching category #{categoryId} with {groupedArguments.ElementAt(i1).Value.Count} arguments and search number: {groupedArguments.ElementAt(i1).Value.First().SearchNumber}");

                                    System.Diagnostics.Debug.WriteLine(
                                        "fetching: " + groupedArguments.ElementAt(i1).Value.ElementAt(3).Value);
                                    ctr.StartFetching(false, new OfferCategory(OfferWebsite.Allegro, categoryId),
                                        groupedArguments.ElementAt(i1).Value);

                                    _logger.Info(
                                        $"Finished fetching category #{categoryId} with {groupedArguments.ElementAt(i1).Value.Count} arguments and search number: {groupedArguments.ElementAt(i1).Value.First().SearchNumber}");
                                }
                            });
                        }

                        Task[] tasks = new Task[4];
                        int p = 0;
                        for (int i = 0; i < t.Length; i++)
                        {
                            if (t[i] == null)
                            {
                                t[i] = new Task(() => { });
                            }

                            System.Diagnostics.Debug.WriteLine("p: " + p + " = " + i);
                            tasks[p] = t[i];
                            p++;
                            if (p == 4)
                            {
                                for (int b = 0; b < 4; b++)
                                {
                                    tasks[b].Start();
                                    Thread.Sleep(500);
                                }

                                Task.WaitAll(tasks);
                                p = 0;
                            }
                        }
                    }
                }

                await Task.Delay(60000 * 15, stoppingToken);
            }
        }
    }
}