using System;
using CSharpAnalytics.Activities;
#if WINDOWS_STORE || WINDOWS_PHONE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Activities
{
    [TestClass]
    public class SocialActivityTests
    {
        [TestMethod]
        public void SocialActivity_Constructor_With_Minimal_Parameters_Sets_Correct_Properties()
        {
            var activity = new SocialActivity("action", "network");

            Assert.AreEqual("network", activity.Network);
            Assert.AreEqual("action", activity.Action);

            Assert.IsNull(activity.Target);
            Assert.IsNull(activity.PagePath);
        }

        [TestMethod]
        public void SocialActivity_Constructor_With_All_Parameters_Sets_Correct_Properties()
        {
            var activity = new SocialActivity("action", "network", target: "target", pagePath: "pagePath");

            Assert.AreEqual("network", activity.Network);
            Assert.AreEqual("action", activity.Action);
            Assert.AreEqual("target", activity.Target);
            Assert.AreEqual("pagePath", activity.PagePath);
        }
    }
}