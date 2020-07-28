using NUnit.Framework;
using Platinum.Core.Model;
using Platinum.Core.Types;

namespace Platinum.Tests.Unit
{
    public class OfferCategoryTest
    {
        [TestCase(OfferWebsite.Allegro, "do-samochodow-dostawczych")]
        [TestCase(OfferWebsite.Allegro, "")]
        [TestCase(OfferWebsite.Allegro, "/brak")]
        public void CheckCategoryOfferConstructorParametersPass(OfferWebsite website, string categoryName)
        {
            OfferCategory category = new OfferCategory(website, categoryName);
            Assert.AreEqual(category.OfferWebsite, website);
            Assert.AreEqual(category.CategoryUrl, categoryName);
        }

        [TestCase(OfferWebsite.Allegro, "do-samochodow-dostawczych")]
        [TestCase(OfferWebsite.Allegro, "")]
        [TestCase(OfferWebsite.Allegro, "/brak")]
        public void CheckCategoryOfferConstructorParametersPassWithBaseCategory(OfferWebsite website,
            string categoryName)
        {
            OfferCategory offerCategoryBase = new OfferCategory(OfferWebsite.Allegro, "do-samochodow-dostawczych");
            OfferCategory category = new OfferCategory(website, offerCategoryBase, categoryName);
            
            Assert.NotNull(category.BaseOfferCategory);
            Assert.IsNull(offerCategoryBase.BaseOfferCategory);
        }

        [Test]
        public void CheckCategoryOfferEqualityPass()
        {
            OfferCategory category1 = new OfferCategory(OfferWebsite.Allegro, "do-samochodow-dostawczych");
            OfferCategory category2 = new OfferCategory(OfferWebsite.Allegro, "do-samochodow-dostawczych");

            Assert.AreEqual(category1, category2);
            Assert.True(category1.Equals(category2));
        }

        [Test]
        public void CheckCategoryOfferEqualityFail()
        {
            OfferCategory category1 = new OfferCategory(OfferWebsite.Allegro, "do-samochodowdostawczych");
            OfferCategory category2 = new OfferCategory(OfferWebsite.Allegro, "do-samochodow-dostawczych");

            Assert.AreNotEqual(category1, category2);
            Assert.False(category1.Equals(category2));
        }

        [Test]
        public void CheckCategoryOfferEqualityNullFail()
        {
            OfferCategory category1 = new OfferCategory(OfferWebsite.Allegro, "do-samochodowdostawczych");

            Assert.AreNotEqual(category1, null);
            Assert.False(category1.Equals(null));
        }

        [Test]
        public void CheckCategoryOfferEqualityNullFailInverted()
        {
            OfferCategory category1 = new OfferCategory(OfferWebsite.Allegro, "do-samochodowdostawczych");

            Assert.AreNotEqual(null, category1);
            Assert.False(category1.Equals(null));
        }

        [Test]
        public void CheckObjectConversionEqualityPass()
        {
            OfferCategory convertedCategory = new OfferCategory(OfferWebsite.Allegro,"testcategory");
            object objCategory = convertedCategory;
            
            Assert.AreEqual(convertedCategory,objCategory);
        }
        
        [Test]
        public void CheckObjectConversionEqualityPassFail()
        {
            OfferCategory convertedCategory = new OfferCategory(OfferWebsite.Allegro,"testcategory");
            object objCategory = null;
            
            Assert.AreNotEqual(convertedCategory,objCategory);
        }
        
        [Test]
        public void CheckObjectConversionEqualityBaseCategoryPass()
        {
            OfferCategory offerCategoryBase = new OfferCategory(OfferWebsite.Allegro, "do-samochodow");
            OfferCategory offerCategory1 = new OfferCategory(OfferWebsite.Allegro,offerCategoryBase, "do-samochodow-dostawczych");
            OfferCategory offerCategory2 = new OfferCategory(OfferWebsite.Allegro,offerCategoryBase, "do-samochodow-dostawczych");
            
            Assert.NotNull(offerCategory1.BaseOfferCategory);
            Assert.NotNull(offerCategory2.BaseOfferCategory);
            Assert.AreEqual(offerCategory1.BaseOfferCategory,offerCategory2.BaseOfferCategory);
        }
        
        [Test]
        public void CheckObjectConversionEqualityBaseCategoryFail()
        {
            OfferCategory offerCategoryBase1 = new OfferCategory(OfferWebsite.Allegro, "do-samochodow");
            OfferCategory offerCategoryBase2 = new OfferCategory(OfferWebsite.Allegro, "do");

            OfferCategory offerCategory1 = new OfferCategory(OfferWebsite.Allegro,offerCategoryBase1, "do-samochodow-dostawczych");
            OfferCategory offerCategory2 = new OfferCategory(OfferWebsite.Allegro,offerCategoryBase2, "do-samochodow-dostawczych");
            
            Assert.NotNull(offerCategory1.BaseOfferCategory);
            Assert.NotNull(offerCategory2.BaseOfferCategory);

            Assert.AreNotEqual(offerCategory1.BaseOfferCategory,offerCategory2.BaseOfferCategory);
        }

        [Test]
        public void CheckObjectEqualityBaseOfferWebsitePass()
        {
            OfferCategory offerCategoryBase1 = new OfferCategory(OfferWebsite.Allegro, "do-samochodow");

            OfferCategory offerCategory1 = new OfferCategory(OfferWebsite.Allegro,offerCategoryBase1, "do-samochodow-dostawczych");
            OfferCategory offerCategory2 = new OfferCategory(OfferWebsite.NoInfo, "do-samochodow-dostawczych");
            
            Assert.AreNotEqual(offerCategory1,offerCategory2);
        }
    }
}