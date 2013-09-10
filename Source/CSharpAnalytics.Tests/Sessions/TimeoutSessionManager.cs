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
    public class TimeoutSessionManagerTests
    {
        [TestMethod]
        public void TimeoutSessionManager_Can_Be_Created_From_State()
        {
            var timeout = TimeSpan.FromMinutes(5);
            var state = CreateSampleState();

            var sessionManager = new TimeoutSessionManager(state, timeout);

            Assert.AreEqual(timeout, sessionManager.Timeout);
            Assert.AreEqual(state.PreviousSessionStartedAt, sessionManager.PreviousSessionStartedAt);
        }

        [TestMethod]
        public void TimeoutSessionManager_Created_From_Null_State_Is_Fresh()
        {
            var timeout = TimeSpan.FromHours(1.25);

            var sessionManager = new TimeoutSessionManager(null, timeout);

            Assert.AreEqual(timeout, sessionManager.Timeout);
            Assert.IsNull(sessionManager.Referrer);
        }

        [TestMethod]
        public void TimeoutSessionManager_Created_From_State_Provides_Same_State()
        {
            var expected = CreateSampleState();

            var sessionManager = new TimeoutSessionManager(expected, TimeSpan.FromDays(1));
            var actual = sessionManager.GetState();

            Assert.AreEqual(expected.VisitorId, actual.VisitorId);
        }

        [TestMethod]
        public void TimeoutSessionManager_Creates_New_Session_When_Hit_After_Timeout()
        {
            var timeout = TimeSpan.FromSeconds(2);
            var sessionManager = new TimeoutSessionManager(null, timeout);

            Assert.AreEqual(1, sessionManager.Session.Number);

            var starting = DateTimeOffset.Now;
            Task.Delay(timeout + TimeSpan.FromSeconds(1)).Wait();

            sessionManager.Hit();
            Assert.AreEqual(2, sessionManager.Session.Number);
            Assert.IsTrue(sessionManager.Session.StartedAt >= starting, "Session StartedAt too early");
            Assert.IsTrue(sessionManager.Session.StartedAt <= DateTimeOffset.Now, "Session StartedAt too late");
        }

        [TestMethod]
        public void TimeoutSessionManager_Creates_New_Session_In_A_Thread_Safe_Way()
        {
            var timeout = TimeSpan.FromSeconds(2);
            var sessionManager = new TimeoutSessionManager(null, timeout);

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
                VisitorId = Guid.NewGuid(),
                SessionHitCount = random.Next(),
                SessionNumber = random.Next(),
                LastActivityAt = DateTime.Now.Subtract(new TimeSpan(0, 0, 0, 1)),
                PreviousSessionStartedAt = DateTime.Now.Subtract(new TimeSpan(0, 1, 10, 15)),
                SessionStartedAt = DateTime.Now.Subtract(new TimeSpan(0, 0, 0, 15)),
                Referrer = new Uri("http://damieng.com/" + random.Next().ToString(CultureInfo.InvariantCulture))
            };
        }
    }
}