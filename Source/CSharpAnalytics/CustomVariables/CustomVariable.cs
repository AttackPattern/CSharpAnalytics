namespace CSharpAnalytics.CustomVariables
{
    /// <summary>
    /// Captures the details of a basic custom variable.
    /// </summary>
    public class CustomVariable : ICustomVariable
    {
        private readonly string name;
        private readonly string value;

        /// <summary>
        /// Name of this custom variable.
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Value of this custom variable.
        /// </summary>
        public string Value { get { return value; } }

        /// <summary>
        /// Create a new evaluated custom variable with a given name and value evaluator.
        /// </summary>
        /// <param name="name">Name of this custom variable to be assigned to the name property.</param>
        /// <param name="value">Value of this custom variable to be assigned to the Value property.</param>
        public CustomVariable(string name, string value)
        {
            this.name = name;
            this.value = value;
        }
    }
}