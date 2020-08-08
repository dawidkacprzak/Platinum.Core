using NUnit.Framework;
using Platinum.Core.Model;
using Platinum.Core.Types;
using Platinum.Core.Types.Exceptions;

namespace Platinum.Tests.Integration
{
    public class OfferCategoryTest
    {
        [TestCase(EOfferWebsite.Allegro, 1)]
        public void CheckCategoryOfferConstructorParametersPass(EOfferWebsite website, int categoryId)
        {
            OfferCategory category = new OfferCategory(website, 1);
            Assert.AreEqual(category.EOfferWebsite, website);
            Assert.AreEqual(category.CategoryUrl, "kategoria/gadzety-cukierki-260532");
        }

        [TestCase(EOfferWebsite.Allegro, 1)]
        public void CheckCategoryOfferConstructorParametersPassWithBaseCategory(EOfferWebsite website,
            int categoryId)
        {
            OfferCategory offerCategoryBase = new OfferCategory(EOfferWebsite.Allegro, 1);

            Assert.NotNull(offerCategoryBase.EOfferWebsite);
            Assert.AreEqual(offerCategoryBase.EOfferWebsite, EOfferWebsite.Allegro);
        }

        [Test]
        public void CheckCategoryOfferEqualityPass()
        {
            OfferCategory category1 = new OfferCategory(EOfferWebsite.Allegro, 1);
            OfferCategory category2 = new OfferCategory(EOfferWebsite.Allegro, 1);

            Assert.AreEqual(category1, category2);
            Assert.True(category1.Equals(category2));
        }

        [Test]
        public void CheckCategoryOfferEqualityFail()
        {
            OfferCategory category1 = new OfferCategory(EOfferWebsite.Allegro, 1);
            OfferCategory category2 = new OfferCategory(EOfferWebsite.Allegro, "testName");
            Assert.AreNotEqual(category1, category2);
            Assert.False(category1.Equals(category2));
        }

        [Test]
        public void CheckCategoryOfferEqualityNullFail()
        {
            OfferCategory category1 = new OfferCategory(EOfferWebsite.Allegro, 1);

            Assert.AreNotEqual(category1, null);
            Assert.False(category1.Equals(null));
        }

        [Test]
        public void CheckCategoryOfferEqualityNullFailInverted()
        {
            OfferCategory category1 = new OfferCategory(EOfferWebsite.Allegro, 1);

            Assert.AreNotEqual(null, category1);
            Assert.False(category1.Equals(null));
        }

        [Test]
        public void CheckObjectConversionEqualityPass()
        {
            OfferCategory convertedCategory = new OfferCategory(EOfferWebsite.Allegro, 1);
            object objCategory = convertedCategory;

            Assert.AreEqual(convertedCategory, objCategory);
        }

        [Test]
        public void CheckObjectConversionEqualityPassFail()
        {
            OfferCategory convertedCategory = new OfferCategory(EOfferWebsite.Allegro, 1);

            Assert.AreNotEqual(convertedCategory, null);
        }

        [Test]
        public void CheckObjectEqualityBaseOfferWebsitePass()
        {
            OfferCategory offerCategory1 = new OfferCategory(EOfferWebsite.Allegro, 1);
            OfferCategory offerCategory2 = new OfferCategory(EOfferWebsite.NoInfo, 1);

            Assert.AreNotEqual(offerCategory1, offerCategory2);
        }

        [Test]
        public void CheckCategoryNameCorrect()
        {
            OfferCategory category1 = new OfferCategory(EOfferWebsite.Allegro, 1);

            Assert.AreEqual(category1.CategoryName, "Cukierki reklamowe");
        }


        [Test]
        public void CheckNotFoundCategoryException()
        {
            OfferCategory offerCategory;
            DalException ex = Assert.Throws<DalException>(() =>
                offerCategory = new OfferCategory(EOfferWebsite.Allegro, -1)
            );
            Assert.That(ex, Is.Not.Null);
        }
    }
}