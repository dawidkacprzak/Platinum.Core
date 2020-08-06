using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Moq.Language.Flow;
using NUnit.Framework;
using Platinum.Service.BufforUrlQueue;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Model;
using Platinum.Core.Types;

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
        public void CheckBufforIsRunnable()
        {
            using (Dal db = new Dal())
            {
                Mock<IBufforUrlQueueTask> task = new Mock<IBufforUrlQueueTask>();
                task.Setup(x => x.Run(It.IsAny<IDal>())).Verifiable();
                Worker worker = new Worker(db, task.Object);

                Assert.DoesNotThrow(() => worker.Run(db,task.Object));
            }
        }

        [Test]
        public void AllegroBufforUrlQueueRunnable()
        {
            using (IDal db = new Dal())
            {
                IBufforUrlQueueTask task = new AllegroBufforUrlQueue();
                task.Run(db);
            }
        }
        
        [Test]
        public void AllegroBufforUrlTransactionErrorRollbackIsInvoked()
        {
            Mock<IDal> db  = new Mock<IDal>();
            db.Setup(x => x.BeginTransaction()).Throws<Exception>();
            db.Setup(x => x.RollbackTransaction()).Verifiable();
            
            IBufforUrlQueueTask task = new AllegroBufforUrlQueue();
            task.Run(db.Object);
            
            db.Verify(x=>x.RollbackTransaction(),Times.Once);
        }
        
        [Test]
        public void AllegroBufforSelectAndMoveOffersFromBufforToOffersThrowsBackExceptionAfterExecuteNonQueryFail()
        {
            Mock<IDal> db  = new Mock<IDal>();
            db.Setup(x => x.ExecuteNonQuery(It.IsAny<string>())).Throws(new Exception("Execute non query fail"));
            
            IBufforUrlQueueTask task = new AllegroBufforUrlQueue();
            Exception ex = Assert.Throws<Exception>(()=>task.SelectAndMoveOffersFromBufforToOffers(db.Object));
            Assert.That(ex.Message,Is.EqualTo("Execute non query fail"));
        }
    }
}