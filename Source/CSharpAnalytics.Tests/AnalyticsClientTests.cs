using System;
using CSharpAnalytics.Activities;
using CSharpAnalytics.Protocols.Urchin;
using CSharpAnalytics.Sessions;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test
{
    [TestClass]
    public class AnalyticsClientTests
    {
        [TestMethod]
        public void AnalyticsClient_Track_Sends_Uri_To_Sender()
        {
            Uri capturedUri = null;
            var client = CreateSampleClient(uri => capturedUri = uri); 
            
            client.Track(new PageViewActivity("FindTitle", "FindPage"));

            Assert.IsTrue(capturedUri.OriginalString.Contains("UA-1234-5"), "Configuration AccountId missing");
            Assert.IsTrue(capturedUri.OriginalString.Contains("FindHostName"), "Configuration ApplicationName missing");
            Assert.IsTrue(capturedUri.OriginalString.Contains("en-gb"), "Environment LanguageCode missing");
            Assert.IsTrue(capturedUri.OriginalString.Contains("FindTitle"), "PageViewActivity Title missing");
            Assert.IsTrue(capturedUri.OriginalString.Contains("FindPage"), "PageViewActivity Page missing");
        }

        [TestMethod]
        public void AnalyticsClient_Track_Updates_Referrer()
        {
            Uri capturedUri = null;
            var client = CreateSampleClient(uri => capturedUri = uri);

            client.Track(new PageViewActivity("FindTitle", "ref123"));
            client.Track(new PageViewActivity("FindTitle", "destination"));

            Assert.IsTrue(capturedUri.OriginalString.Contains("ref123"), "Referrer missing");
        }

        [TestMethod]
        public void AnalyticsClient_Track_Ends_Auto_Timed_Events()
        {
            var client = CreateSampleClient(_ => { });

            var timedEvent = new AutoTimedEventActivity("category", "variable");
            Assert.IsNull(timedEvent.EndedAt);

            client.Track(timedEvent);
            Assert.IsNotNull(timedEvent.EndedAt);
            Assert.IsTrue(timedEvent.EndedAt <= DateTimeOffset.Now);
        }

        private static AnalyticsClient CreateSampleClient(Action<Uri> sender)
        {
            var configuration = new UrchinConfiguration("UA-1234-5", "FindHostName");
            var sessionManager = new SessionManager(TimeSpan.FromDays(1), null);
            var environment = new Environment("en-gb");
            return new AnalyticsClient(configuration, sessionManager, environment, sender);
        }
    }
}