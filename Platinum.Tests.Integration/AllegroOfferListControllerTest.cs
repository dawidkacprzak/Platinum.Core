using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Model;
using Platinum.Core.OfferListController;
using Platinum.Core.Types;
using Platinum.Core.Types.Exceptions;

namespace Platinum.Tests.Integration
{
    [TestFixture]
    public class AllegroOfferListControllerTest
    {
        private IBaseOfferListController _controller;
        private int mainCategory = 1;
        [TearDown]
        public void TearDown()
        {
            _controller.Dispose();
            Thread.Sleep(2000);
        }

        [SetUp]
        public void SetController()
        {
            _controller = new AllegroOfferListController();
        }

        [Test]
        public void FetchingTakesGoodAmountOfMaxPageIndex()
        {
            _controller.StartFetching(true,
                new OfferCategory(OfferWebsite.Allegro,
                    mainCategory));
        
            int maxPages = _controller.GetLastPageIndex();
            Assert.NotZero(maxPages);
            Assert.AreEqual(maxPages, _controller.GetLastPageIndex());
        }


        [Test]
        public void OpenNextPageWithoutStartFetching()
        {
            using IBaseOfferListController controller = new AllegroOfferListController();
            OfferListControllerException ex =
                Assert.Throws<OfferListControllerException>(() => controller.OpenNextPage());
            Assert.That(ex, Is.Not.Null);
            controller.Dispose();
        }

        [Test]
        public void OpenNextPageWithStartFetching()
        {
            _controller.StartFetching(true,
                new OfferCategory(OfferWebsite.Allegro,
                    mainCategory));
            int page = _controller.GetCurrentPageIndex();
            Assert.True(_controller.OpenNextPage());
            Assert.True(_controller.GetCurrentPageIndex() == page + 1);
        }

        [Test]
        public void GetAllOffersSuccess()
        {
            _controller.StartFetching(true,
                new OfferCategory(OfferWebsite.Allegro,
                    mainCategory));
            List<string> offers = _controller.GetAllOfferLinks().ToList();
            Assert.True(offers.Any());
            Assert.True(offers.Count(x => !x.Contains("http")) == 0);
        }

        [Test]
        public void FetchFailAllegroLink()
        {
            using IBaseOfferListController ctr = new AllegroOfferListController();
            OfferListControllerException ex = Assert.Throws<OfferListControllerException>(
                () => ctr.StartFetching(false, new OfferCategory(OfferWebsite.Allegro, "testfaillink")));

            Assert.That(ex, Is.Not.Null);
            ctr.Dispose();
            _controller.Dispose();
        }

        [Test]
        public void IndexPageFailAllegroLink()
        {
            using IBaseOfferListController ctr = new AllegroOfferListController();

            OfferListControllerException e =
                Assert.Throws<OfferListControllerException>(
                    () => ctr.StartFetching(false, new OfferCategory(OfferWebsite.Allegro, "/testfaillink")));
            OfferListControllerException ex =
                Assert.Throws<OfferListControllerException>(() => ctr.GetCurrentPageIndex());
            Assert.That(ex, Is.Not.Null);
            ctr.Dispose();
            _controller.Dispose();
        }

        [Test]
        public void IndexPageFailAllegroLinkNoCategoryWithPagination()
        {
            using IBaseOfferListController ctr = new AllegroOfferListController();
            Assert.Throws<OfferListControllerException>(
                () => ctr.StartFetching(false,
                    new OfferCategory(OfferWebsite.Allegro, "uzytkownik/KOLEKCJONER-PL--/oceny")));
            OfferListControllerException ex =
                Assert.Throws<OfferListControllerException>(() => ctr.GetCurrentPageIndex());
            Assert.That(ex, Is.Not.Null);
            ctr.Dispose();
            _controller.Dispose();
        }

        [Test]
        public void IndexPageFailAllegroLinkNoCategoryWithPaginationLastIndex()
        {
            using IBaseOfferListController ctr = new AllegroOfferListController();
            Assert.Throws<OfferListControllerException>(
                () => ctr.StartFetching(false,
                    new OfferCategory(OfferWebsite.Allegro, "uzytkownik/KOLEKCJONER-PL--/oceny")));
            OfferListControllerException ex =
                Assert.Throws<OfferListControllerException>(() => ctr.GetLastPageIndex());
            Assert.That(ex, Is.Not.Null);
            ctr.Dispose();
            _controller.Dispose();
        }

