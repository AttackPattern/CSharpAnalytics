﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Diagnostics;

namespace CSharpAnalytics.Protocols.Urchin.CustomVariables
{
    /// <summary>
    /// Captures the name and a function that when called will provide the value.
    /// Useful for dynamically changing values like counters, timings or external factors.
    /// </summary>
    [DebuggerDisplay("Name={Name}")]
    public class EvaluatedCustomVariable : ICustomVariable
    {
        private readonly string name;
        private readonly Func<string> valueEvaluator;

        /// <summary>
        /// Name of this custom variable.
        /// </summary>
        public string Name { get { return name; } }

        /// <summary>
        /// Function that will provide the value of this custom variable when required.
        /// </summary>
        public string Value { get { return valueEvaluator(); } }

        /// <summary>
        /// Create a new evaluated custom variable with a given name and value evaluator.
        /// </summary>
        /// <param name="name">name of this custom variable to be assigned to the name property.</param>
        /// <param name="valueEvaluator">Value evaluator function to be assigned to the Value property.</param>
        public EvaluatedCustomVariable(string name, Func<string> valueEvaluator)
        {
            this.name = name;
            this.valueEvaluator = valueEvaluator;
        }
    }
}