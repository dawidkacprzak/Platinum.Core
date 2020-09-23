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
        public static int WebApiUserId;
        [ExcludeFromCodeCoverage]
        public Worker(IHostApplicationLifetime hostApplicationLifetime)
        {
            lifetimeApp = hostApplicationLifetime;
        }

        [ExcludeFromCodeCoverage]
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int pararellTasks = 0;
            if (Program.AppArgs.Count() < 2)
            {
                #if DEBUG
                WebApiUserId = 2;
                pararellTasks = 1;
                //configure by hand
                #endif
                Console.WriteLine("Error, application MUST contain 2 arguments - user id and tasks count");
            }
            else
            {
                if (int.TryParse(Program.AppArgs[0], out _) && int.TryParse(Program.AppArgs[1], out _))
                {
                    int userId = int.Parse(Program.AppArgs[0]);
                    pararellTasks = int.Parse(Program.AppArgs[1]);
                    using (IDal db = new Dal())
                    {
                        int userCount =
                            (int) db.ExecuteScalar(
                                "SELECT COUNT(*) FROM WebApiUsers with (nolock) where Id = " + userId);
                        if (userCount == 0)
                        {
                            throw new Exception($"User with id {userId} cannot be fount");
                        }
                        else
                        {
                            WebApiUserId = userId;
                        }
                    }
                }
                else
                {
                    throw new Exception("User id cannot be parsed to int. Val: " + Program.AppArgs[0]);
                }
            }

            OfferDetailsFetcherFactory factory = new AllegroOfferDetailsFetcherFactory();
            IOfferDetailsFetcher fetcher = factory.GetOfferDetailsFetcher(pararellTasks);
            using (Dal db = new Dal())
            {
                fetcher.Run(db);
            }

            await Task.Delay(5000, stoppingToken);
            lifetimeApp.StopApplication();
        }
    }
}