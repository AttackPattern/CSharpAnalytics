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

            var sessionManager = new SessionManager(timeout, state);

            Assert.AreEqual(timeout, sessionManager.Timeout);
            Assert.AreEqual(state.PreviousSessionStartedAt, sessionManager.PreviousSessionStartedAt);
            Assert.AreEqual(state.Referrer, sessionManager.Referrer);

            Assert.AreEqual(state.FirstVisitAt, sessionManager.Visitor.FirstVisitAt);
            Assert.AreEqual(state.VisitorId, sessionManager.Visitor.Id);
            
            Assert.AreEqual(state.HitId, sessionManager.Session.HitId);
            Assert.AreEqual(state.SessionHitCount, sessionManager.Session.HitCount);
            Assert.AreEqual(state.SessionNumber, sessionManager.Session.Number);
            Assert.AreEqual(state.SessionStartedAt, sessionManager.Session.StartedAt);
        }

        [TestMethod]
        public void SessionManager_Created_From_Null_State_Is_Fresh()
        {
            var timeout = TimeSpan.FromHours(1.25);

            var sessionManager = new SessionManager(timeout, null);

            Assert.AreEqual(timeout, sessionManager.Timeout);
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

            var sessionManager = new SessionManager(TimeSpan.FromDays(1), expected);

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
            var sessionManager = new SessionManager(TimeSpan.FromSeconds(19), null) { Referrer = referrer };

            Assert.AreEqual(referrer, sessionManager.Referrer);
        }

        [TestMethod]
        public void SessionManager_Creates_New_Session_When_Hit_After_Timeout()
        {
            var timeout = TimeSpan.FromSeconds(2);
            var sessionManager = new SessionManager(timeout, null);

            Assert.AreEqual(1, sessionManager.Session.Number);

            var starting = DateTimeOffset.Now;
            Task.Delay(timeout + TimeSpan.FromSeconds(1)).Wait();

            sessionManager.Hit();
            Assert.AreEqual(2, sessionManager.Session.Number);
            Assert.IsTrue(sessionManager.Session.StartedAt >= starting, "Session StartedAt too early");
            Assert.IsTrue(sessionManager.Session.StartedAt <= DateTimeOffset.Now, "Session StartedAt too late");
        }

        [TestMethod]
        public void SessionManager_Creates_New_Session_When_Requested()
        {
            var timeout = TimeSpan.FromSeconds(200);
            var sessionManager = new SessionManager(timeout, null);

            Assert.AreEqual(1, sessionManager.Session.Number);

            var starting = DateTimeOffset.Now;

            sessionManager.StartNewSession();
            Assert.AreEqual(2, sessionManager.Session.Number);
            Assert.IsTrue(sessionManager.Session.StartedAt >= starting, "Session StartedAt too early");
            Assert.IsTrue(sessionManager.Session.StartedAt <= DateTimeOffset.Now, "Session StartedAt too late");
        }

        [TestMethod]
        public void SessionManager_Creates_New_Session_In_A_Thread_Safe_Way()
        {
            var timeout = TimeSpan.FromSeconds(2);
            var sessionManager = new SessionManager(timeout, null);

            Task.Delay(timeout + TimeSpan.FromSeconds(1)).Wait();

            Task.WaitAll(
                Task.Run(() => { for (var i = 0; i < 500; i++) sessionManager.Hit(); }),
                Task.Run(() => { for (var i = 0; i < 500; i++) sessionManager.Hit(); }),
                Task.Run(() => { for (var i = 0; i < 500; i++) sessionManager.Hit(); })
            );

            Task.Delay(timeout + TimeSpan.FromSeconds(1)).Wait();

            Task.WaitAll(
                Task.Run(() => { for (var i = 0; i < 500; i++) sessionManager.Hit(); }),
                Task.Run(() => { for (var i = 0; i < 500; i++) sessionManager.Hit(); }),
                Task.Run(() => { for (var i = 0; i < 500; i++) sessionManager.Hit(); })
            );

            Assert.AreEqual(3, sessionManager.Session.Number);
            Assert.AreEqual(1500, sessionManager.Session.HitCount);
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
                Referrer = new Uri("http://damieng.com" + random.Next().ToString())
            };
        }
    }
}