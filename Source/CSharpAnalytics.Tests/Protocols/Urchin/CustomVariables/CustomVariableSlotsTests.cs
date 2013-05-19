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
        public void CustomVariableSlots_Indexer_Sets_Multiple_Slot()
        {
            var slots = new CustomVariableSlots();
            var slotOne = new CustomVariable("one", "1");
            var slotNine = new CustomVariable("nine", "9");

            slots[1] = slotOne;
            slots[9] = slotNine;

            Assert.AreEqual(2, slots.AllSlots.Count());
            Assert.AreEqual(slotOne, slots[1]);
            Assert.AreEqual(slotNine, slots[9]);
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

        [TestMethod]
        public void CustomVariableSlots_Indexer_Unsets_Slot_When_Set_To_Null()
        {
            var slots = new CustomVariableSlots();
            var slotOne = new CustomVariable("one", "1");
            var slotNine = new CustomVariable("nine", "9");

            slots[1] = slotOne;
            slots[9] = slotNine;
            slots[9] = null;

            Assert.AreEqual(1, slots.AllSlots.Count());
            Assert.AreEqual(slotOne, slots[1]);
            Assert.IsNull(slots[9]);
        }

        [TestMethod]
        public void CustomVariableSlots_Indexer_Overwrites_Slot_When_Set_Again()
        {
            var slots = new CustomVariableSlots();
            var slotOneA = new CustomVariable("oneA", "1A");
            var slotOneB = new CustomVariable("oneB", "1B");

            slots[1] = slotOneA;
            slots[1] = slotOneB;

            Assert.AreEqual(1, slots.AllSlots.Count());
            Assert.AreEqual(slotOneB, slots[1]);
        }
    }
}