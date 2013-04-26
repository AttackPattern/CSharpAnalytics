using System;
using System.Globalization;
using CSharpAnalytics.Protocols.Urchin.CustomVariables;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.Protocols.Urchin.CustomVariables
{
    [TestClass]
    public class EvaluatedCustomVariableTests
    {
        [TestMethod]
        public void EvaluatedCustomVariable_Constructor_Sets_Properties()
        {
            var evaluatedCustomVariable = new EvaluatedCustomVariable("name", () => "value");

            Assert.AreEqual("name", evaluatedCustomVariable.Name);
            Assert.AreEqual("value", evaluatedCustomVariable.Value);
        }

        [TestMethod]
        public void EvaluatedCustomVariable_Evaluates_Value_Every_Time()
        {
            var count = 0;
            Func<string> countFunction = () => { count++; return count.ToString(CultureInfo.InvariantCulture); };

            var evaluatedCustomVariable = new EvaluatedCustomVariable("name", countFunction);

            Assert.AreEqual("1", evaluatedCustomVariable.Value);
            Assert.AreEqual("2", evaluatedCustomVariable.Value);
            Assert.AreEqual("3", evaluatedCustomVariable.Value);
        }
    }
}