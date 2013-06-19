using System;
using System.Globalization;
using CSharpAnalytics.Protocols;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Protocols
{
    [TestClass]
    public class EpochTimeTests
    {
        [TestMethod]
        public void EpochTime_ToString_Returns_Correct_Value_From_Seconds_Constructor()
        {
            const int expected = 1356478605;
            var epochTime = new EpochTime(expected);

            Assert.AreEqual(expected.ToString(CultureInfo.InvariantCulture), epochTime.ToString());
        }

        [TestMethod]
        public void EpochTime_ToDateTimeOffset_Returns_Correct_Value_From_Seconds_Constructor()
        {
            var epochTime = new EpochTime(1356478605);

            Assert.AreEqual(new DateTimeOffset(2012, 12, 25, 23, 36, 45, TimeSpan.Zero), epochTime.ToDateTimeOffset());
        }

        [TestMethod]
        public void EpochTime_ToUtcString_Returns_Correct_Value_From_Seconds_Constructor()
        {
            var epochTime = new EpochTime(1356478605);

            Assert.AreEqual("Tue, 25 Dec 2012 23:36:45 GMT", epochTime.ToUtcString());
        }

        [TestMethod]
        public void EpochTime_ToDateTimeOffset_Returns_Correct_Value_From_DateTimeOffset_Constructor()
        {
            var expected = new DateTimeOffset(2012, 12, 25, 23, 36, 45, TimeSpan.Zero);
            var epochTime = new EpochTime(expected);

            Assert.AreEqual(expected, epochTime.ToDateTimeOffset());
        }

        [TestMethod]
        public void EpochTime_ToUtcString_Returns_Correct_Value_From_DateTimeOffset_Constructor()
        {
            var expected = new DateTimeOffset(2012, 12, 25, 23, 36, 45, TimeSpan.Zero);
            var epochTime = new EpochTime(expected);

            Assert.AreEqual("Tue, 25 Dec 2012 23:36:45 GMT", epochTime.ToUtcString());
        }

        [TestMethod]
        public void EpochTime_TryParse_Returns_Correct_Value_Given_Valid_Number_Of_Seconds()
        {
            EpochTime epochTime;
            var parsed = EpochTime.TryParseSeconds("1356479712", out epochTime);

            Assert.IsTrue(parsed);

            Assert.AreEqual("Tue, 25 Dec 2012 23:55:12 GMT", epochTime.ToUtcString());
        }

        [TestMethod]
        public void EpochTime_TryParse_Fails_Given_Decimal_Number_Of_Seconds()
        {
            EpochTime epochTime;
            var parsed = EpochTime.TryParseSeconds("123.45", out epochTime);

            Assert.IsFalse(parsed);
            Assert.IsNull(epochTime);
        }

        [TestMethod]
        public void EpochTime_TryParse_Fails_Given_Empty_String()
        {
            EpochTime epochTime;
            var parsed = EpochTime.TryParseSeconds("", out epochTime);

            Assert.IsFalse(parsed);
            Assert.IsNull(epochTime);
        }

        [TestMethod]
        public void EpochTime_TryParse_Fails_Given_Text()
        {
            EpochTime epochTime;
            var parsed = EpochTime.TryParseSeconds("Shiny", out epochTime);

            Assert.IsFalse(parsed);
            Assert.IsNull(epochTime);
        }
    }
}