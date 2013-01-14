namespace CSharpAnalytics.CustomVariables
{
    /// <summary>
    /// Defines the various levels of scope that can be given to a set of custom variables.
    /// </summary>
    public enum CustomVariableScope
    {
        /// <summary>
        /// The custom variable is not scoped.
        /// </summary>
        None = 0,

        /// <summary>
        /// The custom variable is scoped to the Visitor.
        /// </summary>
        Visitor = 1,

        /// <summary>
        /// The custom variable is scoped to this visitors session.
        /// </summary>
        Session = 2,

        /// <summary>
        /// The custom variable is scoped to an individual activity or hit.
        /// </summary>
        Activity = 3
    };
}