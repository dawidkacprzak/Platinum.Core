using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace Platinum.Service.UrlFetchTaskCreator
{
    public class Program
    {
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
        
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureLogging(loggingBuilder =>
                {
                    loggingBuilder.ClearProviders();
                    loggingBuilder.SetMinimumLevel(LogLevel.Information);
                })
                .ConfigureServices((hostContext, services) => { services.AddHostedService<AllegroFetchUrls>(); });
    }
}