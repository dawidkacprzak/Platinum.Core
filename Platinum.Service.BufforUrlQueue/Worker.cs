using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Types;

namespace Platinum.Service.BufforUrlQueue
{
    public class Worker : BackgroundService,IBufforUrlQueueService
    {
        private IBufforUrlQueueTask queueTask;
        private IDal db;

        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
        public Worker(IDal db, IBufforUrlQueueTask queueTask)
        {
            this.queueTask = queueTask;
            this.db = db;
        }
        
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
        public Worker()
        {
            this.queueTask = new AllegroBufforUrlQueue();
            this.db = new Dal();
        }
        
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Run(db,queueTask);
                await Task.Delay(25000, stoppingToken);
            }
        }
        
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
        public void Run(IDal dataBase, IBufforUrlQueueTask task)
        {
            task.Run(dataBase);
        }
        
    }
}