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

            Assert.IsNull(slots.Slot1);
            Assert.IsNull(slots.Slot2);
            Assert.IsNull(slots.Slot3);
            Assert.IsNull(slots.Slot4);
            Assert.IsNull(slots.Slot5);
        }

        [TestMethod]
        public void ScopedCustomVariableSlots_Slots_Can_Be_Set()
        {
            var slotOne = new CustomVariable("one", "1");
            var slotTwo = new CustomVariable("two", "2");
            var slotThree = new CustomVariable("three", "3");
            var slotFour = new CustomVariable("four", "4");
            var slotFive = new CustomVariable("five", "5");

            var slots = new ScopedCustomVariableSlots(CustomVariableScope.Session)
            {
                Slot1 = slotOne,
                Slot2 = slotTwo,
                Slot3 = slotThree,
                Slot4 = slotFour,
                Slot5 = slotFive
            };

            Assert.AreSame(slotOne, slots.Slot1);
            Assert.AreSame(slotTwo, slots.Slot2);
            Assert.AreSame(slotThree, slots.Slot3);
            Assert.AreSame(slotFour, slots.Slot4);
            Assert.AreSame(slotFive, slots.Slot5);
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
            int evaluationCount = 0;
            var evaluated = new EvaluatedCustomVariable("name", () => (++evaluationCount).ToString(CultureInfo.InvariantCulture));

            Assert.AreEqual("name", evaluated.Name);
            Assert.AreEqual("1", evaluated.Value);
            Assert.AreEqual("2", evaluated.Value);
            Assert.AreEqual("3", evaluated.Value);
        }

        [TestMethod]
        public void FinalCustomVariables_Selects_Correct_Final_Variables()
        {
            var sessionScopedVariables = new ScopedCustomVariableSlots(CustomVariableScope.Session)
            {
                Slot1 = new CustomVariable("session-one-name", "session-one-value"),
                Slot3 = new CustomVariable("session-three-name", "session-three-value"),
            };

            var visitorScopedVariables = new ScopedCustomVariableSlots(CustomVariableScope.Visitor)
            {
                Slot1 = new CustomVariable("Visitor-one-name", "Visitor-one-value"),
                Slot2 = new CustomVariable("Visitor-two-name", "Visitor-two-value")
            };

            var activityScopedVariables = new ScopedCustomVariableSlots(CustomVariableScope.Activity)
            {
                Slot1 = new CustomVariable("activity-one-name", "activity-one-value"),
                Slot2 = new CustomVariable("activity-two-name", "activity-two-value"),
                Slot4 = new CustomVariable("activity-four-name", "activity-four-value"),
            };

            var final = new FinalCustomVariables(sessionScopedVariables, visitorScopedVariables, activityScopedVariables);

            Assert.AreEqual(CustomVariableScope.Visitor, final.Slot1.Item1);
            Assert.AreEqual(CustomVariableScope.Visitor, final.Slot2.Item1);
            Assert.AreEqual(CustomVariableScope.Session, final.Slot3.Item1);
            Assert.AreEqual(CustomVariableScope.Activity, final.Slot4.Item1);

            Assert.AreEqual("Visitor-one-name", final.Slot1.Item2.Name);
            Assert.AreEqual("Visitor-two-name", final.Slot2.Item2.Name);
            Assert.AreEqual("session-three-name", final.Slot3.Item2.Name);
            Assert.AreEqual("activity-four-name", final.Slot4.Item2.Name);  
 
            Assert.IsNull(final.Slot5);
        }
    }
}