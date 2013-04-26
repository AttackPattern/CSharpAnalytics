﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System.Diagnostics;

namespace CSharpAnalytics.Protocols.Urchin.CustomVariables
{
    [DebuggerDisplay("Scope={Scope}, Variable={Variable}")]
    internal class ScopedCustomVariable
    {
        private readonly ICustomVariable variable;
        private readonly CustomVariableScope scope;

        public ScopedCustomVariable(CustomVariableScope scope, ICustomVariable variable)
        {
            this.scope = scope;
            this.variable = variable;
        }

        public CustomVariableScope Scope
        {
            get { return scope; }
        }

        public ICustomVariable Variable
        {
            get { return variable; }
        }
    }
}