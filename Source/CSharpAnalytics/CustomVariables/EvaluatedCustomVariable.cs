using System;

namespace CSharpAnalytics.CustomVariables
{
    /// <summary>
    /// Captures the name and a function that when called will provide the value.
    /// Useful for dynamically changing values like counters, timings or external factors.
    /// </summary>
    public class EvaluatedCustomVariable : ICustomVariable
    {
        private readonly string name;
        private readonly Func<string> valueEvaluator;

        /// <summary>
        /// name of this custom variable.
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Function that will provide the value of this custom variable when required.
        /// </summary>
        public string Value { get { return valueEvaluator(); } }

        /// <summary>
        /// Creates a new evaluated custom variable with a given name and value evaluator.
        /// </summary>
        /// <param name="name">name of this custom variable to be assigned to the name property.</param>
        /// <param name="valueEvaluator">Value evaluator function to be assigned to the Value property.</param>
        public EvaluatedCustomVariable(string name, Func<string> valueEvaluator)
        {
            this.name = name;
            this.valueEvaluator = valueEvaluator;
        }
    }
}