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
    public class BackgroundHttpRequesterTests
    {
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
    }
}