using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Platinum.Core.Model;
using Platinum.Core.OfferListController;
using Platinum.Core.Types;
using Platinum.Core.Types.Exceptions;

namespace Platinum.Tests.Integration
{
    [TestFixture]
    public class HttpAllegroOfferListControllerTest
    {
        private IBaseOfferListController controller;
        private int mainCategory = 1;
        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
            Thread.Sleep(2000);
        }

        [SetUp]
        public void SetController()
        {
            controller = new HttpAllegroOfferListController();
        }

        [Test]
        public void FetchingTakesGoodAmountOfMaxPageIndex()
        {
            controller.StartFetching(true,
                new OfferCategory(EOfferWebsite.Allegro,
                    mainCategory));
        
            int maxPages = controller.GetLastPageIndex();
            Assert.NotZero(maxPages);
            Assert.AreEqual(maxPages, controller.GetLastPageIndex());
        }


        [Test]
        public void OpenNextPageWithoutStartFetching()
        {
            using IBaseOfferListController allegroOfferListController = new HttpAllegroOfferListController();
            OfferListControllerException ex =
                Assert.Throws<OfferListControllerException>(() => allegroOfferListController.OpenNextPage());
            Assert.That(ex, Is.Not.Null);
            allegroOfferListController.Dispose();
        }

        [Test]
        public void OpenNextPageWithStartFetching()
        {
            controller.StartFetching(true,
                new OfferCategory(EOfferWebsite.Allegro,
                    mainCategory));
            int page = controller.GetCurrentPageIndex();
            Assert.True(controller.OpenNextPage());
            Assert.True(controller.GetCurrentPageIndex() == page + 1);
        }

        [Test]
        public void GetAllOffersSuccess()
        {
            controller.StartFetching(true,
                new OfferCategory(EOfferWebsite.Allegro,
                    mainCategory));
            List<string> offers = controller.GetAllOfferLinks().ToList();
            Assert.True(offers.Any());
            Assert.True(offers.Count(x => !x.Contains("http")) == 0);
        }

        [Test]
        public void FetchFailAllegroLink()
        {
            using IBaseOfferListController ctr = new HttpAllegroOfferListController();
            OfferListControllerException ex = Assert.Throws<OfferListControllerException>(
                () => ctr.StartFetching(false, new OfferCategory(EOfferWebsite.Allegro, "testfaillink")));

            Assert.That(ex, Is.Not.Null);
            ctr.Dispose();
            controller.Dispose();
        }

        [Test]
        public void IndexPageFailAllegroLink()
        {
            using IBaseOfferListController ctr = new HttpAllegroOfferListController();

            OfferListControllerException e =
                Assert.Throws<OfferListControllerException>(
                    () => ctr.StartFetching(false, new OfferCategory(EOfferWebsite.Allegro, "/testfaillink")));
            OfferListControllerException ex =
                Assert.Throws<OfferListControllerException>(() => ctr.GetCurrentPageIndex());
            Assert.That(e, Is.Not.Null);
            Assert.That(ex, Is.Not.Null);
            ctr.Dispose();
            controller.Dispose();
        }

        [Test]
        public void IndexPageFailAllegroLinkNoCategoryWithPagination()
        {
            using IBaseOfferListController ctr = new HttpAllegroOfferListController();
            Assert.Throws<OfferListControllerException>(
                () => ctr.StartFetching(false,
                    new OfferCategory(EOfferWebsite.Allegro, "uzytkownik/KOLEKCJONER-PL--/oceny")));
            OfferListControllerException ex =
                Assert.Throws<OfferListControllerException>(() => ctr.GetCurrentPageIndex());
            Assert.That(ex, Is.Not.Null);
            ctr.Dispose();
            controller.Dispose();
        }

        [Test]
        public void IndexPageFailAllegroLinkNoCategoryWithPaginationLastIndex()
        {
            using IBaseOfferListController ctr = new HttpAllegroOfferListController();
            Assert.Throws<OfferListControllerException>(
                () => ctr.StartFetching(false,
                    new OfferCategory(EOfferWebsite.Allegro, "uzytkownik/KOLEKCJONER-PL--/oceny")));
            OfferListControllerException ex =
                Assert.Throws<OfferListControllerException>(() => ctr.GetLastPageIndex());
            Assert.That(ex, Is.Not.Null);
            ctr.Dispose();
            controller.Dispose();
        }

