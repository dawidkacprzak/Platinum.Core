using NUnit.Framework;
using Platinum.Core.DatabaseIntegration;

namespace Platinum.Tests.Unit
{
    public class DalTest
    {
        [TestCase("test", "098f6bcd4621d373cade4e832627b4f6")]
        [TestCase("xxx", "f561aaf6ef0bf14d4208bb46a4ccb3ad")]
        [TestCase("", "d41d8cd98f00b204e9800998ecf8427e")]
        [TestCase("https://offer.test.xyz", "813fb2bbc24e7c74d4004814924312ba")]
        public void CreateMd5Success(string input,string output)
        {
            Assert.AreEqual(output, Dal.CreateMd5(input).ToLower());
        }
    }
}