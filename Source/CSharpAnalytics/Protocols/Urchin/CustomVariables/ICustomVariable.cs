﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

namespace CSharpAnalytics.Protocols.Urchin.CustomVariables
{
    /// <summary>
    /// Interface for classes capable of acting as a custom variable by providing name and value.
    /// </summary>
    public interface ICustomVariable
    {
        /// <summary>
        /// Name of a custom variable.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Value of a custom variable.
        /// </summary>
        string Value { get; }
    }
}