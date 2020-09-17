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
        IHostApplicationLifetime lifetimeApp;
        [ExcludeFromCodeCoverage]
        public Worker(IHostApplicationLifetime hostApplicationLifetime)
        {
            lifetimeApp = hostApplicationLifetime;
        }

        [ExcludeFromCodeCoverage]
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (Program.AppArgs.Count() < 1)
            {
                Console.WriteLine("Error, application MUST contain 1 arguments - tasks count, default 1 set");
                Program.AppArgs = new[] { "5" };
            }

            OfferDetailsFetcherFactory factory = new AllegroOfferDetailsFetcherFactory();
            IOfferDetailsFetcher fetcher = factory.GetOfferDetailsFetcher(int.Parse(Program.AppArgs[0]));
            using (Dal db = new Dal())
            {
                fetcher.Run(db);
            }

            await Task.Delay(5000, stoppingToken);
            lifetimeApp.StopApplication();
        }
    }
}