using NUnit.Framework;
using Platinum.Core.Model;
using Platinum.Core.Types;
using Platinum.Core.Types.Exceptions;

namespace Platinum.Tests.Integration
{
    public class OfferCategoryTest
    {
        [TestCase(OfferWebsite.Allegro, 1)]
        public void CheckCategoryOfferConstructorParametersPass(OfferWebsite website, int categoryId)
        {
            OfferCategory category = new OfferCategory(website, 1);
            Assert.AreEqual(category.OfferWebsite, website);
            Assert.AreEqual(category.CategoryUrl, "kategoria/kuchnia-potrawy-kuchnia-weganska-261336");
        }

        [TestCase(OfferWebsite.Allegro, 1)]
        public void CheckCategoryOfferConstructorParametersPassWithBaseCategory(OfferWebsite website,
            int categoryId)
        {
            OfferCategory offerCategoryBase = new OfferCategory(OfferWebsite.Allegro, 1);
            
            Assert.NotNull(offerCategoryBase.OfferWebsite);
            Assert.AreEqual(offerCategoryBase.OfferWebsite, OfferWebsite.Allegro);
        }

        [Test]
        public void CheckCategoryOfferEqualityPass()
        {
            OfferCategory category1 = new OfferCategory(OfferWebsite.Allegro, 1);
            OfferCategory category2 = new OfferCategory(OfferWebsite.Allegro, 1);

            Assert.AreEqual(category1, category2);
            Assert.True(category1.Equals(category2));
        }

        [Test]
        public void CheckCategoryOfferEqualityFail()
        {
            OfferCategory category1 = new OfferCategory(OfferWebsite.Allegro, 1);
            OfferCategory category2 = new OfferCategory(OfferWebsite.Allegro, "testName");
            Assert.AreNotEqual(category1, category2);
            Assert.False(category1.Equals(category2));
        }

        [Test]
        public void CheckCategoryOfferEqualityNullFail()
        {
            OfferCategory category1 = new OfferCategory(OfferWebsite.Allegro, 1);

            Assert.AreNotEqual(category1, null);
            Assert.False(category1.Equals(null));
        }

        [Test]
        public void CheckCategoryOfferEqualityNullFailInverted()
        {
            OfferCategory category1 = new OfferCategory(OfferWebsite.Allegro, 1);

            Assert.AreNotEqual(null, category1);
            Assert.False(category1.Equals(null));
        }

        [Test]
        public void CheckObjectConversionEqualityPass()
        {
            OfferCategory convertedCategory = new OfferCategory(OfferWebsite.Allegro,1);
            object objCategory = convertedCategory;
            
            Assert.AreEqual(convertedCategory,objCategory);
        }
        
        [Test]
        public void CheckObjectConversionEqualityPassFail()
        {
            OfferCategory convertedCategory = new OfferCategory(OfferWebsite.Allegro,1);
            object objCategory = null;
            
            Assert.AreNotEqual(convertedCategory,objCategory);
        }

        [Test]
        public void CheckObjectEqualityBaseOfferWebsitePass()
        {
            OfferCategory offerCategoryBase1 = new OfferCategory(OfferWebsite.Allegro, 1);

            OfferCategory offerCategory1 = new OfferCategory(OfferWebsite.Allegro,1);
            OfferCategory offerCategory2 = new OfferCategory(OfferWebsite.NoInfo, 1);
            
            Assert.AreNotEqual(offerCategory1,offerCategory2);
        }
        
        [Test]
        public void CheckCategoryNameCorrect()
        {
            OfferCategory category1 = new OfferCategory(OfferWebsite.Allegro, 1);

            Assert.AreEqual(category1.CategoryName, "Kuchnia wegańska - Książki kucharskie");
        }
        
                
        [Test]
        public void CheckNotFoundCategoryException()
        {
            DalException ex = Assert.Throws<DalException>(()=>new OfferCategory(OfferWebsite.Allegro, -1));
            Assert.That(ex, Is.Not.Null);
        }
    }
}