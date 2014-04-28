// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;

namespace CSharpAnalytics.Protocols.Measurement
{
    public interface ICustomMetrics
    {
        /// <summary>
        /// Set the integer value of a custom metric.
        /// </summary>
        /// <remarks>
        /// These need to be configured first in Google Analytics.
        /// </remarks>
        /// <param name="index">Index of the custom metric the value is for.</param>
        /// <param name="value">Integer value for the custom metric specified by the index.</param>
        void SetCustomMetric(int index, long value);

        /// <summary>
        /// Set the time value of a custom metric.
        /// </summary>
        /// <remarks>
        /// These need to be configured first in Google Analytics.
        /// </remarks>
        /// <param name="index">Index of the custom metric the value is for.</param>
        /// <param name="value">Time value for the custom metric specified by the index.</param>
        void SetCustomMetric(int index, TimeSpan value);

        /// <summary>
        /// Set the financial value of a custom metric.
        /// </summary>
        /// <remarks>
        /// These need to be configured first in Google Analytics.
        /// </remarks>
        /// <param name="index">Index of the custom metric the value is for.</param>
        /// <param name="value">Financial value for the custom metric specified by the index.</param>
        void SetCustomMetric(int index, decimal value);
    }
}