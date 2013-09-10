using System;
using CSharpAnalytics.Sessions;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Sessions
{
    [TestClass]
    public class SessionTests
    {
        [TestMethod]
        public void Session_Created_Started_Now()
        {
            var start = DateTimeOffset.Now;
            var session = new Session();
            var end = DateTimeOffset.Now;

            Assert.IsTrue(session.StartedAt >= start, "StartedtAt too early expected after {0} found {1}", start, session.StartedAt);
            Assert.IsTrue(session.StartedAt <= end, "StartedtAt too late expected before {0} found {1}", end, session.StartedAt);
        }

        [TestMethod]
        public void Session_Created_Is_Session_Number_One()
        {
            var session = new Session();

            Assert.AreEqual(1, session.Number);
        }

        [TestMethod]
        public void Session_Created_With_Parameters_Sets_Properties()
        {
            var startedAt = DateTimeOffset.Now.Subtract(new TimeSpan(1, 2, 3, 4, 5));
            const int sessionNumber = 29;
            
            var session = new Session(startedAt, sessionNumber);

            Assert.AreEqual(startedAt, session.StartedAt);
            Assert.AreEqual(sessionNumber, session.Number);
        }
    }
}