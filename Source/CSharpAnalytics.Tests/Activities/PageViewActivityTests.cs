using System;
using CSharpAnalytics.Activities;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Activities
{
    [TestClass]
    public class PageViewActivityTests
    {
        [TestMethod]
        public void PageViewActivity_Constructor_With_All_Parameters_Sets_Correct_Properties()
        {
            var activity = new PageViewActivity("title", "page");

            Assert.AreEqual("title", activity.Title);
            Assert.AreEqual("page", activity.Page);
        }
    }
}