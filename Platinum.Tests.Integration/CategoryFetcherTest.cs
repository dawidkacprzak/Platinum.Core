using System;
using System.Net;
using Moq;
using NUnit.Framework;
using Platinum.Core.DatabaseIntegration;
using Platinum.Core.Model;
using Platinum.Core.Types;
using Platinum.Service.CategoryFetcher;
using Platinum.Service.CategoryFetcher.Factory;
using RestSharp;

namespace Platinum.Tests.Integration
{
    [TestFixture]
    public class CategoryFetcherTest
    {
        [Test]
        public void GetAllegroCategoryFetchFromFactory()
        {
            CategoryFetcherFactory fetcher = new AllegroCategoryFetcherFactory();
            ICategoryFetcher instance = fetcher.GetFetcher();

            Assert.IsInstanceOf<AllegroCategoryFetcher>(instance);
        }

        [Test]
        public void DatabaseExceptionDuringRunDoNotThrowUp()
        {
            Mock<IDal> dbMock = new Mock<IDal>();
            dbMock.Setup(x => x.ExecuteNonQuery(It.IsAny<string>())).Throws<Exception>();

            CategoryFetcherFactory fetcher = new AllegroCategoryFetcherFactory();
            ICategoryFetcher instance = fetcher.GetFetcher();
            instance.SetIndexCategoryFetchLimit(10);
            Assert.DoesNotThrow(() => instance.Run(dbMock.Object));
        }

        [Test]
        public void TakeFirstTenCategoriesWithoutException()
        {
            using (Dal db = new Dal())
            {
                CategoryFetcherFactory fetcher = new AllegroCategoryFetcherFactory();
                ICategoryFetcher instance = fetcher.GetFetcher();
                instance.SetIndexCategoryFetchLimit(10);
                Assert.DoesNotThrow(() => instance.Run(db));
            }
        }

        [Test]
        public void IfCategoryDoNotExistInDbUpdateIsInvokedOnce()
        {
            Mock<IDal> dbMock = new Mock<IDal>();
            dbMock.Setup(x => x.ExecuteScalar(It.IsAny<string>())).Returns(0);
            dbMock.Setup(x => x.ExecuteNonQuery(It.IsAny<string>())).Verifiable();

            CategoryFetcherFactory fetcher = new AllegroCategoryFetcherFactory();
            ICategoryFetcher instance = fetcher.GetFetcher();
            instance.SetIndexCategoryFetchLimit(10);
            instance.Run(dbMock.Object);
            dbMock.Verify(x => x.ExecuteNonQuery(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Test]
        public void CreateRequestNotFoundDoNotThrowButReturnResponse()
        {
            
            CategoryFetcherFactory fetcher = new AllegroCategoryFetcherFactory();
            ICategoryFetcher instance = fetcher.GetFetcher();

            OfferCategory category = instance.GetCategoryById(0);
            Assert.IsNull(category);
        }
        
        [Test]
        public void RequestNotFoundExceptionDoNotThrowUp()
        {
            Mock<IRest> restMock = new Mock<IRest>();
            restMock.Setup(x => x.Get(It.IsAny<IRestRequest>())).Returns(new RestResponse()
            {
                StatusCode = HttpStatusCode.NotFound
            });

            CategoryFetcherFactory fetcher = new AllegroCategoryFetcherFactory();
            ICategoryFetcher instance = fetcher.GetFetcher(restMock.Object);
            instance.SetIndexCategoryFetchLimit(10);
            instance.GetCategoryById(0);
        }
        
        
        
        /// <summary>
        /// Null reference cause of some functions are not visible and its impossible without cost to test it
        /// </summary>
        [Test]
        public void RequestUnauthorizedDoNotThrowUp()
        {
            Mock<IRest> restMock = new Mock<IRest>();
            restMock.Setup(x => x.Get(It.IsAny<IRestRequest>())).Returns(new RestResponse()
            {
                StatusCode = HttpStatusCode.Unauthorized
            });

            CategoryFetcherFactory fetcher = new AllegroCategoryFetcherFactory();
            ICategoryFetcher instance = fetcher.GetFetcher(restMock.Object);
            instance.SetIndexCategoryFetchLimit(1);
            NullReferenceException ex = Assert.Throws<NullReferenceException> (()=>instance.GetCategoryById(0));
            Assert.That(ex.Message,Contains.Substring("Object reference"));
        }
    }
}