        [Test]
        public void CheckLastPageIndexIsGreater()
        {
            controller.StartFetching(true,
                new OfferCategory(EOfferWebsite.Allegro,
                    mainCategory));
            int lastPage = controller.GetLastPageIndex();
            int currentPage = controller.GetCurrentPageIndex();
            Assert.True(lastPage >= currentPage);
        }

        [Test]
        public void CurrentPageIndexSuccess()
        {
            controller.StartFetching(true,
                new OfferCategory(EOfferWebsite.Allegro,
                    mainCategory));
            int page = controller.GetCurrentPageIndex();
            Assert.True(controller.OpenNextPage());
            Assert.IsTrue(controller.GetCurrentPageIndex() - 1 == page);
        }

        [Test]
        public void IterateToLastPageAndTryToGoNext()
        {
            controller.StartFetching(true,
                new OfferCategory(EOfferWebsite.Allegro,
                    mainCategory));
            int lastPage = controller.GetLastPageIndex();
            int page = -1;
            while (controller.OpenNextPage())
            {
                page = controller.GetCurrentPageIndex();
            }

            Assert.AreEqual(page, lastPage-1);
        }

        [Test]
        public void OpenAllPagesInCategory()
        {
            controller.StartFetching(true,
                new OfferCategory(EOfferWebsite.Allegro,
                    mainCategory));
            bool open = controller.OpenNextPage();
            while (open)
            {
                open = controller.OpenNextPage();
            }
        }

        [Test]
        public void FetchAllPages()
        {
            using IBaseOfferListController ctr = new HttpAllegroOfferListController();
            ctr.StartFetching(false,
                new OfferCategory(EOfferWebsite.Allegro,
                    1));
        }
        
        [Test]
        public void FetchAllPagesWithFilter()
        {
            using IBaseOfferListController ctr = new HttpAllegroOfferListController();
            ctr.StartFetching(false,
                new OfferCategory(EOfferWebsite.Allegro,
                    1),new List<WebsiteCategoriesFilterSearch>()
                {
                    new WebsiteCategoriesFilterSearch()
                    {
                        Argument = "offerTypeBuyNow",
                        Value = "1",
                        Id = 1,
                        SearchNumber = 1,
                        WebsiteCategoryId = (int)EOfferWebsite.Allegro
                    }, new WebsiteCategoriesFilterSearch()
                    {
                        Argument = "price_to",
                        Value = "500",
                        SearchNumber = 1,
                        Id = 2,
                        WebsiteCategoryId = (int) EOfferWebsite.Allegro
                    }
                });
        }

        [Test]
        public void FetchAllPagesWithWrongFilter()
        {
            using IBaseOfferListController ctr = new HttpAllegroOfferListController();
            OfferListControllerException ex =Assert.Throws<OfferListControllerException>(() => ctr.StartFetching(false,
                new OfferCategory(EOfferWebsite.Allegro,
                    1), new List<WebsiteCategoriesFilterSearch>()
                {
                    new WebsiteCategoriesFilterSearch()
                    {
                        Argument = "offerTypeBuyNow",
                        Value = "1",
                        Id = 1,
                        SearchNumber = 1,
                        WebsiteCategoryId = (int) EOfferWebsite.Allegro
                    },
                    new WebsiteCategoriesFilterSearch()
                    {
                        Argument = "price_to",
                        Value = "500",
                        SearchNumber = 1,
                        Id = 2,
                        WebsiteCategoryId = (int) EOfferWebsite.NoInfo
                    }
                }));
            Assert.IsNotNull(ex);
        }

        
        [Test]
        public void UpdateOfferDatabaseWithEmptyListDoNotThrow()
        {
            controller.StartFetching(true,
                new OfferCategory(EOfferWebsite.Allegro,
                    mainCategory));
            List<string> testList = new List<string>()
            {
                Guid.NewGuid().ToString()
            };

            controller.UpdateDatabaseWithOffers(testList);
        }

        [Test]
        public void UpdateOfferDatabaseWithInvalidString()
        {
            controller.StartFetching(true,
                new OfferCategory(EOfferWebsite.Allegro,
                    mainCategory));
            List<string> testList = new List<string>()
            {
                "{',''][[[]{{}",
            };
            DalException ex = Assert.Throws<DalException>(() => controller.UpdateDatabaseWithOffers(testList));
            Assert.That(ex, Is.Not.Null);
        }
        
    }
}