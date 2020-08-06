using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using Platinum.Core.Types;
using Platinum.Service.UrlTaskInvoker;

namespace Platinum.Service.UrlTaskInvoker
{
    public class Worker : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                IUrlTaskInvoker invoker = new AllegroTaskInvoker();
                await RunTaskInvoker(new AllegroTaskInvoker());
                
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task RunTaskInvoker(IUrlTaskInvoker task)
        {
            await task.Run();
        }
    }
}