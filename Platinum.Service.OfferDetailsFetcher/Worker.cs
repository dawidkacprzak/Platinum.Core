using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Types;
using Platinum.Service.OfferDetailsFetcher.Factory;

namespace Platinum.Service.OfferDetailsFetcher
{
    public class Worker : BackgroundService
    {

        [ExcludeFromCodeCoverage]
        public Worker()
        {
        }
        
        [ExcludeFromCodeCoverage]
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (Program.AppArgs.Count() < 2)
            {
                Console.WriteLine("Error, application MUST contain 1 arguments - tasks count, default 10 set");
                Program.AppArgs = new[] {"1"};
            }
            while (!stoppingToken.IsCancellationRequested)
            {
                OfferDetailsFetcherFactory factory = new AllegroOfferDetailsFetcherFactory();
                IOfferDetailsFetcher fetcher = factory.GetOfferDetailsFetcher(int.Parse(Program.AppArgs[0]));
                using (Dal db = new Dal())
                {
                    fetcher.Run(db);
                }

                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}