using System;
using System.Net.Http;
using CSharpAnalytics.Network;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Network
{
    [TestClass]
    public class BackgroundHttpClientRequesterTests
    {
        private const string BaseUriString = "http://localhost/csharpanalytics";

        [TestMethod]
        public void BackgroundHttpClientRequester_Calls_Preprocessor()
        {
            const int expected = 19743587;
            var actual = 0;
            Action<HttpRequestMessage> preprocessor = m => actual = expected;

            var requester = new BackgroundHttpClientRequester(preprocessor);
            requester.Start(TimeSpan.FromMilliseconds(10));
            requester.Add(new Uri(TestHelpers.Utm));
            
            TestHelpers.WaitForQueueToEmpty(requester);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void BackgroundHttpClientRequester_CreateMessage_Uses_Get_Under_2000_Bytes()
        {
            var shortUri = new Uri(BaseUriString);
            var request = BackgroundHttpClientRequester.CreateRequest(shortUri);

            Assert.AreEqual(HttpMethod.Get, request.Method);
            Assert.AreEqual(shortUri.AbsoluteUri, request.RequestUri.AbsoluteUri);
            Assert.IsNull(request.Content);
        }

        [TestMethod]
        public void BackgroundHttpClientRequester_CreateRequest_Uses_Get_When_2000_Bytes()
        {
            var longUri = new Uri(BaseUriString + "?" + TestHelpers.RandomChars(2000));
            var borderlineUri = new Uri(longUri.AbsoluteUri.Substring(0, 2000));
                
            var request = BackgroundHttpClientRequester.CreateRequest(borderlineUri);

            Assert.AreEqual(HttpMethod.Get, request.Method);
            Assert.AreEqual(borderlineUri.AbsoluteUri, request.RequestUri.AbsoluteUri);
            Assert.IsNull(request.Content);
        }

        [TestMethod]
        public void BackgroundHttpClientRequester_CreateMessage_Uses_Post_Over_2000_Bytes()
        {
            var baseUri = new Uri(BaseUriString);
            var longUri = new Uri(baseUri.AbsoluteUri + "?" + TestHelpers.RandomChars(2000));
            var encodedQuery = longUri.GetComponents(UriComponents.Query, UriFormat.UriEscaped);

            var request = BackgroundHttpClientRequester.CreateRequest(longUri);

            Assert.AreEqual(HttpMethod.Post, request.Method);
            Assert.AreEqual(baseUri.AbsoluteUri, request.RequestUri.AbsoluteUri);
            Assert.AreEqual(encodedQuery, request.Content.ReadAsStringAsync().Result);
        }
    }
}