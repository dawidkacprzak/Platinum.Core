using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Model;

namespace Platinum.BufforUrlQueue
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                List<Offer> cachedOffers = GetOldest10Offers().ToList();
                ProceedAndPopFromBuffor(new Offer(1,1,null,new byte[]{1,2,4,3,4},DateTime.Now, 1));
                await Task.Delay(1000, stoppingToken);
            }
        }

        public void ProceedAndPopFromBuffor(Offer offer)
        {
            using (Dal db = new Dal())
            {
     
                    db.ExecuteReader($"SELECT COUNT(*) FROM offers where UriHash = '{offer.UriHash}'");

 

            }
        }

        public IEnumerable<Offer> GetOldest10Offers()
        {
            using (Dal db = new Dal())
            {
                using (DbDataReader reader = db.ExecuteReader("SELECT top 10 * FROM offersBuffor"))
                {
                    while (reader.Read())
                    {
                        yield return new Offer(
                            reader.GetInt32(reader.GetOrdinal("Id")),
                            reader.GetInt32(reader.GetOrdinal("WebsiteId")),
                            reader.GetString(reader.GetOrdinal("Uri")),
                            (byte[]) reader["UriHash"],
                            reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                            reader.GetInt32(reader.GetOrdinal("WebsiteCategoryId"))
                        );
                    }
                }
            }
        } 
    }
}