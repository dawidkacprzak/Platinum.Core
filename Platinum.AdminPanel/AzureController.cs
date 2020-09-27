using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Platinum.AdminPanel.Model;

namespace Platinum.AdminPanel
{
    public class AzureController
    {
        private IAzure azure;
        private string resourceGroupName = "container_Registry_fetcher";

        public void LoginToAzure()
        {
            string authFilePath = "my.azureauth";
            azure = GetAzureContext(authFilePath);
        }

        private IAzure GetAzureContext(string authFilePath)
        {
            IAzure azure;
            ISubscription sub;
            azure = Azure.Authenticate(authFilePath).WithDefaultSubscription();
            sub = azure.GetCurrentSubscription();

            return azure;
        }

        public IEnumerable<AzureContainerRow> GetUserContainers(int userId)
        {
            foreach (var containerGroup in azure.ContainerGroups.ListByResourceGroup(resourceGroupName)
                .Where(x => x.Name.Contains("client-" + userId)).ToList())
            {
                yield return new AzureContainerRow()
                {
                    ContainerName = containerGroup.Name,
                    ContainerStatus = containerGroup.State,
                    ContainerType = containerGroup.Name.ToLower().Contains("detailsfetcher")
                        ? "Details fetcher"
                        : "Url fetcher",
                    Ip = containerGroup.IPAddress
                };
            }
        }
    }
}