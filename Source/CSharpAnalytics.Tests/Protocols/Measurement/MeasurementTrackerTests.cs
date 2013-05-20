using System;
using System.Collections.Generic;
using System.Linq;
using CSharpAnalytics.Activities;
using CSharpAnalytics.Protocols.Measurement;
using CSharpAnalytics.Sessions;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Protocols.Measurement
{
    [TestClass]
    public class MeasurementTrackerTests
    {
        [TestMethod]
        public void MeasurementTracker_Track_Hits_Session()
        {
            var actual = new List<Uri>();
            var sessionManager = MeasurementTestHelpers.CreateSessionManager();
            var tracker = new MeasurementTracker(MeasurementTestHelpers.Configuration, sessionManager, MeasurementTestHelpers.CreateEnvironment(), actual.Add);

            tracker.Track(new MeasurementActivityEntry(new AppViewActivity("Testing")));

            Assert.AreEqual(1, sessionManager.Session.HitCount);
        }

        [TestMethod]
        public void MeasurementTracker_Track_Ends_Session()
        {
            var actual = new List<Uri>();
            var sessionManager = MeasurementTestHelpers.CreateSessionManager();
            var tracker = new MeasurementTracker(MeasurementTestHelpers.Configuration, sessionManager, MeasurementTestHelpers.CreateEnvironment(), actual.Add);

            tracker.Track(new MeasurementActivityEntry(new AppViewActivity("Testing")) { EndSession = true });

            Assert.AreEqual(SessionStatus.Ending, sessionManager.SessionStatus);
            StringAssert.Contains(actual.Last().OriginalString, "sc=end");
        }

        [TestMethod]
        public void MeasurementTracker_Track_Sends_Request()
        {
            var actual = new List<Uri>();
            var tracker = new MeasurementTracker(MeasurementTestHelpers.Configuration, MeasurementTestHelpers.CreateSessionManager(), MeasurementTestHelpers.CreateEnvironment(), actual.Add);

            tracker.Track(new MeasurementActivityEntry(new AppViewActivity("Testing")));

            Assert.AreEqual(1, actual.Count);
        }

        [TestMethod]
        public void MeasurementTracker_Track_Carries_Forward_Last_Transaction()
        {
            var actual = new List<Uri>();
            var tracker = new MeasurementTracker(MeasurementTestHelpers.Configuration, MeasurementTestHelpers.CreateSessionManager(), MeasurementTestHelpers.CreateEnvironment(), actual.Add);

            var transaction = new TransactionActivity { OrderId = "123", Currency = "GBP" };
            tracker.Track(new MeasurementActivityEntry(transaction));

            var transactionItem = new TransactionItemActivity("ABC", "Unit Test", 1.23m, 4);
            tracker.Track(new MeasurementActivityEntry(transactionItem));

            Assert.AreEqual(transaction, transactionItem.Transaction);
            StringAssert.Contains(actual.Last().OriginalString, "ti=123");
        }
    }
}