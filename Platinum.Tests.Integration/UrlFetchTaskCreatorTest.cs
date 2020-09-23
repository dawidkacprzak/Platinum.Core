using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Platinum.Core.Model;
using Platinum.Service.UrlFetchTaskCreator;

namespace Platinum.Tests.Integration
{
    [TestFixture]
    public class UrlFetchTaskCreatorTest
    {
        [Test]
        public void AllegroGetCategoryFiltersIsNotEmpty()
        {
            AllegroFetchUrls afu = new AllegroFetchUrls();
            List<WebsiteCategoriesFilterSearch> filters =  afu.GetCategoryFilters(1).ToList();
            Assert.IsTrue(filters.Any());
            Assert.IsTrue(filters.All(x => x.WebsiteCategoryId == 1));
        }
        
        [Test]
        public void AllegroGetCategoryFiltersIsEmpty()
        {
            AllegroFetchUrls afu = new AllegroFetchUrls();
            List<WebsiteCategoriesFilterSearch> filters =  afu.GetCategoryFilters(0).ToList();
            Assert.IsTrue(!filters.Any());
        }

        [Test]
        public void AllegroGetAllCategories()
        {
            AllegroFetchUrls afu = new AllegroFetchUrls();
            var categories = afu.GetAllCategories().ToList();
            Assert.IsTrue(categories.Any());
        }

        [Test]
        public void AllegroGetTaskCountNotThrow()
        {
            AllegroFetchUrls afu = new AllegroFetchUrls();
            Assert.DoesNotThrow(()=>afu.GetTaskCount(6406,1));
        }
    }
}