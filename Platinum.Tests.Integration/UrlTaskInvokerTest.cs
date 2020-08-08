using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Factory.BrowserRestClient;
using Platinum.Core.Model;
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
        public void ResetBrowserThrowNoToCrashRunMethod()
        {
            Mock<IBrowserRestClient> browser = new Mock<IBrowserRestClient>();
            browser.Setup(x => x.ResetBrowser()).Throws<Exception>();

            Mock<IBrowserRestClientFactory> restClientFactory = new Mock<IBrowserRestClientFactory>();
            restClientFactory.Setup(x => x.GetBrowser(It.IsAny<string>())).Returns(browser.Object);

            AllegroTaskInvoker invoker = new AllegroTaskInvoker();
            using (Dal db = new Dal())
            {
                Assert.DoesNotThrow(() => invoker.Run(restClientFactory.Object, db));
            }
        }

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
        public void GetBrowsersDalExceptionThrowsUp()
        {
            AllegroTaskInvoker invoker = new AllegroTaskInvoker();
            Mock<IDal> dbMock = new Mock<IDal>();
            dbMock.Setup(x => x.ExecuteReader(It.IsAny<string>())).Throws(new Exception("test"));
            List<string> browsers;
            Exception ex = Assert.Throws<Exception>(() => invoker.GetBrowsers(dbMock.Object).ToList());
            Assert.That(ex.Message, Is.EqualTo("test"));
        }

        [Test]
        public void GetBrowsersDalReaderNoRows()
        {
            AllegroTaskInvoker invoker = new AllegroTaskInvoker();
            Mock<IDal> dbMock = new Mock<IDal>();
            dbMock.Setup(x => x.ExecuteReader(It.IsAny<string>())).Returns(new DbDataReaderNoRows());

            TaskInvokerException ex =
                Assert.Throws<TaskInvokerException>(() => invoker.GetBrowsers(dbMock.Object).ToList());

            Assert.That(ex.Message, Is.EqualTo("No browsers found"));
        }

        [Test]
        public void GetBrowsers()
        {
            AllegroTaskInvoker invoker = new AllegroTaskInvoker();
            using (Dal db = new Dal())
            {
                List<string> browsers = invoker.GetBrowsers(db).ToList();
                Assert.IsNotEmpty(browsers);
            }
        }

        [Test]
        public void ResetBrowserRequestExceptionThrowsTaskInvokerException()
        {
            AllegroTaskInvoker invoker = new AllegroTaskInvoker();
            Mock<IBrowserRestClient> browserMock = new Mock<IBrowserRestClient>();
            browserMock.Setup(x => x.ResetBrowser()).Throws(new RequestException("test"));

            TaskInvokerException ex =
                Assert.Throws<TaskInvokerException>(
                    () => invoker.ResetBrowser(browserMock.Object, "http://192.168.0.0"));

            Assert.That(ex.Message, Contains.Substring("Cannot reset browser"));
        }

        [Test]
        public void ResetBrowserPass()
        {
            AllegroTaskInvoker invoker = new AllegroTaskInvoker();
            Mock<IBrowserRestClient> browserMock = new Mock<IBrowserRestClient>();

            Assert.DoesNotThrow(() => invoker.ResetBrowser(browserMock.Object, "http://192.168.0.0"));
        }

        [Test]
        public void GetUrlFetchingTasksWithZeroBrowsersReturns0Tasks()
        {
            AllegroTaskInvoker invoker = new AllegroTaskInvoker();
            Task[] tasks = invoker.GetUrlFetchingTasks(new List<string>());

            Assert.IsTrue(tasks.Length == 0);
        }

        [Test]
        public void GetUrlFetchingTasksWithZeroBrowsersReturnsSameTasksAsCount()
        {
            AllegroTaskInvoker invoker = new AllegroTaskInvoker();
            Task[] tasks = invoker.GetUrlFetchingTasks(new List<string>()
            {
                "1", "2", "3"
            });

            Assert.IsTrue(tasks.Length == 3);
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