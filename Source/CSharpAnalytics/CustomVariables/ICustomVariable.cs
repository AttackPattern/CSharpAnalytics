namespace CSharpAnalytics.CustomVariables
{
    /// <summary>
    /// Interface for classes that can provide a name and value for custom variables.
    /// </summary>
    public interface ICustomVariable
    {
        /// <summary>
        /// name of a custom variable.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Value of a custom variable.
        /// </summary>
        string Value { get; }
    }
}
