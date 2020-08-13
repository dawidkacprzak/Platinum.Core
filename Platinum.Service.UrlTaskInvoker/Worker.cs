using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Factory.BrowserRestClient;
using Platinum.Core.Types;
using Platinum.Service.UrlTaskInvoker;
using Platinum.Service.UrlTaskInvoker.Factory;

namespace Platinum.Service.UrlTaskInvoker
{
    public class Worker : BackgroundService
    {
        [ExcludeFromCodeCoverage]
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            UrlTaskInvokerFactory factory = new AllegroUrlTaskInvokerFactory();
            while (!stoppingToken.IsCancellationRequested)
            {
                await RunTaskInvoker(factory.GetInvoker());

                await Task.Delay(50, stoppingToken);
            }
        }
        
        [ExcludeFromCodeCoverage]
        private async Task RunTaskInvoker(IUrlTaskInvoker task)
        {
            using (Dal db = new Dal())
            {
                await task.Run(new PlatinumABrowserRestClientFactory(), db);
            }
        }
    }
}