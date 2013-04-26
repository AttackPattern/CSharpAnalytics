﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CSharpAnalytics.Protocols.Urchin.CustomVariables
{
    /// <summary>
    /// Captures a set of custom variables in slots numbered 1-5 with an associated scope.
    /// </summary>
    [DebuggerDisplay("Scope={Scope}")]
    public class ScopedCustomVariableSlots
    {
        private readonly CustomVariableScope scope;
        private readonly Dictionary<int, ICustomVariable> slots = new Dictionary<int, ICustomVariable>();

        /// <summary>
        /// Create a ScopedCustomVariableSlots to contain a number of slots for a given scope.
        /// </summary>
        /// <param name="scope">Scope of the custom variables held in the slots.</param>
        internal ScopedCustomVariableSlots(CustomVariableScope scope)
        {
            if (!Enum.IsDefined(typeof(CustomVariableScope), scope) || scope == CustomVariableScope.None)
                throw new ArgumentOutOfRangeException("scope");

            this.scope = scope;
        }

        /// <summary>
        /// Scope of the custom variables for these slots.
        /// </summary>
        public CustomVariableScope Scope { get { return scope; } }

        /// <summary>
        /// Custom variables indexed by their slot number.
        /// </summary>
        /// <param name="slot">Slot number.</param>
        /// <returns>CustomVariable associated with this slot.</returns>
        public ICustomVariable this[int slot]
        {
            get
            {
                ICustomVariable customVariable;
                slots.TryGetValue(slot, out customVariable);
                return customVariable;
            }
            set
            {
                if (value == null)
                {
                    if (slots.ContainsKey(slot))
                        slots.Remove(slot);
                }
                else
                {
                    slots[slot] = value;
                }
            }
        }

        /// <summary>
        /// Enumeration of all custom variables and their associated indexes.
        /// </summary>
        public IEnumerable<KeyValuePair<int, ICustomVariable>> AllSlots
        {
            get { return slots; }
        }
    }
}