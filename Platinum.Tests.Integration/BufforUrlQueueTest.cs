using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Moq.Language.Flow;
using NUnit.Framework;
using Platinum.Service.BufforUrlQueue;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Model;

namespace Platinum.Tests.Integration
{
    [TestFixture]
    public class BufforUrlQueueTest
    {
        [Test]
        public void ConfigureServicesDoNotThrow()
        {
            Assert.DoesNotThrow(() => Program.ConfigureServices(null, new ServiceCollection()));
        }

        [Test]
        public void WorkerDoNotThrow()
        {
            Worker worker = new Worker();
            using (Dal db = new Dal())
            {
                Assert.DoesNotThrow(() => worker.ProceedAndPopFromBuffor(db, new Offer(
                    0, 0, "https://allegro.pl/ofertatestowa", Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()), DateTime.Now, 0)
                ));
            }
        }

        [Test]
        public void GetLastOffersNotEmpty()
        {
            Worker worker = new Worker();
            IEnumerable<Offer> offers = worker.GetOldest50Offers();
            foreach (Offer oldest50Offer in offers)
            {
                Assert.IsNotNull(oldest50Offer);
            }

            

            Assert.IsNotNull(offers);
        }

        [Test]
        public void InsertOfferTest()
        {
            Worker worker = new Worker();
            using (Dal db = new Dal())
            {
                db.BeginTransaction();
                worker.InsertOffer(db, new Offer(
                    0, 0, Guid.NewGuid().ToString(), Encoding.ASCII.GetBytes(Guid.NewGuid().ToString()), DateTime.Now, 0)
                );
                db.RollbackTransaction();
            }
        }
    }
}