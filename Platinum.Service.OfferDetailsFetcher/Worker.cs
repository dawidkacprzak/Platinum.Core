using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Types;
using Platinum.Service.OfferDetailsFetcher.Factory;

namespace Platinum.Service.OfferDetailsFetcher
{
    public class Worker : BackgroundService
    {

        public Worker()
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                OfferDetailsFetcherFactory factory = new AllegroOfferDetailsFetcherFactory();
                IOfferDetailsFetcher fetcher = factory.GetOfferDetailsFetcher("3001", 10);
                using (Dal db = new Dal())
                {
                    fetcher.Run(db);
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}