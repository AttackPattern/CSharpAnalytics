using System;
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
    public class ScopedCustomVariableTests
    {
        [TestMethod]
        public void ScopedCustomVariable_Constructor_Sets_Properties()
        {
            var customVariable = new CustomVariable("name3", "value3");
            var scopedCustomVariableSlot = new ScopedCustomVariableSlot(CustomVariableScope.Session, customVariable , 3);

            Assert.AreEqual(CustomVariableScope.Session, scopedCustomVariableSlot.Scope);
            Assert.AreEqual(3, scopedCustomVariableSlot.Slot);
            Assert.AreEqual(customVariable, scopedCustomVariableSlot.Variable);
        }

#if WINDOWS_STORE
        [TestMethod]
        public void ScopedCustomVariableSlot_Constructor_Throws_ArgumentOutOfRange_If_Enum_Undefined()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ScopedCustomVariableSlot((CustomVariableScope) 421, new CustomVariable("t", "k"), 9));
        }
#endif

#if NET45
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CustomVariable_Constructor_Throws_ArgumentOutOfRange_If_Enum_Undefined()
        {
            var scoped = new ScopedCustomVariableSlot((CustomVariableScope) 421, new CustomVariable("t", "k"), 9);
        }
#endif
    }
}