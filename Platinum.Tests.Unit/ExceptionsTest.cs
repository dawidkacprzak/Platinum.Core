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
            OfferListControllerException ex = new OfferListControllerException("Example message", new AllegroOfferListController());
            Assert.IsInstanceOf<Exception>(ex);
            Assert.IsInstanceOf<AllegroOfferListController>(ex.InvalidController);
        }
        
        [Test]
        public void CheckDalExceptionType()
        {
            DalException ex = new DalException("Example message");
            Assert.IsInstanceOf<Exception>(ex);
        }
    }
}