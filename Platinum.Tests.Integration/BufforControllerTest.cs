using System;
using NUnit.Framework;
using Platinum.Core.ElasticIntegration;
using Platinum.Core.Model;
using Platinum.Core.Types;

namespace Platinum.Tests.Integration
{
    public class BufforControllerTest
    {
        [Test]
        public void CheckOfferExist()
        {
            bool exist = BufforController.Instance.OfferExistsInBuffor("https://allegro.pl/oferta/1x-opona-25x10-12-maxxis-all-trak-c9209-38j-9564014457");
            Assert.IsTrue(exist);
        }

        [Test]
        public void InsertOffer()
        {
            Offer offer = new Offer(3,(int)OfferWebsite.Allegro,"https://test.pl",new byte[]{0},DateTime.Now,0);
            Assert.DoesNotThrow(()=>BufforController.Instance.InsertOffer(offer));
        }
    }
}