using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        
        [ExcludeFromCodeCoverage]
        public static void Main(string[] args)
        {
            AppArgs = args;
            CreateHostBuilder(args).Build().Run();
        }
        
        [ExcludeFromCodeCoverage]
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSystemDependedService()
                .ConfigureServices((hostContext, services) => { services.AddHostedService<Worker>(); });
    }
}