using System;
using System.Collections.Generic;
using System.Linq;
using CSharpAnalytics.Network;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
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
            Func<Uri, bool> processor = u => { actualList.Add(u); return true; };

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
            Func<Uri, bool> processor = u => { actualList.Add(u); return true; };

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
            Func<Uri, bool> processor = u => { actualList.Add(u); return true; };

            var requester = new BackgroundHttpFuncRequester(processor);
            requester.Start(TimeSpan.FromMilliseconds(10));
            requester.StopAsync().Wait();
            foreach (var uri in TestHelpers.CreateRequestList(3))
                requester.Add(uri);

            Assert.AreEqual(0, actualList.Count);
            Assert.AreEqual(3, requester.QueueCount);
        }
    }
}