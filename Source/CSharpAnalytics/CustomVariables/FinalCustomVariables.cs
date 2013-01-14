using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpAnalytics.CustomVariables
{
    /// <summary>
    /// Resolves three sets of scoped custom variable slots into a single set of slots to track.
    /// </summary>
    internal class FinalCustomVariables
    {
        /// <summary>
        /// Create a final set of resolved custom variables from a single set of slots.
        /// </summary>
        /// <param name="slots">All unresolved custom variable slots.</param>
        public FinalCustomVariables(params ScopedCustomVariableSlots[] slots)
        {
            foreach (var item in slots.OrderBy(o => o.Scope))
            {
                Slot1 = Slot1 ?? (item.Slot1 == null ? null : Tuple.Create(item.Scope, item.Slot1));
                Slot2 = Slot2 ?? (item.Slot2 == null ? null : Tuple.Create(item.Scope, item.Slot2));
                Slot3 = Slot3 ?? (item.Slot3 == null ? null : Tuple.Create(item.Scope, item.Slot3));
                Slot4 = Slot4 ?? (item.Slot4 == null ? null : Tuple.Create(item.Scope, item.Slot4));
                Slot5 = Slot5 ?? (item.Slot5 == null ? null : Tuple.Create(item.Scope, item.Slot5));
            }
        }

        /// <summary>
        /// Custom variable and its scope in slot 1.
        /// </summary>
        public Tuple<CustomVariableScope, ICustomVariable> Slot1 { get; private set; }

        /// <summary>
        /// Custom variable and its scope in slot 2.
        /// </summary>
        public Tuple<CustomVariableScope, ICustomVariable> Slot2 { get; private set; }

        /// <summary>
        /// Custom variable and its scope in slot 3.
        /// </summary>
        public Tuple<CustomVariableScope, ICustomVariable> Slot3 { get; private set; }

        /// <summary>
        /// Custom variable and its scope in slot 4.
        /// </summary>
        public Tuple<CustomVariableScope, ICustomVariable> Slot4 { get; private set; }

        /// <summary>
        /// Custom variable and its scope in slot 5.
        /// </summary>
        public Tuple<CustomVariableScope, ICustomVariable> Slot5 { get; private set; }

        /// <summary>
        /// All final variables and their scopes.
        /// </summary>
        public IEnumerable<Tuple<CustomVariableScope, ICustomVariable>> AllSlots
        {
            get { return new [] { Slot1, Slot2, Slot3, Slot4, Slot4, Slot5 }; }
        }
    }
}