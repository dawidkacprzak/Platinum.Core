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
        public static int UserId;
        public static int CategoryId;
        public static void Main(string[] args)
        {
            Console.WriteLine("Args: " + string.Join("/",args));
            if (args.Length < 3)
            {
                #if DEBUG
                NumberOfTasksArg = "1";
                UserId = 1;
                CategoryId = 6406;
#else
                Console.WriteLine("User id and task count cannot be empty (first app argument)");
                throw new Exception("User id and task count cannot be empty (first app argument)");
#endif
            }
            else
            {
                if (int.TryParse(args[0], out _) && int.TryParse(args[1], out _) && int.TryParse(args[2], out _))
                {
                    int userId = int.Parse(args[0]);
                    int taskCount = int.Parse(args[1]);
                    CategoryId = int.Parse(args[2]);
                    NumberOfTasksArg = taskCount.ToString();
                    using (IDal db = new Dal())
                    {
                        int userCount = (int)db.ExecuteScalar("SELECT COUNT(*) FROM WebApiUsers with (nolock) where Id = " + userId);
                        if (userCount == 0)
                        {
                            Console.WriteLine($"User with id {userId} cannot be fount");
                            throw  new Exception($"User with id {userId} cannot be fount");
                        }
                        else
                        {
                            UserId = userId;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("User id cannot be parsed to int. Val: " + args[0]);
                    throw new Exception("User id cannot be parsed to int. Val: " + args[0]);
                }
            }
            
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSystemDependedService()
                .ConfigureServices((hostContext, services) => { services.AddHostedService<Worker>(); });
    }
}