using System.Globalization;
using CSharpAnalytics.Sessions;
using System;
using System.Threading.Tasks;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Sessions
{
    [TestClass]
    public class SessionManagerTests
    {
        [TestMethod]
        public void SessionManager_Can_Be_Created_From_State()
        {
            var timeout = TimeSpan.FromMinutes(5);
            var state = CreateSampleState();

            var sessionManager = new SessionManager(state);

            Assert.AreEqual(state.PreviousSessionStartedAt, sessionManager.PreviousSessionStartedAt);
            Assert.AreEqual(state.Referrer, sessionManager.Referrer);

            Assert.AreEqual(state.FirstVisitAt, sessionManager.Visitor.FirstVisitAt);
            Assert.AreEqual(state.VisitorId, sessionManager.Visitor.ClientId);
            
            Assert.AreEqual(state.HitId, sessionManager.Session.HitId);
            Assert.AreEqual(state.SessionHitCount, sessionManager.Session.HitCount);
            Assert.AreEqual(state.SessionNumber, sessionManager.Session.Number);
            Assert.AreEqual(state.SessionStartedAt, sessionManager.Session.StartedAt);
        }

        [TestMethod]
        public void SessionManager_Created_From_Null_State_Is_Fresh()
        {
            var sessionManager = new SessionManager(null);

            Assert.IsNull(sessionManager.Referrer);
            Assert.IsNotNull(sessionManager.Visitor);
            Assert.IsNotNull(sessionManager.Session);
            Assert.AreEqual(0, sessionManager.Session.HitCount);
            Assert.IsTrue(sessionManager.PreviousSessionStartedAt <= DateTimeOffset.Now);
        }

        [TestMethod]
        public void SessionManager_Created_From_State_Provides_Same_State()
        {
            var expected = CreateSampleState();

            var sessionManager = new SessionManager(expected);

            var actual = sessionManager.GetState();

            Assert.AreEqual(expected.FirstVisitAt, actual.FirstVisitAt);
            Assert.AreEqual(expected.HitId, actual.HitId);
            Assert.AreEqual(expected.LastActivityAt, actual.LastActivityAt);
            Assert.AreEqual(expected.PreviousSessionStartedAt, actual.PreviousSessionStartedAt);
            Assert.AreEqual(expected.Referrer, actual.Referrer);
            Assert.AreEqual(expected.SessionHitCount, actual.SessionHitCount);
            Assert.AreEqual(expected.SessionNumber, actual.SessionNumber);
            Assert.AreEqual(expected.SessionStartedAt, actual.SessionStartedAt);
        }

        [TestMethod]
        public void SessionManager_Referrer_Property_Can_Be_Set()
        {
            var referrer = new Uri("http://stickertales.com");
            var sessionManager = new SessionManager(null) { Referrer = referrer };

            Assert.AreEqual(referrer, sessionManager.Referrer);
        }

        [TestMethod]
        public void SessionManager_Creates_New_Session_When_Requested()
        {
            var sessionManager = new SessionManager(null);

            Assert.AreEqual(1, sessionManager.Session.Number);

            var starting = DateTimeOffset.Now;

            sessionManager.StartNewSession();
            Assert.AreEqual(2, sessionManager.Session.Number);
            Assert.IsTrue(sessionManager.Session.StartedAt >= starting, "Session StartedAt too early");
            Assert.IsTrue(sessionManager.Session.StartedAt <= DateTimeOffset.Now, "Session StartedAt too late");
        }


        [TestMethod]
        public void SessionManager_SessionStatus_Is_Starting_For_New_Session()
        {
            var sessionManager = new SessionManager(null);

            Assert.AreEqual(SessionStatus.Starting, sessionManager.SessionStatus);
        }

        [TestMethod]
        public void SessionManager_SessionStatus_Is_Active_After_First_Hit()
        {
            var sessionManager = new SessionManager(null);
            sessionManager.Hit();

            Assert.AreEqual(SessionStatus.Active, sessionManager.SessionStatus);
        }

        [TestMethod]
        public void SessionManager_SessionStatus_Is_Ending_After_End()
        {
            var sessionManager = new SessionManager(null);
            sessionManager.Hit();
            sessionManager.End();

            Assert.AreEqual(SessionStatus.Ending, sessionManager.SessionStatus);
        }

        [TestMethod]
        public void SessionManager_SessionStatus_Is_Starting_After_Hit_After_End()
        {
            var sessionManager = new SessionManager(null);
            sessionManager.Hit();
            sessionManager.End();
            sessionManager.Hit();

            Assert.AreEqual(SessionStatus.Starting, sessionManager.SessionStatus);
        }

        [TestMethod]
        public void SessionManager_SampleRate_0_Should_Never_Choose_Visitor()
        {
            for (var i = 0; i < 100; i++)
            {
                var sessionManager = new SessionManager(null, 0);
                Assert.AreEqual(VisitorStatus.SampledOut, sessionManager.VisitorStatus);
            }
        }

        [TestMethod]
        public void SessionManager_SampleRate_100_Should_Always_Choose_Visitor()
        {
            for (var i = 0; i < 100; i++)
            {
                var sessionManager = new SessionManager(null, 100);
                Assert.AreEqual(VisitorStatus.Active, sessionManager.VisitorStatus);
            }
        }

        [TestMethod]
        public void SessionManager_SampleRate_50_Should_Choose_Half_Of_The_Visitors()
        {
            TestSampleRateDistribution(50);
        }

        [TestMethod]
        public void SessionManager_SampleRate_25_Should_Choose_Quarter_Of_The_Visitors()
        {
            TestSampleRateDistribution(25);
        }

        [TestMethod]
        public void SessionManager_SampleRate_66_Should_Choose_Two_Thirds_Of_The_Visitors()
        {
            TestSampleRateDistribution(66);
        }

        private static void TestSampleRateDistribution(double sampleRate)
        {
            const int repetitions = 1000;

            int sampledCount = 0;
            var sessionManager = new SessionManager(null);

            // Setup a linear distribution as random numbers are too unpredictable
            var nextSelector = 0.0;
            sessionManager.SampleSelector = () =>
            {
                var selector = nextSelector;
                nextSelector += (100.0 / repetitions);
                return selector;
            };

            for (var i = 0; i < repetitions; i++)
            {
                if (sessionManager.ShouldTrackThisNewVisitor(sampleRate))
                    sampledCount++;
            }

            var expected = repetitions * (sampleRate / 100);
            Assert.AreEqual(expected, sampledCount);
        }

        private static readonly Random random = new Random();

        private static SessionState CreateSampleState()
        {
            return new SessionState
            {
                HitId = random.Next(),
                VisitorId = Guid.NewGuid(),
                SessionHitCount = random.Next(),
                SessionNumber = random.Next(),
                FirstVisitAt = DateTime.Now.Subtract(new TimeSpan(1, 12, 30, 20)),
                LastActivityAt = DateTime.Now.Subtract(new TimeSpan(0, 0, 0, 1)),
                PreviousSessionStartedAt = DateTime.Now.Subtract(new TimeSpan(0, 1, 10, 15)),
                SessionStartedAt = DateTime.Now.Subtract(new TimeSpan(0, 0, 0, 15)),
                Referrer = new Uri("http://damieng.com/" + random.Next().ToString(CultureInfo.InvariantCulture))
            };
        }
    }
}