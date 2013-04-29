using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CSharpAnalytics.Network;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Network
{
    [TestClass]
    public class BackgroundHttpRequesterTests
    {
        private const string Utm = "http://www.google-analytics.com/__utm.gif";

        [TestMethod]
        public void BackgroundHttpRequester_Calls_Preprocessor()
        {
            const int expected = 19743587;
            var actual = 0;
            Action<HttpRequestMessage> preprocessor = m => actual = expected;

            var requester = new BackgroundHttpRequester(preprocessor);
            requester.Start(TimeSpan.FromMilliseconds(10));
            requester.Add(new Uri(Utm));
            
            WaitForQueueToEmpty(requester);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void BackgroundHttpRequester_Start_Uses_Previous_List()
        {
            var expectedList = CreateRequestList(4);
            var actualList = new List<Uri>();
            Action<HttpRequestMessage> preprocessor = m => actualList.Add(m.RequestUri);

            var requester = new BackgroundHttpRequester(preprocessor);
            requester.Start(TimeSpan.FromMilliseconds(10), expectedList);
            
            WaitForQueueToEmpty(requester);
            CollectionAssert.AreEqual(expectedList, actualList);
        }

        [TestMethod]
        public void BackgroundHttpRequester_Start_Uses_Previous_List_First()
        {
            var expectedList = CreateRequestList(10);
            var actualList = new List<Uri>();
            Action<HttpRequestMessage> preprocessor = m => actualList.Add(m.RequestUri);

            var requester = new BackgroundHttpRequester(preprocessor);
            requester.Start(TimeSpan.FromMilliseconds(10), expectedList.Take(5));
            foreach (var uri in expectedList.Skip(5))
                requester.Add(uri);
            
            WaitForQueueToEmpty(requester);
            CollectionAssert.AreEqual(expectedList, actualList);
        }

        [TestMethod]
        public void BackgroundHttpRequester_StopAsync_Stops_Requesting()
        {
            var preprocessorCalled = false;
            Action<HttpRequestMessage> preprocessor = m => preprocessorCalled = true;

            var requester = new BackgroundHttpRequester(preprocessor);
            requester.Start(TimeSpan.FromMilliseconds(10));
            requester.StopAsync().Wait();
            foreach (var uri in CreateRequestList(3))
                requester.Add(uri);

            Assert.IsFalse(preprocessorCalled);
            Assert.AreEqual(3, requester.QueueCount);
        }

        [TestMethod]
        public void BackgroundHttpRequester_CreateMessage_Uses_Get_Under_2000_Bytes()
        {
            var shortUri = new Uri("http://unittest.csharpanalytics.com");
            var message = BackgroundHttpRequester.CreateRequestMessage(shortUri);

            Assert.AreEqual(HttpMethod.Get, message.Method);
            Assert.AreEqual(shortUri.AbsoluteUri, message.RequestUri.AbsoluteUri);
            Assert.IsNull(message.Content);
        }

        [TestMethod]
        public void BackgroundHttpRequester_CreateMessage_Uses_Post_Over_2000_Bytes()
        {
            var baseUri = new Uri("http://unittest.csharpanalytics.com/a");
            var messageUri = new Uri(baseUri.AbsoluteUri + "?" + RandomChars(2000));
            var encodedQuery = messageUri.GetComponents(UriComponents.Query, UriFormat.UriEscaped);

            var message = BackgroundHttpRequester.CreateRequestMessage(messageUri);

            Assert.AreEqual(HttpMethod.Post, message.Method);
            Assert.AreEqual(baseUri.AbsoluteUri, message.RequestUri.AbsoluteUri);
            Assert.AreEqual(encodedQuery, message.Content.ReadAsStringAsync().Result);
        }

        private static readonly Random random = new Random();
        private const int AsciiLow = 32;
        private const int AsciiHigh = 127;

        private static string RandomChars(int length)
        {
            var chars = new char[length];
            for (var i = 0; i < length; i++)
                chars[i] = (char)random.Next(AsciiLow, AsciiHigh);

            return new string(chars);
        }

        private static void WaitForQueueToEmpty(BackgroundHttpRequester requester)
        {
            while (requester.QueueCount != 0)
                Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();
        }

        private static List<Uri> CreateRequestList(int count)
        {
            return new List<Uri>(Enumerable.Range(1, count).Select(i => new Uri(Utm + "?" + i)));
        }
    }
}