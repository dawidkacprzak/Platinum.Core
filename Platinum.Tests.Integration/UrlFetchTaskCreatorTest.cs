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
        public void AllegroStartFor5Sec()
        {
            AllegroFetchUrls fetchUrl = new AllegroFetchUrls();
            Thread task = new Thread(()=>fetchUrl.StartAsync(new CancellationToken()));
            task.Start();
            Thread.Sleep(5000);
            task.Abort();
        }

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
            List<int> categories = afu.GetAllCategories().ToList();
            Assert.IsTrue(categories.Any());
            Assert.IsTrue(categories.All(x=>x != 0));
        }

        [Test]
        public void AllegroGetTaskCountNotThrow()
        {
            AllegroFetchUrls afu = new AllegroFetchUrls();
            Assert.DoesNotThrow(()=>afu.GetTaskCount());
        }
    }
}