// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

namespace CSharpAnalytics.Protocols.Measurement
{
    /// <summary>
    /// Contains the details of a custom dimension measurement.
    /// </summary>
    internal class CustomDimension
    {
        private readonly int index;
        private readonly string dimension;

        /// <summary>
        /// Create a new custom dimension measurement.
        /// </summary>
        /// <param name="index">Index of this custom dimension as defined in Google Analytics.</param>
        /// <param name="dimension">Value of this custom dimension to be reported.</param>
        public CustomDimension(int index, string dimension)
        {
            this.index = index;
            this.dimension = dimension;
        }

        /// <summary>
        /// Index of this custom dimension as defined in Google Analytics.
        /// </summary>
        public int Index { get { return index; } }

        /// <summary>
        /// Value of this custom dimension to be reported.
        /// </summary>
        public string Dimension { get { return dimension; } }
    }
}