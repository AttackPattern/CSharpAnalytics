using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CSharpAnalytics.Network;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.Threading.Tasks;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Network
{
    [TestClass]
    public class BackgroundHttpRequesterTests
    {
        [TestMethod]
        public void BackgroundHttpRequester_Start_Uses_Previous_List()
        {
            var expectedList = TestHelpers.CreateRequestList(4);
            var actualList = new List<Uri>();
            Func<Uri, CancellationToken, bool> processor = (u, c) => { actualList.Add(u); return true; };

            var requester = new BackgroundHttpFuncRequester(processor);
            requester.Start(TimeSpan.FromMilliseconds(10), expectedList);

            TestHelpers.WaitForQueueToEmpty(requester);
            Assert.AreEqual(expectedList.Count, actualList.Count);
            CollectionAssert.AreEqual(expectedList, actualList);
        }

        [TestMethod]
        public void BackgroundHttpRequester_Start_Uses_Previous_List_First()
        {
            var expectedList = TestHelpers.CreateRequestList(10);
            var actualList = new List<Uri>();
            Func<Uri, CancellationToken, bool> processor = (u, c) => { actualList.Add(u); return true; };

            var requester = new BackgroundHttpFuncRequester(processor);
            requester.Start(TimeSpan.FromMilliseconds(10), expectedList.Take(5));
            foreach (var uri in expectedList.Skip(5))
                requester.Add(uri);

            TestHelpers.WaitForQueueToEmpty(requester);
            Assert.AreEqual(expectedList.Count, actualList.Count);
            CollectionAssert.AreEqual(expectedList, actualList);
        }

        [TestMethod]
        public void BackgroundHttpRequester_StopAsync_Stops_Requesting()
        {
            var actualList = new List<Uri>();
            Func<Uri, CancellationToken, bool> processor = (u, c) => { actualList.Add(u); return true; };

            var requester = new BackgroundHttpFuncRequester(processor);
            requester.Start(TimeSpan.FromMilliseconds(10));
            requester.StopAsync().Wait();
            foreach (var uri in TestHelpers.CreateRequestList(3))
                requester.Add(uri);

            Assert.AreEqual(0, actualList.Count);
            Assert.AreEqual(3, requester.QueueCount);
        }

        [TestMethod]
        public void BackgroundHttpRequester_StopAsync_Stops_Current_Active_Request()
        {
            var cancelled = false;
            var mre = new ManualResetEventSlim();

            Func<Uri, CancellationToken, bool> processor = (u, c) =>
            {
                try
                {
                    mre.Wait(c);
                }
                catch (AggregateException)
                {
                }
                catch (OperationCanceledException)
                {
                }
                mre.Set();
                cancelled = c.IsCancellationRequested;
                return true;
            };

            var requester = new BackgroundHttpFuncRequester(processor);
            requester.Add(TestHelpers.CreateRequestList(1)[0]);
            requester.Start(TimeSpan.FromMilliseconds(10));

            Assert.IsFalse(cancelled);
            requester.StopAsync().Wait(3000);

            Assert.IsTrue(mre.Wait(3000));
            Assert.IsTrue(cancelled);
        }
    }
}