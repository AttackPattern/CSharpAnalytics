using System.Linq;
using CSharpAnalytics.Protocols.Urchin.CustomVariables;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Protocols.Urchin.CustomVariables
{
    [TestClass]
    public class CustomVariableSlotsTests
    {
        [TestMethod]
        public void CustomVariableSlots_Indexer_Sets_Slot()
        {
            var slots = new CustomVariableSlots();
            var slotOne = new CustomVariable("one", "1");

            slots[1] = slotOne;

            Assert.AreEqual(slotOne, slots[1]);
        }

        [TestMethod]
        public void CustomVariableSlots_AllSlots_Identifies_By_Key()
        {
            var slots = new CustomVariableSlots();
            var slotOne = new CustomVariable("one", "1");
            var slotSix = new CustomVariable("six", "6");

            slots[1] = slotOne;
            slots[6] = slotSix;

            Assert.AreEqual(slotOne, slots.AllSlots.First(s => s.Key == 1).Value);
            Assert.AreEqual(slotSix, slots.AllSlots.First(s => s.Key == 6).Value);
        }
    }
}