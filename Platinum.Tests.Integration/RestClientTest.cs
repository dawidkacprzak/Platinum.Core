using System.Net;
using System.Threading;
using NUnit.Framework;
using Platinum.Core.Types;
using Platinum.Core.Types.Exceptions;
using RestSharp;
using RestClient = Platinum.Core.ApiIntegration.RestClient;

namespace Platinum.Tests.Integration
{
    [TestFixture]
    public class RestClientTest
    {
        [TearDown]
        public void TearDown()
        {
            Thread.Sleep(1000);
        }

        [Test]
        public void CheckRestClientSuccessGet()
        {
            IRest client = new RestClient();
            IRestResponse response = client.Get(new RestRequest("https://google.pl"));

            Assert.AreEqual(response.StatusCode, HttpStatusCode.OK);
            Assert.True(response.IsSuccessful);
        }

        [TestCase(@"https://godogle.xyz")]
        [TestCase(@"https://goddogle.xyz")]
        [TestCase(@"htt//godog3le.xyz")]
        public void CheckRestClientFailGet(string url)
        {
            IRest client = new RestClient();
            RequestException exception = Assert.Throws<RequestException>(() =>  client.Get(new RestRequest(url)));
            Assert.That(exception.Message, Is.Not.Null.Or.Empty);
        }
    }
}