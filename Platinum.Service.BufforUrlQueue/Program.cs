using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Platinum.Core.Types;

namespace Platinum.Service.BufforUrlQueue
{
    public class Program 
    {
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
        public static void Main(string[] args)
        {
            IHost host = CreateHostBuilder(args).Build();
            if (!args.Contains("doNotRun"))
            {
                host.Run();
            }
        }
        
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices(
                    (host,services) => ConfigureServices(host,services)
                );
        }
        
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
        public static IServiceCollection ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            return services.AddHostedService<Worker>();
        }

    }
}