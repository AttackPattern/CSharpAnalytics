using System;
using System.Linq;
using CSharpAnalytics.CustomVariables;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.CustomVariables
{
    [TestClass]
    public class ScopedCustomVariableTests
    {
        [TestMethod]
        public void ScopedCustomVariable_Constructor_Sets_Properties()
        {
            const CustomVariableScope scope = CustomVariableScope.Session;
            var scopedCustomVariableSlots = new ScopedCustomVariableSlots(scope);

            Assert.AreEqual(scope, scopedCustomVariableSlots.Scope);
            Assert.AreEqual(0, scopedCustomVariableSlots.AllSlots.Count());
        }

        [TestMethod]
        public void ScopedCustomVariable_Slot_Properties_Can_Be_Set()
        {
            var scopedCustomVariableSlots = new ScopedCustomVariableSlots(CustomVariableScope.Session);
            var slot1 = new CustomVariable("name1", "value1");
            var slot2 = new CustomVariable("name2", "value2");
            var slot3 = new CustomVariable("name3", "value3");
            var slot4 = new CustomVariable("name4", "value4");
            var slot5 = new CustomVariable("name5", "value5");

            scopedCustomVariableSlots[0] = slot1;
            scopedCustomVariableSlots[1] = slot2;
            scopedCustomVariableSlots[2] = slot3;
            scopedCustomVariableSlots[3] = slot4;
            scopedCustomVariableSlots[4] = slot5;

            Assert.AreSame(slot1, scopedCustomVariableSlots[0]);
            Assert.AreSame(slot2, scopedCustomVariableSlots[1]);
            Assert.AreSame(slot3, scopedCustomVariableSlots[2]);
            Assert.AreSame(slot4, scopedCustomVariableSlots[3]);
            Assert.AreSame(slot5, scopedCustomVariableSlots[4]);
        }

#if WINDOWS_STORE
        [TestMethod]
        public void CustomVariable_Constructor_Throws_ArgumentOutOfRange_If_Enum_Undefined()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ScopedCustomVariableSlots((CustomVariableScope) 1234));
        }

        [TestMethod]
        public void CustomVariable_Constructor_Throws_ArgumentOutOfRange_If_Scope_Is_None()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ScopedCustomVariableSlots(CustomVariableScope.None));
        }
#endif

#if NET45
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CustomVariable_Constructor_Throws_ArgumentOutOfRange_If_Enum_Undefined()
        {
            var scoped = new ScopedCustomVariableSlots((CustomVariableScope) 1234);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CustomVariable_Constructor_Throws_ArgumentOutOfRange_If_Scope_Is_None()
        {
            var scoped = new ScopedCustomVariableSlots(CustomVariableScope.None);
        }
#endif
    }
}