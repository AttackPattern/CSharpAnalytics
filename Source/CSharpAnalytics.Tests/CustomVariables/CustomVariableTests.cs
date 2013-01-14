using System;
using CSharpAnalytics.CustomVariables;
#if WINDOWS_STORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
#endif

namespace CSharpAnalytics.Test.CustomVariables
{
    [TestClass]
    public class CustomVariableTests
    {
        [TestMethod]
        public void CustomVariable_Constructor_Sets_Properties()
        {
            var customVariable = new CustomVariable("name", "value");

            Assert.AreEqual("name", customVariable.Name);
            Assert.AreEqual("value", customVariable.Value);
        }
    }
}