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
            bool exist = ElasticController.Instance.OfferExistsInBuffor("https://allegro.pl/oferta/opony-18x8-8-journey-p340-9545976287");
            Assert.IsTrue(exist);
        }

        [Test]
        public void InsertOffer()
        {
            Offer offer = new Offer(3,(int)EOfferWebsite.Allegro,"https://test.pl",new byte[]{0},DateTime.Now,0);
            Assert.DoesNotThrow(()=>ElasticController.Instance.InsertOffer(offer));
        }

        [Test]
        public void CheckOfferDetailsExist()
        {
            for (int i = 0; i < 50; i++)
            {
                ElasticController.Instance.OfferDetailsExists(i);
            }
        }
    }
}