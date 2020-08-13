using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Model;
using Platinum.Core.Types;
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
                IOfferDetailsFetcher fetcher = factory.GetOfferDetailsFetcher("3000", 0);
                IEnumerable<Offer> offers = fetcher.GetLastNotProcessedOffers(db, offerCount);

                Assert.IsTrue(offers.Count() <= offerCount && offers.Count() > 0);
            }
        }
        [Test]
        public void GetLastNotProcessedPageDetails()
        {
            using (Dal db = new Dal())
            {
                OfferDetailsFetcherFactory factory = new AllegroOfferDetailsFetcherFactory();
                IOfferDetailsFetcher fetcher = factory.GetOfferDetailsFetcher("3001", 1);
                fetcher.Run(db);
            }
        }
    }
}