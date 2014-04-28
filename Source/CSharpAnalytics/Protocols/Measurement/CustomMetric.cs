// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

namespace CSharpAnalytics.Protocols.Measurement
{
    /// <summary>
    /// Contains the details of a custom metric measurement.
    /// </summary>
    internal class CustomMetric
    {
        private readonly int index;
        private readonly object metric;

        /// <summary>
        /// Create a new custom metric.
        /// </summary>
        /// <param name="index">Index of this custom metric as defined in Google Analytics.</param>
        /// <param name="metric">Value of this custom metric to be reported.</param>
        public CustomMetric(int index, string metric)
        {
            this.index = index;
            this.metric = metric;
        }

        /// <summary>
        /// Index of this custom metric as defined in Google Analytics.
        /// </summary>
        public int Index { get { return index; } }

        /// <summary>
        /// Value of this custom metric to be reported.
        /// </summary>
        public object Metric { get { return metric; } }
    }
}