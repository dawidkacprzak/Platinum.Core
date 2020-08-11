using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Types;
using Platinum.Service.CategoryFetcher.Factory;

namespace Platinum.Service.CategoryFetcher
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                using (IDal dal = new Dal())
                {
                    CategoryFetcherFactory factory = new AllegroCategoryFetcherFactory();
                    factory.GetFetcher().Run(dal);
                }

                await Task.Delay(60000 * (60*24), stoppingToken);
            }
        }
    }
}