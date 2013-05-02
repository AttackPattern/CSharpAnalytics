using System;
using System.Collections.Generic;
using System.Linq;
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
        public void BackgroundHttpClientRequester_Start_Uses_Previous_List()
        {
            var expectedList = TestHelpers.CreateRequestList(4);
            var actualList = new List<Uri>();
            Action<HttpRequestMessage> preprocessor = m => actualList.Add(m.RequestUri);

            var requester = new BackgroundHttpClientRequester(preprocessor);
            requester.Start(TimeSpan.FromMilliseconds(10), expectedList);

            TestHelpers.WaitForQueueToEmpty(requester);
            CollectionAssert.AreEqual(expectedList, actualList);
        }

        [TestMethod]
        public void BackgroundHttpClientRequester_Start_Uses_Previous_List_First()
        {
            var expectedList = TestHelpers.CreateRequestList(10);
            var actualList = new List<Uri>();
            Action<HttpRequestMessage> preprocessor = m => actualList.Add(m.RequestUri);

            var requester = new BackgroundHttpClientRequester(preprocessor);
            requester.Start(TimeSpan.FromMilliseconds(10), expectedList.Take(5));
            foreach (var uri in expectedList.Skip(5))
                requester.Add(uri);

            TestHelpers.WaitForQueueToEmpty(requester);
            CollectionAssert.AreEqual(expectedList, actualList);
        }

        [TestMethod]
        public void BackgroundHttpClientRequester_StopAsync_Stops_Requesting()
        {
            var preprocessorCalled = false;
            Action<HttpRequestMessage> preprocessor = m => preprocessorCalled = true;

            var requester = new BackgroundHttpClientRequester(preprocessor);
            requester.Start(TimeSpan.FromMilliseconds(10));
            requester.StopAsync().Wait();
            foreach (var uri in TestHelpers.CreateRequestList(3))
                requester.Add(uri);

            Assert.IsFalse(preprocessorCalled);
            Assert.AreEqual(3, requester.QueueCount);
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