using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Types;

namespace Platinum.Service.UrlFetchTaskCreator
{
    public class Program
    {
        public static int UserId { get; set; }
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSystemDependedService()
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.SetMinimumLevel(LogLevel.Information);
                })
                .ConfigureServices((hostContext, services) => { services.AddHostedService<AllegroFetchUrls>(); });
    }
}