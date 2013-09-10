﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System.Collections.Generic;

namespace CSharpAnalytics.Protocols.Measurement
{
    /// <summary>
    /// Captures a measurement activity and its associated data for later tracking.
    /// </summary>
    internal class MeasurementActivityEntry
    {
        /// <summary>
        /// The activity this entry should track.
        /// </summary>
        public IMeasurementActivity Activity;

        /// <summary>
        /// Any custom dimensions associated with this activity.
        /// </summary>
        public IEnumerable<KeyValuePair<int, string>> CustomDimensions;

        /// <summary>
        /// Any custom metrics associated with this activity.
        /// </summary>
        public IEnumerable<KeyValuePair<int, object>> CustomMetrics;

        /// <summary>
        /// Whether the session should end with this activity.
        /// </summary>
        public bool EndSession;

        /// <summary>
        /// Create a new MeasurementActivityEntry given an activity to capture.
        /// </summary>
        /// <param name="activity">Measurement activity to capture.</param>
        public MeasurementActivityEntry(IMeasurementActivity activity)
        {
            Activity = activity;
        }
    }
}