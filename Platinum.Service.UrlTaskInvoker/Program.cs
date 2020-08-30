using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Types;

namespace Platinum.Service.UrlTaskInvoker
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
    public class Program
    {
        public static string NumberOfTasksArg;
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                NumberOfTasksArg = args[0];
            }
            else
            {
                NumberOfTasksArg = "1";
            }
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSystemDependedService()
                .ConfigureServices((hostContext, services) => { services.AddHostedService<Worker>(); });
    }
}