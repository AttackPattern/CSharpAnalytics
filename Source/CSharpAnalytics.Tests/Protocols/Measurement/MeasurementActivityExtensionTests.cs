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