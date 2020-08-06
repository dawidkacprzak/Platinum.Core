using NUnit.Framework;
using Platinum.Core.Types.Exceptions;
using Platinum.Service.UrlTaskInvoker;

namespace Platinum.Tests.Integration
{
    [TestFixture]
    public class UrlTaskInvokerTest
    {
        [Test]
        public void ResetNotExistBrowser()
        {
            AllegroTaskInvoker invoker = new AllegroTaskInvoker();
            TaskInvokerException ex = Assert.Throws<TaskInvokerException>(() => invoker.ResetBrowser("http://notExistBrowser.pl3"));
            Assert.That(ex.Message,Contains.Substring("Cannot"));
        }
    }
}