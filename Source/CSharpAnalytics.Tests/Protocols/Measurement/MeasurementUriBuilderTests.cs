using System.Linq;
using CSharpAnalytics.Protocols.Measurement;
using CSharpAnalytics.Sessions;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Protocols.Measurement
{
    [TestClass]
    public class MeasurementUriBuilderTests
    {
        [TestMethod]
        public void MeasurementUriBuilderTests_GetParameters_For_Configuration_Returns_Correct_Keys()
        {
            var configuration = new MeasurementConfiguration("UA-1234-5", "AppName", "1.2.3.4");

            var keys = MeasurementUriBuilder.GetParameters(configuration).Select(k => k.Key).ToArray();

            CollectionAssert.AreEquivalent(new[] { "tid", "an", "av", "aip" }, keys);
        }

        [TestMethod]
        public void MeasurementUriBuilderTests_GetParameters_For_Configuration_Returns_No_Aip_Value_When_False()
        {
            var configuration = new MeasurementConfiguration("UA-1234-5", "AppName", "1.2.3.4") { AnonymizeIp = false };

            var keys = MeasurementUriBuilder.GetParameters(configuration).Select(k => k.Key).ToArray();

            CollectionAssert.DoesNotContain(keys, "aip");
        }

        [TestMethod]
        public void MeasurementUriBuilderTests_GetParameters_For_Environment_Returns_Correct_Values()
        {
            var environment = new Environment("en-gb")
                {
                    CharacterSet = "ISO-8550-1",
                    FlashVersion = "11.0.1b",
                    ScreenColorDepth = 32,
                    JavaEnabled = true,
                    ScreenHeight = 1050,
                    ScreenWidth = 1920,
                    ViewportHeight = 768,
                    ViewportWidth = 1024
                };

            var parameters = MeasurementUriBuilder.GetParameters(environment).ToDictionary(k => k.Key, v => v.Value);

            Assert.AreEqual("ISO-8550-1", parameters["de"]);
            Assert.AreEqual("en-gb", parameters["ul"]);
            Assert.AreEqual("11.0.1b", parameters["fl"]);
            Assert.AreEqual("32-bit", parameters["sd"]);
            Assert.AreEqual("1", parameters["je"]);
            Assert.AreEqual("1024x768", parameters["vp"]);
            Assert.AreEqual("1920x1050", parameters["sr"]);
        }

        [TestMethod]
        public void MeasurementUriBuilderTests_GetParameters_For_Environment_Returns_Correct_Je_Value()
        {
            var environment = new Environment("en-gb") { JavaEnabled = false };

            var jeValue = MeasurementUriBuilder.GetParameters(environment).First(f => f.Key == "je").Value;

            Assert.AreEqual("0", jeValue);
        }
    }
}