using System.Linq;
using CSharpAnalytics.Protocols.Urchin;
using CSharpAnalytics.Protocols.Urchin.CustomVariables;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Protocols.Urchin
{
    [TestClass]
    public class UrchinAnalyticsTests
    {
        [TestMethod]
        public void UrchinUriBuilder_GetFinalCustomVariables_Selects_Correct_Final_Variables()
        {
            var analyticsClient = new UrchinAnalyticsClient();
            analyticsClient.SessionCustomVariables[0] = new CustomVariable("session-one-name", "session-one-value");
            analyticsClient.SessionCustomVariables[2] = new CustomVariable("session-three-name", "session-three-value");
            analyticsClient.VisitorCustomVariables[0] = new CustomVariable("Visitor-one-name", "Visitor-one-value");
            analyticsClient.VisitorCustomVariables[1] = new CustomVariable("Visitor-two-name", "Visitor-two-value");

            var activityScopedVariables = new CustomVariableSlots();
            activityScopedVariables[0] = new CustomVariable("activity-one-name", "activity-one-value");
            activityScopedVariables[1] = new CustomVariable("activity-two-name", "activity-two-value");
            activityScopedVariables[3] = new CustomVariable("activity-four-name", "activity-four-value");

            var final = analyticsClient.GetFinalCustomVariables(activityScopedVariables).ToDictionary(s => s.Slot);

            Assert.AreEqual(CustomVariableScope.Visitor, final[0].Scope);
            Assert.AreEqual(CustomVariableScope.Visitor, final[1].Scope);
            Assert.AreEqual(CustomVariableScope.Session, final[2].Scope);
            Assert.AreEqual(CustomVariableScope.Activity, final[3].Scope);

            Assert.AreEqual("Visitor-one-name", final[0].Variable.Name);
            Assert.AreEqual("Visitor-two-name", final[1].Variable.Name);
            Assert.AreEqual("session-three-name", final[2].Variable.Name);
            Assert.AreEqual("activity-four-name", final[3].Variable.Name);
        }
    }
}