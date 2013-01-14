using System;
using CSharpAnalytics.Internal.Urchin;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Internal.Urchin
{
    [TestClass]
    public class UtmeEncoderTests
    {
        [TestMethod]
        public void UtmeEncoder_EncodeValue_Encodes_Correctly()
        {
            var encoded = UtmeEncoder.EscapeValue("a*b)c!d'2*(!*");

            Assert.AreEqual("a'2b'1c'3d'02'2('3'2", encoded);
        }

        [TestMethod]
        public void UtmeEncoder_Compress_No_Missing_Values_Correctly()
        {
            var original = new[] { "a", "b", "c", "d", "e" };

            var compressed = UtmeEncoder.Compress(original);

            CollectionAssert.AreEqual(original, compressed);
        }

        [TestMethod]
        public void UtmeEncoder_Compress_Middle_Missing_Values_Correctly()
        {
            var compressed = UtmeEncoder.Compress(new[] { "a", null, "c", "d", "", " ", "g" });

            CollectionAssert.AreEqual(new [] { "a", "3!c", "d", "7!g" }, compressed);
        }

        [TestMethod]
        public void UtmeEncoder_Compress_Starting_Missing_Values_Correctly()
        {
            var compressed = UtmeEncoder.Compress(new[] { "", null, " ", "d", "e" });

            CollectionAssert.AreEqual(new[] { "4!d", "e" }, compressed);
        }

        [TestMethod]
        public void UtmeEncoder_Compress_Ending_Missing_Values_Correctly()
        {
            var compressed = UtmeEncoder.Compress(new[] { "a", "b", "", null, " " });

            CollectionAssert.AreEqual(new[] { "a", "b" }, compressed);
        }

        [TestMethod]
        public void UtmeEncoder_Compress_All_Missing_Values_Correctly()
        {
            var compressed = UtmeEncoder.Compress(new[] { "", null, " " });

            CollectionAssert.AreEqual(new string[] { }, compressed);
        }

        [TestMethod]
        public void UtmeEncoder_EscapeValue_With_Empty_String_Returns_Empty_String()
        {
            var escaped = UtmeEncoder.EscapeValue("");
            Assert.AreEqual("", escaped);
        }

#if WINDOWS_STORE
        [TestMethod]
        public void UtmeEncoder_Compress_With_Null_Throws_ArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => UtmeEncoder.Compress(null));
        }
#endif

#if NET45
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UtmeEncoder_Compress_With_Null_Throws_ArgumentNullException()
        {
            UtmeEncoder.Compress(null);
        }
#endif
    }
}