        [Test]
        public void CheckLastPageIndexIsGreater()
        {
            _controller.StartFetching(true,
                new OfferCategory(OfferWebsite.Allegro,
                    mainCategory));
            int lastPage = _controller.GetLastPageIndex();
            int currentPage = _controller.GetCurrentPageIndex();
            Assert.True(lastPage >= currentPage);
        }

        [Test]
        public void CurrentPageIndexSuccess()
        {
            _controller.StartFetching(true,
                new OfferCategory(OfferWebsite.Allegro,
                    mainCategory));
            int page = _controller.GetCurrentPageIndex();
            Assert.True(_controller.OpenNextPage());
            Assert.IsTrue(_controller.GetCurrentPageIndex() - 1 == page);
        }

        [Test]
        public void IterateToLastPageAndTryToGoNext()
        {
            _controller.StartFetching(true,
                new OfferCategory(OfferWebsite.Allegro,
                    mainCategory));
            int lastPage = _controller.GetLastPageIndex();
            int page = -1;
            while (_controller.OpenNextPage())
            {
                page = _controller.GetCurrentPageIndex();
            }

            Assert.AreEqual(page, lastPage);
        }

        [Test]
        public void OpenAllPagesInCategory()
        {
            _controller.StartFetching(true,
                new OfferCategory(OfferWebsite.Allegro,
                    mainCategory));
            bool open = _controller.OpenNextPage();
            while (open)
            {
                open = _controller.OpenNextPage();
            }
        }

        [Test]
        public void FetchAllPages()
        {
            using IBaseOfferListController ctr = new AllegroOfferListController();
            ctr.StartFetching(false,
                new OfferCategory(OfferWebsite.Allegro,
                    1));
        }
        
        [Test]
        public void FetchAllPagesWithFilter()
        {
            using IBaseOfferListController ctr = new AllegroOfferListController();
            ctr.StartFetching(false,
                new OfferCategory(OfferWebsite.Allegro,
                    1),new List<WebsiteCategoriesFilterSearch>()
                {
                    new WebsiteCategoriesFilterSearch()
                    {
                        Argument = "offerTypeBuyNow",
                        Value = "1",
                        Id = 1,
                        SearchNumber = 1,
                        WebsiteCategoryId = (int)OfferWebsite.Allegro
                    }, new WebsiteCategoriesFilterSearch()
                    {
                        Argument = "price_to",
                        Value = "500",
                        SearchNumber = 1,
                        Id = 2,
                        WebsiteCategoryId = (int) OfferWebsite.Allegro
                    }
                });
        }

        [Test]
        public void FetchAllPagesWithWrongFilter()
        {
            using IBaseOfferListController ctr = new AllegroOfferListController();
            OfferListControllerException ex =Assert.Throws<OfferListControllerException>(() => ctr.StartFetching(false,
                new OfferCategory(OfferWebsite.Allegro,
                    1), new List<WebsiteCategoriesFilterSearch>()
                {
                    new WebsiteCategoriesFilterSearch()
                    {
                        Argument = "offerTypeBuyNow",
                        Value = "1",
                        Id = 1,
                        SearchNumber = 1,
                        WebsiteCategoryId = (int) OfferWebsite.Allegro
                    },
                    new WebsiteCategoriesFilterSearch()
                    {
                        Argument = "price_to",
                        Value = "500",
                        SearchNumber = 1,
                        Id = 2,
                        WebsiteCategoryId = (int) OfferWebsite.NoInfo
                    }
                }));
            Assert.IsNotNull(ex);
        }

        
        [Test]
        public void UpdateOfferDatabaseWithEmptyListDoNotThrow()
        {
            _controller.StartFetching(true,
                new OfferCategory(OfferWebsite.Allegro,
                    mainCategory));
            List<string> testList = new List<string>()
            {
                Guid.NewGuid().ToString()
            };

            _controller.UpdateDatabaseWithOffers(testList);
        }

        [Test]
        public void TestOpenNextPageAsTrueDoNotThrow()
        {
             Mock<IBaseOfferListController> offer = new Mock<IBaseOfferListController>();
        }

        [Test]
        public void UpdateOfferDatabaseWithInvalidString()
        {
            _controller.StartFetching(true,
                new OfferCategory(OfferWebsite.Allegro,
                    mainCategory));
            List<string> testList = new List<string>()
            {
                "{',''][[[]{{}",
            };
            DalException ex = Assert.Throws<DalException>(() => _controller.UpdateDatabaseWithOffers(testList));
            Assert.That(ex, Is.Not.Null);
        }
        
    }
}