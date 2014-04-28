using System;
using System.Collections.Generic;
using CSharpAnalytics.Protocols.Measurement;
#if WINDOWS_STORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Protocols.Measurement
{
    [TestClass]
    public class MeasurementActivityExtensionTests
    {
        [TestMethod]
        public void ScreenViewExtension_Tracks_ScreenView()
        {
            var list = new List<Uri>();
            var client = new MeasurementAnalyticsClient();
            MeasurementTestHelpers.ConfigureForTest(client, list.Add);

            client.TrackScreenView("SomeScreenName");

            Assert.AreEqual(1, list.Count);
            StringAssert.Contains(list[0].OriginalString, "t=screenview");
        }

        [TestMethod]
        public void AppViewExtension_Tracks_ScreenView()
        {
            var list = new List<Uri>();
            var client = new MeasurementAnalyticsClient();
            MeasurementTestHelpers.ConfigureForTest(client, list.Add);

            client.TrackAppView("SomeScreenName");

            Assert.AreEqual(1, list.Count);
            StringAssert.Contains(list[0].OriginalString, "t=screenview");
        }

        [TestMethod]
        public void ContentViewExtension_Tracks_ContentView()
        {
            var url = new Uri("http://csharpanalytics.com/doc");
            const string title = "CSharpAnalytics docs";
            const string description = "Documentation for CSharpAnalaytics";
            const string path = "/docs";
            const string hostName = "docs.csharpanalytics.com";

            var list = new List<Uri>();
            var client = new MeasurementAnalyticsClient();
            MeasurementTestHelpers.ConfigureForTest(client, list.Add);

            client.TrackContentView(url, title, description, path, hostName);

            Assert.AreEqual(1, list.Count);
            var parameters = list[0].GetComponents(UriComponents.Query, UriFormat.Unescaped).Split('&');

            CollectionAssert.Contains(parameters, "t=pageview");
            CollectionAssert.Contains(parameters, "dl=" + url);
            CollectionAssert.Contains(parameters, "dt=" + title);
            CollectionAssert.Contains(parameters, "cd=" + description);
            CollectionAssert.Contains(parameters, "dp=" + path);
            CollectionAssert.Contains(parameters, "dh=" + hostName);
        }

        [TestMethod]
        public void TrackEventExtension_Tracks_Event()
        {
            const string action = "Some action";
            const string category = "Category Z";
            const string label = "I am a label";
            const int value = 55;

            var list = new List<Uri>();
            var client = new MeasurementAnalyticsClient();
            MeasurementTestHelpers.ConfigureForTest(client, list.Add);

            client.TrackEvent(action, category, label, value, true);

            Assert.AreEqual(1, list.Count);
            var parameters = list[0].GetComponents(UriComponents.Query, UriFormat.Unescaped).Split('&');

            CollectionAssert.Contains(parameters, "t=event");
            CollectionAssert.Contains(parameters, "ea=" + action);
            CollectionAssert.Contains(parameters, "ec=" + category); 
            CollectionAssert.Contains(parameters, "el=" + label);
            CollectionAssert.Contains(parameters, "ev=" + value);
            CollectionAssert.Contains(parameters, "ni=1");
        }

        [TestMethod]
        public void TrackExceptionExtension_Tracks_Exception()
        {
            const string description = "Some action";

            var list = new List<Uri>();
            var client = new MeasurementAnalyticsClient();
            MeasurementTestHelpers.ConfigureForTest(client, list.Add);

            client.TrackException(description, false);

            Assert.AreEqual(1, list.Count);
            var parameters = list[0].GetComponents(UriComponents.Query, UriFormat.Unescaped).Split('&');

            CollectionAssert.Contains(parameters, "t=exception");
            CollectionAssert.Contains(parameters, "exd=" + description);
            CollectionAssert.Contains(parameters, "exf=0");
        }

        [TestMethod]
        public void TrackSocialExtension_Tracks_Social()
        {
            const string action = "Poke";
            const string network = "FriendFace";
            const string target = "Clown";

            var list = new List<Uri>();
            var client = new MeasurementAnalyticsClient();
            MeasurementTestHelpers.ConfigureForTest(client, list.Add);

            client.TrackSocial(action, network, target);

            Assert.AreEqual(1, list.Count);
            var parameters = list[0].GetComponents(UriComponents.Query, UriFormat.Unescaped).Split('&');

            CollectionAssert.Contains(parameters, "t=social");
            CollectionAssert.Contains(parameters, "sa=" + action);
            CollectionAssert.Contains(parameters, "sn=" + network);
            CollectionAssert.Contains(parameters, "st=" + target);
        }

        [TestMethod]
        public void TrackTimedEventExtension_Tracks_TimedEvent()
        {
            const string category = "A category";
            const string variable = "Some variable";
            var time = TimeSpan.FromMilliseconds(12345);
            const string label = "Blue";

            var list = new List<Uri>();
            var client = new MeasurementAnalyticsClient();
            MeasurementTestHelpers.ConfigureForTest(client, list.Add);

            client.TrackTimedEvent(category, variable, time, label);

            Assert.AreEqual(1, list.Count);
            var parameters = list[0].GetComponents(UriComponents.Query, UriFormat.Unescaped).Split('&');

            CollectionAssert.Contains(parameters, "t=timing");
            CollectionAssert.Contains(parameters, "utc=" + category);
            CollectionAssert.Contains(parameters, "utv=" + variable);
            CollectionAssert.Contains(parameters, "utt=" + time.TotalMilliseconds);
            CollectionAssert.Contains(parameters, "utl=" + label);
        }

#if WINDOWS_STORE || WINDOWS_PHONE
        [TestMethod]
        public void ScreenViewExtension_Throws_If_AnalyticsClient_Null()
        {
            Assert.ThrowsException<ArgumentNullException>(() => MeasurementActivityExtensions.TrackScreenView(null, "test"));
        }

        [TestMethod]
        public void ScreenViewExtension_Throws_If_ScreenName_Null()
        {
            Assert.ThrowsException<ArgumentException>(() => new MeasurementAnalyticsClient().TrackScreenView(null));
        }

        [TestMethod]
        public void ScreenViewExtension_Throws_If_ScreenName_Blank()
        {
            Assert.ThrowsException<ArgumentException>(() => new MeasurementAnalyticsClient().TrackScreenView(""));
        }
#endif

#if NET45
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ScreenViewExtension_Throws_If_AnalyticsClient_Null()
        {
            MeasurementActivityExtensions.TrackScreenView(null, "test");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ScreenViewExtension_Throws_If_ScreenName_Null()
        {
            new MeasurementAnalyticsClient().TrackScreenView(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ScreenViewExtension_Throws_If_ScreenName_Blank()
        {
            new MeasurementAnalyticsClient().TrackScreenView("");
        }
#endif
    }
}