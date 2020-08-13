using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Platinum.Core.Types;

namespace Platinum.Service.OfferDetailsFetcher
{
    public class Program
    {
        public static string[] AppArgs { get; set; }
        public static void Main(string[] args)
        {
            AppArgs = args;
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSystemDependedService()
                .ConfigureServices((hostContext, services) => { services.AddHostedService<Worker>(); });
    }
}