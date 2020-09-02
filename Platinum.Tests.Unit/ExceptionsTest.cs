using System;
using NUnit.Framework;
using Platinum.Core.OfferListController;
using Platinum.Core.Types.Exceptions;
using RestSharp;

namespace Platinum.Tests.Unit
{
    public class ExceptionsTest
    {
        [Test]
        public void CheckPlatinumBrowserRequestExceptionType()
        {
            RequestException ex = new RequestException("PageNotInitied", new RestResponse());
            Assert.IsInstanceOf<Exception>(ex);
        }
        
        [Test]
        public void CheckOfferListControllerExceptionType()
        {
            OfferListControllerException ex = new OfferListControllerException("Example message", new BrowserAllegroOfferListController());
            Assert.IsInstanceOf<Exception>(ex);
            Assert.IsInstanceOf<BrowserAllegroOfferListController>(ex.InvalidController);
        }
        
        [Test]
        public void CheckDalExceptionType()
        {
            DalException ex = new DalException("Example message");
            Assert.IsInstanceOf<Exception>(ex);
        }
        
        [Test]
        public void CheckTaskInvokerExceptionTypeInner()
        {
            TaskInvokerException ex = new TaskInvokerException("Example message", new Exception());
            Assert.IsInstanceOf<Exception>(ex);
            Assert.IsInstanceOf<TaskInvokerException>(ex);
            Assert.AreEqual(ex.Message, "Example message");
            Assert.IsInstanceOf<Exception>(ex.InnerException);
        }
        
        [Test]
        public void CheckTaskInvokerExceptionTypeMessage()
        {
            TaskInvokerException ex = new TaskInvokerException("Example message");
            Assert.IsInstanceOf<Exception>(ex);
            Assert.IsInstanceOf<TaskInvokerException>(ex);
            Assert.AreEqual(ex.Message, "Example message");
        }
        
        [Test]
        public void CheckTaskInvokerExceptionType()
        {
            TaskInvokerException ex = new TaskInvokerException();
            Assert.IsInstanceOf<TaskInvokerException>(ex);
        }
    }
}