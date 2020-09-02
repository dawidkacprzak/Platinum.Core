using System.Reflection;
using HtmlAgilityPack;
using NUnit.Framework;
using Platinum.Core.OfferListController;

namespace Platinum.Tests.Unit
{
    public class AllegroOfferListControllerTest
    {
        [Test]
        public void ValidatePaginationContainerTestEmptyCollection()
        {
            HtmlNodeCollection collection = new HtmlNodeCollection(null);
            object obj = typeof(BrowserAllegroOfferListController).GetMethod(
                    "ValidatePaginationContainer",
                    BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(
                    new BrowserAllegroOfferListController()
                    , new object[]{collection});
             
            Assert.NotNull(obj);
            Assert.DoesNotThrow(() =>
            {
                bool testParse = (bool) obj;
                Assert.False(testParse);
            });
        }
        
        
        [Test]
        public void ValidatePaginationContainerTestNullCollection()
        {
            object obj = typeof(BrowserAllegroOfferListController).GetMethod(
                    "ValidatePaginationContainer",
                    BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(
                    new BrowserAllegroOfferListController()
                    , new object[]{null});
             
            Assert.NotNull(obj);
            Assert.DoesNotThrow(() =>
            {
                bool testParse = (bool) obj;
                Assert.False(testParse);
            });
        }
        
        [Test]
        public void ValidatePaginationContainerTestNotEmptyCollection()
        {
            HtmlNodeCollection collection = new HtmlNodeCollection(null) {HtmlNode.CreateNode("<h1>")};
            object obj = typeof(BrowserAllegroOfferListController).GetMethod(
                    "ValidatePaginationContainer",
                    BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(
                    new BrowserAllegroOfferListController()
                    , new object[]{collection});
             
            Assert.NotNull(obj);
            Assert.DoesNotThrow(() =>
            {
                bool testParse = (bool) obj;
                Assert.True(testParse);
            });
        }
    }
}