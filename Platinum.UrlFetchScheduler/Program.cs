using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Platinum.Core.ApiIntegration;

namespace Platinum.UrlFetchScheduler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            PlatinumBrowserRestClient client = new PlatinumBrowserRestClient();
            client.InitBrowser();
            
            Thread.Sleep(3000);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                    loggingBuilder.AddNLog();
                })
                .ConfigureServices((hostContext, services) => { services.AddHostedService<AllegroFetchUrls>(); });
    }
}