﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Diagnostics;

namespace CSharpAnalytics.Protocols.Urchin.CustomVariables
{
    [DebuggerDisplay("Scope={Scope}, Variable={Variable}")]
    internal class ScopedCustomVariableSlot
    {
        private readonly ICustomVariable variable;
        private readonly CustomVariableScope scope;
        private readonly int slot;

        public ScopedCustomVariableSlot(CustomVariableScope scope, ICustomVariable variable, int slot)
        {
            if (!Enum.IsDefined(typeof(CustomVariableScope), scope))
                throw new ArgumentOutOfRangeException("scope");

            this.scope = scope;
            this.variable = variable;
            this.slot = slot;
        }

        public CustomVariableScope Scope
        {
            get { return scope; }
        }

        public ICustomVariable Variable
        {
            get { return variable; }
        }

        public int Slot
        {
            get { return slot; }
        }
    }
}