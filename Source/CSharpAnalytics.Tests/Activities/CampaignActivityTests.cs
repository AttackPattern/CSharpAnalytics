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
    public class CampaignActivityTests
    {
        [TestMethod]
        public void CampaignActivity_Constructor_With_All_Parameters_Sets_Correct_Properties()
        {
            var activity = new CampaignActivity("source");

            Assert.AreEqual("source", activity.Source);

            Assert.IsNull(activity.Name);
            Assert.IsNull(activity.Medium);
            Assert.IsNull(activity.Term);
            Assert.IsNull(activity.Content);
        }

        [TestMethod]
        public void CampaignActivity_Properties_Can_Be_Set()
        {
            var activity = new CampaignActivity("source") {
                Name = "name",
                Medium = "medium",
                Term = "term",
                Content = "content",
                IsNewVisit = true
            };

            Assert.AreEqual("name", activity.Name);
            Assert.AreEqual("medium", activity.Medium);
            Assert.AreEqual("term", activity.Term);
            Assert.AreEqual("content", activity.Content);
            Assert.AreEqual(true, activity.IsNewVisit);
        }
    }
}