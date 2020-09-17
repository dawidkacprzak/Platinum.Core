using System;
using System.Collections.Generic;
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

                    // Authenticate with Azure
                    IAzure azure = GetAzureContext(authFilePath);

                    IEnumerable<string> containers = ListContainerGroups(azure, resourceGroupName);
                    var enumerable = containers as string[] ?? containers.ToArray();
                    _logger.Info($"found: " + enumerable.Count() +
                                 $" containers to restart. ({string.Join(",", enumerable)})");
                    foreach (var container in enumerable)
                    {
                        RestartContainers(azure, resourceGroupName, container);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Info("Error: "+ex.Message +" " + ex.StackTrace);
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
                .Where(x => x.State.ToLower().Contains("succeeded") || x.State.ToLower().Contains("stopped")))
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