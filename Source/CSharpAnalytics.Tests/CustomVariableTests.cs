using CSharpAnalytics.CustomVariables;
using System.Globalization;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test
{
    [TestClass]
    public class CustomVariableTests
    {
        [TestMethod]
        public void ScopedCustomVariableSlots_Are_Scoped()
        {
            var sessionSlots = new ScopedCustomVariableSlots(CustomVariableScope.Session);
            var activitySlots = new ScopedCustomVariableSlots(CustomVariableScope.Activity);

            Assert.AreEqual(CustomVariableScope.Session, sessionSlots.Scope);
            Assert.AreEqual(CustomVariableScope.Activity, activitySlots.Scope);
        }

        [TestMethod]
        public void ScopedCustomVariableSlots_Slots_Are_Initially_Empty()
        {
            var slots = new ScopedCustomVariableSlots(CustomVariableScope.Session);

            Assert.IsNull(slots[0]);
            Assert.IsNull(slots[1]);
            Assert.IsNull(slots[2]);
            Assert.IsNull(slots[3]);
            Assert.IsNull(slots[4]);
        }

        [TestMethod]
        public void ScopedCustomVariableSlots_Slots_Can_Be_Set()
        {
            var slotOne = new CustomVariable("one", "1");
            var slotTwo = new CustomVariable("two", "2");
            var slotThree = new CustomVariable("three", "3");
            var slotFour = new CustomVariable("four", "4");
            var slotFive = new CustomVariable("five", "5");

            var slots = new ScopedCustomVariableSlots(CustomVariableScope.Session);
            slots[0] = slotOne;
            slots[1] = slotTwo;
            slots[2] = slotThree;
            slots[3] = slotFour;
            slots[4] = slotFive;           

            Assert.AreSame(slotOne, slots[0]);
            Assert.AreSame(slotTwo, slots[1]);
            Assert.AreSame(slotThree, slots[2]);
            Assert.AreSame(slotFour, slots[3]);
            Assert.AreSame(slotFive, slots[4]);
        }

        [TestMethod]
        public void CustomVariable_Constructor_Sets_Properties_Correctly()
        {
            var custom = new CustomVariable("name", "value");

            Assert.AreEqual("name", custom.Name);
            Assert.AreEqual("value", custom.Value);
        }

        [TestMethod]
        public void EvaluatedCustomVariable_Constructor_Sets_Properties_Correctly()
        {
            var evaluated = new EvaluatedCustomVariable("name", () => "value");

            Assert.AreEqual("name", evaluated.Name);
            Assert.AreEqual("value", evaluated.Value);
        }

        [TestMethod]
        public void EvaluatedCustomVariable_Evaluates_Every_Time_Valued_Accessed()
        {
            var evaluationCount = 0;
            var evaluated = new EvaluatedCustomVariable("name", () => (++evaluationCount).ToString(CultureInfo.InvariantCulture));

            Assert.AreEqual("name", evaluated.Name);
            Assert.AreEqual("1", evaluated.Value);
            Assert.AreEqual("2", evaluated.Value);
            Assert.AreEqual("3", evaluated.Value);
        }
    }
}