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
            if (Program.AppArgs.Count() < 2)
            {
                Console.WriteLine("Error, application MUST contain 2 arguments.  application Port and tasks count");
            }
            while (!stoppingToken.IsCancellationRequested)
            {
                OfferDetailsFetcherFactory factory = new AllegroOfferDetailsFetcherFactory();
                IOfferDetailsFetcher fetcher = factory.GetOfferDetailsFetcher(Program.AppArgs[0], int.Parse(Program.AppArgs[1]));
                using (Dal db = new Dal())
                {
                    fetcher.Run(db);
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}