﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

 namespace CSharpAnalytics.CustomVariables
{
    /// <summary>
    /// Interface for classes that can provide a name and value for custom variables.
    /// </summary>
    public interface ICustomVariable
    {
        /// <summary>
        /// name of a custom variable.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Value of a custom variable.
        /// </summary>
        string Value { get; }
    }
}