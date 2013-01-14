﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
using System;

namespace CSharpAnalytics.CustomVariables
{
    /// <summary>
    /// Captures a set of custom variables in slots numbered 1-5 with an associated scope.
    /// </summary>
    public class ScopedCustomVariableSlots
    {
        private readonly CustomVariableScope scope;

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
        /// Custom variable in slot 1.
        /// </summary>
        public ICustomVariable Slot1 { get; set; }

        /// <summary>
        /// Custom variable in slot 2.
        /// </summary>
        public ICustomVariable Slot2 { get; set; }

        /// <summary>
        /// Custom variable in slot 3.
        /// </summary>
        public ICustomVariable Slot3 { get; set; }

        /// <summary>
        /// Custom variable in slot 4.
        /// </summary>
        public ICustomVariable Slot4 { get; set; }

        /// <summary>
        /// Custom variable in slot 5.
        /// </summary>
        public ICustomVariable Slot5 { get; set; }
    }
}