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
            var shortUri = new Uri("http://unittest.csharpanalytics.com");
            var message = BackgroundHttpClientRequester.CreateRequestMessage(shortUri);

            Assert.AreEqual(HttpMethod.Get, message.Method);
            Assert.AreEqual(shortUri.AbsoluteUri, message.RequestUri.AbsoluteUri);
            Assert.IsNull(message.Content);
        }

        [TestMethod]
        public void BackgroundHttpClientRequester_CreateMessage_Uses_Post_Over_2000_Bytes()
        {
            var baseUri = new Uri("http://unittest.csharpanalytics.com/a");
            var messageUri = new Uri(baseUri.AbsoluteUri + "?" + TestHelpers.RandomChars(2000));
            var encodedQuery = messageUri.GetComponents(UriComponents.Query, UriFormat.UriEscaped);

            var message = BackgroundHttpClientRequester.CreateRequestMessage(messageUri);

            Assert.AreEqual(HttpMethod.Post, message.Method);
            Assert.AreEqual(baseUri.AbsoluteUri, message.RequestUri.AbsoluteUri);
            Assert.AreEqual(encodedQuery, message.Content.ReadAsStringAsync().Result);
        }
    }
}