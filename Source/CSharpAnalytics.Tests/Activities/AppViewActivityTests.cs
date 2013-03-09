using CSharpAnalytics.Activities;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Activities
{
    [TestClass]
    public class AppViewActivityTests
    {
        [TestMethod]
        public void AppViewActivity_Constructor_With_Minimal_Parameters_Sets_Correct_Properties()
        {
            var activity = new AppViewActivity("screenName");

            Assert.AreEqual("screenName", activity.ScreenName);
        }
    }
}