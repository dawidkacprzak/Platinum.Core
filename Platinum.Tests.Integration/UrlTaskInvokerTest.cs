using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Factory.BrowserRestClient;
using Platinum.Core.Model;
using Platinum.Core.OfferDetailsParser;
using Platinum.Core.Types;
using Platinum.Core.Types.Exceptions;
using Platinum.Service.UrlTaskInvoker;
using Platinum.Tests.Integration.Stubs;

namespace Platinum.Tests.Integration
{
    [TestFixture]
    public class UrlTaskInvokerTest
    {
        [Test]
        public void PopFromQueueDbExceptionRollbackInvoked()
        {
            AllegroTaskInvoker invoker = new AllegroTaskInvoker();
            Mock<IDal> dbMock = new Mock<IDal>();
            dbMock.Setup(x => x.BeginTransaction()).Throws(new DalException("Test message"));
            dbMock.Setup(x => x.RollbackTransaction()).Verifiable();

            Assert.DoesNotThrow(() => invoker.PopTaskFromQueue(dbMock.Object, 6));
            dbMock.Verify(x => x.RollbackTransaction(), Times.Once);
        }

        [Test]
        public void PopFromQueuePassCommitInvoked()
        {
            AllegroTaskInvoker invoker = new AllegroTaskInvoker();
            Mock<IDal> dbMock = new Mock<IDal>();
            dbMock.Setup(x => x.CommitTransaction()).Verifiable();
            Assert.DoesNotThrow(() => invoker.PopTaskFromQueue(dbMock.Object, 6));
            dbMock.Verify(x => x.CommitTransaction(), Times.Once);
        }

        [Test]
        public void PopFromQueueUnexpectedException()
        {
            AllegroTaskInvoker invoker = new AllegroTaskInvoker();
            Mock<IDal> dbMock = new Mock<IDal>();
            dbMock.Setup(x => x.CommitTransaction()).Throws<Exception>();
            Assert.DoesNotThrow(() => invoker.PopTaskFromQueue(dbMock.Object, 6));
            dbMock.Verify(x => x.RollbackTransaction(), Times.Once);
        }
        

        [Test]
        public void GetOldestTaskReaderHasNoRows()
        {
            Mock<IDal> dbMock = new Mock<IDal>();
            dbMock.Setup(x => x.ExecuteReader(It.IsAny<string>())).Returns(new DbDataReaderNoRows());
            
            AllegroTaskInvoker invoker = new AllegroTaskInvoker();
            TaskInvokerException ex = Assert.Throws<TaskInvokerException>(()=>invoker.GetOldestTask(dbMock.Object));
            
            Assert.That(ex.Message, Contains.Substring("Cannot get olders task. Not found any."));
        }
        
        [Test]
        public void GetOldestTaskWithActiveTaskIds()
        {
            AllegroTaskInvoker invoker = new AllegroTaskInvoker();
            invoker.ActiveTasksId = new List<string>()
            {
                "1","2","3"
            };
            using (Dal db = new Dal())
            {
                Assert.DoesNotThrow(() => invoker.GetOldestTask(db));
            }
        }

    }
}