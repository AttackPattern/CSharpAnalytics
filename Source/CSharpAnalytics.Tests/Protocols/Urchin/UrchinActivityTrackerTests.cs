using System;
using System.Linq;
using CSharpAnalytics.Activities;
using CSharpAnalytics.Protocols.Urchin;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Protocols.Urchin
{
    [TestClass]
    public class UrchinActivityTrackerTests
    {
        [TestMethod]
        public void UrchinActivityTracker_GetParameter_For_CampaignActivity_Returns_Correct_Values()
        {
            var activity = new CampaignActivity("source");

            var parameters = UrchinActivityTracker.GetParameters(activity).ToDictionary(k => k.Key, v => v.Value);

            Assert.AreEqual("source", parameters["utmcsr"]);
        }

        [TestMethod]
        public void UrchinActivityTracker_GetParameter_For_CampaignActivity_Returns_Correct_Optional_Values()
        {
            var activity = new CampaignActivity("source")
            {
                Name = "name",
                Medium = "medium",
                Term = "term",
                Content = "content"
            };

            var parameters = UrchinActivityTracker.GetParameters(activity).ToDictionary(k => k.Key, v => v.Value);

            Assert.AreEqual("source", parameters["utmcsr"]);
            Assert.AreEqual("name", parameters["utmccn"]);
            Assert.AreEqual("medium", parameters["utmcmd"]);
            Assert.AreEqual("term", parameters["utmctr"]);
            Assert.AreEqual("content", parameters["utmcct"]);
        }

        [TestMethod]
        public void UrchinActivityTracker_GetParameter_For_CampaignActivity_Returns_Correct_Keys_When_IsNewVisit()
        {
            var activity = new CampaignActivity("source") { IsNewVisit = true };

            var results = UrchinActivityTracker.GetParameters(activity).ToDictionary(k => k.Key, v => v);

            var expectedKeys = new[] { "utmcsr", "utmcn" };
            CollectionAssert.AreEquivalent(expectedKeys, results.Keys);
        }

        [TestMethod]
        public void UrchinActivityTracker_GetParameter_For_CampaignActivity_Returns_Correct_Keys_When_Not_IsNewVisit()
        {
            var activity = new CampaignActivity("source");

            var results = UrchinActivityTracker.GetParameters(activity).ToDictionary(k => k.Key, v => v);

            var expectedKeys = new[] { "utmcsr", "utmcr" };
            CollectionAssert.AreEquivalent(expectedKeys, results.Keys);
        }

        [TestMethod]
        public void UrchinActivityTracker_GetParameter_For_EventActivity_Returns_Correct_Keys()
        {
            var activity = new EventActivity("action", "category", nonInteraction: true);

            var results = UrchinActivityTracker.GetParameters(activity).ToDictionary(k => k.Key, v => v);

            var expectedKeys = new[] { "utmt", "utme", "utmi" };
            CollectionAssert.AreEquivalent(expectedKeys, results.Keys);
        }

        [TestMethod]
        public void UrchinActivityTracker_GetParameter_For_EventActivity_Returns_Correct_Utmt_Value()
        {
            var activity = new EventActivity("action", "category");

            var results = UrchinActivityTracker.GetParameters(activity).ToDictionary(k => k.Key, v => v);

            CollectionAssert.Contains(results.Keys, "utmt");
            Assert.AreEqual("event", results["utmt"].Value);
        }

        [TestMethod]
        public void UrchinActivityTracker_GetParameter_For_EventActivity_Returns_Correctly_Encoded_Utme_Value()
        {
            var activity = new EventActivity("*)!", "a*b)c!d'2", label: "*", value: 1);

            var actual = UrchinActivityTracker.GetParameters(activity).First(f => f.Key == "utme").Value;

            Assert.AreEqual("5(a'2b'1c'3d'02*'2'1'3*'2)(1)", actual);
        }

        [TestMethod]
        public void UrchinActivityTracker_GetParameter_For_EventActivity_With_Two_Parameters_Returns_Correct_Utme_Value()
        {
            var activity = new EventActivity("action", "category");

            var actual = UrchinActivityTracker.GetParameters(activity).First(f => f.Key == "utme").Value;

            Assert.AreEqual("5(category*action)", actual);
        }

        [TestMethod]
        public void UrchinActivityTracker_GetParameter_For_EventActivity_With_Three_Parameters_Returns_Correct_Utme_Value()
        {
            var activity = new EventActivity("action", "category", "label");

            var actual = UrchinActivityTracker.GetParameters(activity).First(f => f.Key == "utme").Value;

            Assert.AreEqual("5(category*action*label)", actual);
        }

        [TestMethod]
        public void UrchinActivityTracker_GetParameter_For_EventActivity_With_Four_Parameters_Returns_Correct_Utme_Value()
        {
            var activity = new EventActivity("action", "category", label: "label", value: 1234);

            var actual = UrchinActivityTracker.GetParameters(activity).First(f => f.Key == "utme").Value;

            Assert.AreEqual("5(category*action*label)(1234)", actual);
        }

        [TestMethod]
        public void UrchinActivityTracker_GetParameter_For_ItemActivity_Returns_Correct_Keys()
        {
            var activity = new TransactionItemActivity("code", "name", 1.23m, 4, "variation");

            var results = UrchinActivityTracker.GetParameters(activity).ToDictionary(k => k.Key, v => v);

            var expectedKeys = new[] { "utmt", "utmipc", "utmipn", "utmipr", "utmiqt", "utmiva" };
            CollectionAssert.AreEquivalent(expectedKeys, results.Keys);
        }

        [TestMethod]
        public void UrchinActivityTracker_GetParameter_For_ItemActivity_Returns_Correct_Utmt_Value()
        {
            var activity = new TransactionItemActivity("code", "name", 1.23m, 1);

            var results = UrchinActivityTracker.GetParameters(activity).ToDictionary(k => k.Key, v => v);

            CollectionAssert.Contains(results.Keys, "utmt");
            Assert.AreEqual("item", results["utmt"].Value);
        }

        [TestMethod]
        public void UrchinActivityTracker_GetParameter_For_ItemActivity_Returns_Correct_Values()
        {
            var activity = new TransactionItemActivity("code", "name", 1.23m, 1);

            var parameters = UrchinActivityTracker.GetParameters(activity).ToDictionary(k => k.Key, v => v.Value);

            Assert.AreEqual("code", parameters["utmipc"]);
            Assert.AreEqual("name", parameters["utmipn"]);
            Assert.AreEqual("1.23", parameters["utmipr"]);
        }

        [TestMethod]
        public void UrchinActivityTracker_GetParameter_For_ItemActivity_Returns_Correct_Optional_Values()
        {
            var activity = new TransactionItemActivity("code", "name", 1.23m, 4, "variation");

            var parameters = UrchinActivityTracker.GetParameters(activity).ToDictionary(k => k.Key, v => v.Value);

            Assert.AreEqual("variation", parameters["utmiva"]);
        }

        [TestMethod]
        public void UrchinActivityTracker_GetParameter_For_PageViewActivity_Returns_Correct_Keys()
        {
            var activity = new PageViewActivity("title", "page");

            var results =  UrchinActivityTracker.GetParameters(activity).ToDictionary(k => k.Key, v => v);

            var expectedKeys = new[] { "utmp", "utmdt" };
            CollectionAssert.AreEquivalent(expectedKeys, results.Keys);
        }

        [TestMethod]
        public void UrchinActivityTracker_GetParameter_For_PageViewActivity_Returns_No_Utmt_Value()
        {
            var activity = new PageViewActivity("title", "page");

            var results = UrchinActivityTracker.GetParameters(activity).ToDictionary(k => k.Key, v => v);

            CollectionAssert.DoesNotContain(results.Keys, "utmt");
        }

        [TestMethod]
        public void UrchinActivityTracker_GetParameter_For_SocialActivity_Returns_Correct_Keys()
        {
            var activity = new SocialActivity("action", "network", pagePath: "pagePath", target: "target");

            var actualKeys = UrchinActivityTracker.GetParameters(activity).Select(k => k.Key).ToArray();

            var expectedKeys = new[] { "utmsn", "utmsa", "utmsid", "utmp", "utmt" };
            CollectionAssert.AreEquivalent(expectedKeys, actualKeys);
        }

        [TestMethod]
        public void UrchinActivityTracker_GetParameter_For_SocialActivity_Returns_Correct_Utmt_Type()
        {
            var activity = new SocialActivity("action", "category");

            var results = UrchinActivityTracker.GetParameters(activity).ToDictionary(k => k.Key, v => v);

            CollectionAssert.Contains(results.Keys, "utmt");
            Assert.AreEqual("social", results["utmt"].Value);
        }

        [TestMethod]
        public void UrchinActivityTracker_GetParameter_For_SocialActivity_Returns_Correct_Values()
        {
            var activity = new SocialActivity("action", "network");

            var parameters = UrchinActivityTracker.GetParameters(activity).ToDictionary(k => k.Key, v => v.Value);

            Assert.AreEqual("network", parameters["utmsn"]);
            Assert.AreEqual("action", parameters["utmsa"]);
        }

        [TestMethod]
        public void UrchinActivityTracker_GetParameter_For_SocialActivity_Returns_Correct_Optional_Values()
        {
            var activity = new SocialActivity("action", "network", pagePath: "pagePath", target: "target");

            var parameters = UrchinActivityTracker.GetParameters(activity).ToDictionary(k => k.Key, v => v.Value);

            Assert.AreEqual("target", parameters["utmsid"]);
            Assert.AreEqual("pagePath", parameters["utmp"]);
        }

        [TestMethod]
        public void UrchinActivityTracker_GetParameter_For_TimedEventActivity_Returns_Correct_Keys()
        {
            var activity = new TimedEventActivity("category", "variable", TimeSpan.FromSeconds(1.5), "label");

            var results = UrchinActivityTracker.GetParameters(activity).ToDictionary(k => k.Key, v => v);

            var expectedKeys = new[] { "utmt", "utme" };
            CollectionAssert.AreEquivalent(expectedKeys, results.Keys);
        }

        [TestMethod]
        public void UrchinActivityTracker_GetParameter_For_TimedEventActivity_Returns_Correct_Utmt_Value()
        {
            var activity = new TimedEventActivity("category", "variable", TimeSpan.Zero);

            var results = UrchinActivityTracker.GetParameters(activity).ToDictionary(k => k.Key, v => v);

            CollectionAssert.Contains(results.Keys, "utmt");
            Assert.AreEqual("event", results["utmt"].Value);
        }
    }
}