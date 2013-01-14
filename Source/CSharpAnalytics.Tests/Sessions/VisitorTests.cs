using CSharpAnalytics.Sessions;
using System;
using System.Linq;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Sessions
{
    [TestClass]
    public class VisitorTests
    {
        private readonly Random random = new Random();

        [TestMethod]
        public void New_Default_Visitor_Has_Random_Id()
        {
            var ids = Enumerable.Range(1, 50).Select(_ => new Visitor().Id).ToArray();

            CollectionAssert.AllItemsAreUnique(ids);
        }

        [TestMethod]
        public void New_Default_Visitor_Started_Now()
        {
            var start = DateTimeOffset.Now;
            var session = new Visitor();
            var end = DateTimeOffset.Now;

            Assert.IsTrue(session.FirstVisitAt >= start, "FirstVisitAt too early expected after {0} found {1}", start, session.FirstVisitAt);
            Assert.IsTrue(session.FirstVisitAt <= end, "FirstVisitAt too late expected before {0} found {1}", end, session.FirstVisitAt);
        }

        [TestMethod]
        public void New_Visitor_With_Parameters_Sets_Properties()
        {
            var id = random.Next();
            var startedAt = DateTimeOffset.Now.Subtract(new TimeSpan(1, 2, 3, 4, 5));
            
            var visitor = new Visitor(id, startedAt);

            Assert.AreEqual(id, visitor.Id);
            Assert.AreEqual(startedAt, visitor.FirstVisitAt);
        }
    }
}