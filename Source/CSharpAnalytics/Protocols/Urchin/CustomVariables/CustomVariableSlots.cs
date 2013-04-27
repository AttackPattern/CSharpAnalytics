﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System.Collections.Generic;

namespace CSharpAnalytics.Protocols.Urchin.CustomVariables
{
    /// <summary>
    /// Captures a set of custom variables in slots numbered 1-5 for a given scope.
    /// </summary>
    public class CustomVariableSlots
    {
        private readonly Dictionary<int, ICustomVariable> slots = new Dictionary<int, ICustomVariable>();

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