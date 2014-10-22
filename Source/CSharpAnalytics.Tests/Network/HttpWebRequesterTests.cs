using System;
using CSharpAnalytics.Network;
#if WINDOWS_STORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Network
{
    [TestClass]
    public class HttpWebRequesterTests
    {
        private const string BaseUriString = "http://localhost/csharpanalytics";

        [TestMethod]
        public void HttpWebRequester_CreateMessage_Uses_Post()
        {
            var baseUri = new Uri(BaseUriString);
            var longUri = new Uri(baseUri.AbsoluteUri + "?" + TestHelpers.RandomChars(110));

            var request = HttpWebRequester.CreateRequest(longUri, false);

            Assert.AreEqual("POST", request.Method);
            Assert.AreEqual(baseUri.AbsoluteUri, request.RequestUri.AbsoluteUri);
            Assert.IsTrue(request.ContentLength > 100);
        }
    }
}