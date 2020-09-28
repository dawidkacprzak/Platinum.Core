using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core.ResourceActions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.ElasticIntegration;

namespace Platinum.Service.AzureContainersRestart
{
    public class Worker : BackgroundService
    {
        static Logger _logger = LogManager.GetCurrentClassLogger();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    string resourceGroupName = "container_Registry_fetcher";

                    string authFilePath = "my.azureauth";

                    IAzure azure = GetAzureContext(authFilePath);

                    IEnumerable<string> containers = ListContainerGroups(azure, resourceGroupName);
                    var enumerable = containers as string[] ?? containers.ToArray();
                    _logger.Info($"found: " + enumerable.Count() +
                                 $" containers to restart. ({string.Join(",", enumerable)})");
                    foreach (string container in enumerable)
                    {
                        int clientId = int.Parse(container.Split('-')[1]);
                        int categoryId = int.Parse(container.Split('-')[3]);
                        using (Dal db = new Dal())
                        {
                            try
                            {
                                using (DbDataReader reader = db.ExecuteReader(
                                    $@"select WebApiUserId,WebsiteCategoryId,MaxOffersInDb,MaxProceedOffersInMonth from WebApiUserWebsiteCategory
                            INNER JOIN PaidPlan on PaidPlan.Id = WebApiUserWebsiteCategory.PaidPlanId
                            WHERE WebsiteCategoryId ={categoryId} and WebApiUserId = {clientId}"))
                                {
                                    reader.Read();
                                    if (reader.HasRows)
                                    {
                                        int MaxOffersInDb = reader.GetInt32(2);
                                        int maxProceedOffersInMonth = reader.GetInt32(3);
                                        int currentYear = DateTime.Now.Year;
                                        int currentMonth = DateTime.Now.Month;
                                        DateTime startMonth = new DateTime(currentYear, currentMonth, 1);
                                        DateTime endMonth = new DateTime(currentYear, currentMonth, 1);
                                        endMonth = endMonth.AddMonths(1);

                                        long indexedCountAllTime =
                                            ElasticController.Instance.GetIndexDocumentCountByDateRange(16520, 2,
                                                startMonth, new DateTime(3000, 1, 1));
                                        long indexedCountThisMonth =
                                            ElasticController.Instance.GetIndexDocumentCountByDateRange(16520, 2,
                                                startMonth, endMonth);

                                        if (indexedCountAllTime > MaxOffersInDb)
                                        {
                                            _logger.Info("Cannot restart container " + container +
                                                         " - maximum offers in db reached");
                                            continue;
                                        }

                                        if (indexedCountThisMonth > maxProceedOffersInMonth)
                                        {
                                            _logger.Info("Cannot restart container " + container +
                                                         " - month offers in db reached " + indexedCountThisMonth +
                                                         " / " + maxProceedOffersInMonth);
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        _logger.Info("No category found for container: " + container + " - skipped");
                                        continue;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message + " " + ex.StackTrace);
                                _logger.Error(ex);
                            }
                        }

                        RestartContainers(azure, resourceGroupName, container);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Info("Error: " + ex.Message + " " + ex.StackTrace);
                }

                await Task.Delay(10000, stoppingToken);
            }
        }

        private static IAzure GetAzureContext(string authFilePath)
        {
            IAzure azure;
            ISubscription sub;


            Console.WriteLine($"Authenticating with Azure using credentials in file at {authFilePath}");

            azure = Azure.Authenticate(authFilePath).WithDefaultSubscription();
            sub = azure.GetCurrentSubscription();

            Console.WriteLine($"Authenticated with subscription '{sub.DisplayName}' (ID: {sub.SubscriptionId})");


            return azure;
        }


        private static IEnumerable<string> ListContainerGroups(IAzure azure, string resourceGroupName)
        {
            foreach (var containerGroup in azure.ContainerGroups.ListByResourceGroup(resourceGroupName)
                .Where(x => x.State.ToLower().Contains("succeeded") || x.State.ToLower().Contains("stopped") ||
                            x.State.ToLower().Contains("failed")))
            {
                yield return containerGroup.Name;
            }
        }

        private static void RestartContainers(IAzure azure, string resourceGroupName, string containerGroupName)
        {
            IContainerGroup containerGroup = null;
            while (containerGroup == null)
            {
                Console.Write(".");


                containerGroup = azure.ContainerGroups.GetByResourceGroup(resourceGroupName, containerGroupName);
                if (containerGroup != null)
                {
                    _logger.Info("Restarting: " + resourceGroupName + "/" + containerGroupName);
                    Thread.Sleep(10000);
                    ContainerGroupsOperationsExtensions.StartAsync(
                        containerGroup.Manager.Inner.ContainerGroups,
                        containerGroup.ResourceGroupName,
                        containerGroup.Name).GetAwaiter().GetResult();
                    SdkContext.DelayProvider.Delay(8000);
                }
            }
        }
    }
}