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
        public static IHostApplicationLifetime lifetimeApp;

        public Worker(IHostApplicationLifetime hostApplicationLifetime)
        {
            lifetimeApp = hostApplicationLifetime;
        }

        [ExcludeFromCodeCoverage]
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            UrlTaskInvokerFactory factory = new AllegroUrlTaskInvokerFactory();

            await RunTaskInvoker(factory.GetInvoker());
            lifetimeApp.StopApplication();
        }

        [ExcludeFromCodeCoverage]
        private async Task RunTaskInvoker(IUrlTaskInvoker task)
        {
            await task.Run();
        }
    }
}