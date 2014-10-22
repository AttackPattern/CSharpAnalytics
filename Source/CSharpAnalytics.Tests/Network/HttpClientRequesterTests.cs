using System;
using System.Net.Http;
using CSharpAnalytics.Network;
#if WINDOWS_STORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Network
{
    [TestClass]
    public class HttpClientRequesterTests
    {
        private const string BaseUriString = "http://localhost/csharpanalytics";

        [TestMethod]
        public void HttpClientRequester_CreateMessage_Uses_Post()
        {
            var baseUri = new Uri(BaseUriString);
            var longUri = new Uri(baseUri.AbsoluteUri + "?" + TestHelpers.RandomChars(100));
            var encodedQuery = longUri.GetComponents(UriComponents.Query, UriFormat.UriEscaped);

            var request = HttpClientRequester.CreateRequest(longUri);

            Assert.AreEqual(HttpMethod.Post, request.Method);
            Assert.AreEqual(baseUri.AbsoluteUri, request.RequestUri.AbsoluteUri);
            Assert.AreEqual(encodedQuery, request.Content.ReadAsStringAsync().Result);
        }
    }
}