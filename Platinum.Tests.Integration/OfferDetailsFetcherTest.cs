using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Model;
using Platinum.Core.Types;
using Platinum.Service.OfferDetailsFetcher;
using Platinum.Service.OfferDetailsFetcher.Factory;

namespace Platinum.Tests.Integration
{
    [TestFixture]
    public class OfferDetailsFetcherTest
    {
        [TestCase(4)]
        [TestCase(1)]
        [TestCase(20)]
        [TestCase(50)]
        public void GetLastNotProcessedOffersDoNotDeadLock(int offerCount)
        {
            using (Dal db = new Dal())
            {
                OfferDetailsFetcherFactory factory = new AllegroOfferDetailsFetcherFactory();
                IOfferDetailsFetcher fetcher = factory.GetOfferDetailsFetcher( 0);
                IEnumerable<Offer> offers = fetcher.GetLastNotProcessedOffers(db, offerCount);
                int offersCount = offers.Count();
                Assert.IsTrue(offersCount <= offerCount && offersCount > 0);
            }
        }
        [Test]
        public void GetLastNotProcessedPageDetails()
        {
            using (Dal db = new Dal())
            {
                OfferDetailsFetcherFactory factory = new AllegroOfferDetailsFetcherFactory();
                IOfferDetailsFetcher fetcher = factory.GetOfferDetailsFetcher(1);
                fetcher.Run(db);
            }
        }

        [Test]
        public void SetOffersAsInProcessErrorThrowsUp()
        {
            Mock<IDal> db = new Mock<IDal>();
            db.Setup(x => x.ExecuteNonQuery(It.IsAny<string>())).Throws(new Exception("Error test"));
            AllegroOfferDetailsFetcher fetcher = new AllegroOfferDetailsFetcher(10);
            
            Exception ex = Assert.Throws<Exception>(() => fetcher.SetOfferAsProcessed(db.Object,new Offer()));
            
            Assert.That(ex.Message,Contains.Substring("Error test"));
        }
        
        [Test]
        public void SetOffersAsUnprocessedErrorThrowsUp()
        {
            Mock<IDal> db = new Mock<IDal>();
            db.Setup(x => x.ExecuteNonQuery(It.IsAny<string>())).Throws(new Exception("Error test"));
            AllegroOfferDetailsFetcher fetcher = new AllegroOfferDetailsFetcher(10);
            
            Exception ex = Assert.Throws<Exception>(() => fetcher.SetOfferAsUnprocessed(db.Object,new Offer()));
            
            Assert.That(ex.Message,Contains.Substring("Error test"));
        }
        
        [Test]
        public void SetOffersAsInactiveErrorThrowsUp()
        {
            Mock<IDal> db = new Mock<IDal>();
            db.Setup(x => x.ExecuteNonQuery(It.IsAny<string>())).Throws(new Exception("Error test"));
            AllegroOfferDetailsFetcher fetcher = new AllegroOfferDetailsFetcher(10);
            
            Exception ex = Assert.Throws<Exception>(() => fetcher.SetOfferAsInActive(db.Object,new Offer()));
            
            Assert.That(ex.Message,Contains.Substring("Error test"));
        }
        
        [Test]
        public void ExceuteUpdateOfferStatusQueries()
        {
            Mock<IDal> db = new Mock<IDal>();
            AllegroOfferDetailsFetcher fetcher = new AllegroOfferDetailsFetcher(10);
            fetcher.SetOfferAsProcessed(db.Object,new Offer());
            fetcher.SetOfferAsUnprocessed(db.Object,new Offer());
            fetcher.SetOfferAsInActive(db.Object,new Offer());
            fetcher.SetOffersAsInProcess(db.Object,new List<Offer>()
            {
                new Offer()
            });
        }
    }
}