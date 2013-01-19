﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
using CSharpAnalytics.CustomVariables;

namespace CSharpAnalytics.Activities
{
    public abstract class ActivityBase : IActivity
    {
        private readonly ScopedCustomVariableSlots customVariables = new ScopedCustomVariableSlots(CustomVariableScope.Activity);

        public ScopedCustomVariableSlots CustomVariables
        {
            get { return customVariables; }
        }
    }
}