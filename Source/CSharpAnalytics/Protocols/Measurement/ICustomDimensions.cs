// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;

namespace CSharpAnalytics.Protocols.Measurement
{
    public interface ICustomDimensions
    {
        /// <summary>
        /// Set the value of a custom dimension.
        /// </summary>
        /// <remarks>
        /// These need to be configured first in Google Analytics.
        /// </remarks>
        /// <param name="index">Index of the custom dimension the value is for.</param>
        /// <param name="value">Value for the custom dimension specified by the index.</param>
        void SetCustomDimension(int index, string value);

        /// <summary>
        /// Set the value of a custom dimension.
        /// </summary>
        /// <remarks>
        /// These need to be configured first in Google Analytics.
        /// This overide allows you to use an enum instead of integers for the index.
        /// </remarks>
        /// <param name="index">Index of the custom dimension the value is for.</param>
        /// <param name="value">Value for the custom dimension specified by the index.</param>
        void SetCustomDimension(Enum index, string value);
    }
}