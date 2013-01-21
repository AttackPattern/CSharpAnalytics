using CSharpAnalytics.Protocols.Urchin;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Protocols.Urchin
{
    [TestClass]
    public class UtmeDecoderTests
    {
        [TestMethod]
        public void UtmeDecoder_Decompress_Does_Nothing_When_No_Missing_Values()
        {
            var original = new[] { "a", "b", "c", "d", "e" };

            var compressed = UtmeDecoder.Decompress(original);

            CollectionAssert.AreEqual(original, compressed);
        }

        [TestMethod]
        public void UtmeDecoder_Decompress_Ignores_Redundant_Index_Specifications()
        {
            var compressed = UtmeDecoder.Decompress(new[] { "a", "2!b", "3!c", "d", "5!e" });

            CollectionAssert.AreEqual(new[] { "a", "b", "c", "d", "e" }, compressed);
        }
    }
}
