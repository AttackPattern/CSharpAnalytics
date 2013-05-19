using System;
using System.Net;
using CSharpAnalytics.Network;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Network
{
    [TestClass]
    public class BackgroundHttpWebRequesterTests
    {
        private const string BaseUriString = "http://localhost/csharpanalytics";

        [TestMethod]
        public void BackgroundHttpWebRequester_Calls_Preprocessor()
        {
            const int expected = 19743587;
            var actual = 0;
            Action<HttpWebRequest> preprocessor = m => actual = expected;

            var requester = new BackgroundHttpWebRequester(preprocessor);
            requester.Start(TimeSpan.FromMilliseconds(10));
            requester.Add(new Uri(TestHelpers.Utm));
            
            TestHelpers.WaitForQueueToEmpty(requester);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void BackgroundHttpWebRequester_CreateRequest_Uses_Get_Under_2000_Bytes()
        {
            var shortUri = new Uri(BaseUriString);
            var request = BackgroundHttpWebRequester.CreateRequest(shortUri);

            Assert.AreEqual("GET", request.Method);
            Assert.AreEqual(shortUri.AbsoluteUri, request.RequestUri.AbsoluteUri);
            Assert.AreEqual(-1, request.ContentLength);
        }

        [TestMethod]
        public void BackgroundHttpWebRequester_CreateRequest_Uses_Get_When_2000_Bytes()
        {
            var longUri = new Uri(BaseUriString + "?" + TestHelpers.RandomChars(2000));
            var borderlineUri = new Uri(longUri.AbsoluteUri.Substring(0, 2000));

            var request = BackgroundHttpWebRequester.CreateRequest(borderlineUri);

            Assert.AreEqual("GET", request.Method);
            Assert.AreEqual(borderlineUri.AbsoluteUri, request.RequestUri.AbsoluteUri);
            Assert.AreEqual(-1, request.ContentLength);
        }

        [TestMethod]
        public void BackgroundHttpWebRequester_CreateMessage_Uses_Post_Over_2000_Bytes()
        {
            var baseUri = new Uri(BaseUriString);
            var longUri = new Uri(baseUri.AbsoluteUri + "?" + TestHelpers.RandomChars(2000));

            var request = BackgroundHttpWebRequester.CreateRequest(longUri);

            Assert.AreEqual("POST", request.Method);
            Assert.AreEqual(baseUri.AbsoluteUri, request.RequestUri.AbsoluteUri);
            Assert.IsTrue(request.ContentLength > 100);
        }
    }
}