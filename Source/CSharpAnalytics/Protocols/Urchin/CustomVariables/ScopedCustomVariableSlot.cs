﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Diagnostics;

namespace CSharpAnalytics.Protocols.Urchin.CustomVariables
{
    /// <summary>
    /// Represents a custom variable along with it's slot index and scope.
    /// </summary>
    [DebuggerDisplay("Scope={Scope}, Variable={Variable}")]
    internal class ScopedCustomVariableSlot
    {
        private readonly ICustomVariable variable;
        private readonly CustomVariableScope scope;
        private readonly int slot;

        /// <summary>
        /// Create a new ScopedCustomVariableSlot with a given variable, scope and slot index.
        /// </summary>
        /// <param name="scope">Scope of this custom variable.</param>
        /// <param name="variable">Custom variable.</param>
        /// <param name="slot">Slot index for this custom variable.</param>
        public ScopedCustomVariableSlot(CustomVariableScope scope, ICustomVariable variable, int slot)
        {
            if (!Enum.IsDefined(typeof(CustomVariableScope), scope))
                throw new ArgumentOutOfRangeException("scope");

            this.scope = scope;
            this.variable = variable;
            this.slot = slot;
        }

        /// <summary>
        /// Scope that this custom variable.
        /// </summary>
        /// <example>
        /// Session
        /// </example>
        public CustomVariableScope Scope
        {
            get { return scope; }
        }

        /// <summary>
        /// Custom variable.
        /// </summary>
        public ICustomVariable Variable
        {
            get { return variable; }
        }

        /// <summary>
        /// Slot index for this custom variable.
        /// </summary>
        public int Slot
        {
            get { return slot; }
        }
    }
}