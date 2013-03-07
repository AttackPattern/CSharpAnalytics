﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System.Diagnostics;

namespace CSharpAnalytics.CustomVariables
{
    /// <summary>
    /// Captures the details of a basic custom variable.
    /// </summary>
    [DebuggerDisplay("{Name}={Value}")]
    public class CustomVariable : ICustomVariable
    {
        private readonly string name;
        private readonly string value;

        /// <summary>
        /// Name of this custom variable.
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Value of this custom variable.
        /// </summary>
        public string Value { get { return value; } }

        /// <summary>
        /// Create a new evaluated custom variable with a given name and value evaluator.
        /// </summary>
        /// <param name="name">Name of this custom variable to be assigned to the name property.</param>
        /// <param name="value">Value of this custom variable to be assigned to the Value property.</param>
        public CustomVariable(string name, string value)
        {
            this.name = name;
            this.value = value;
        }
    }
}