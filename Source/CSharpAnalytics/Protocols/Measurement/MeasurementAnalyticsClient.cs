﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using CSharpAnalytics.Activities;
using CSharpAnalytics.Sessions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace CSharpAnalytics.Protocols.Measurement
{
    /// <summary>
    /// MeasurementAnalyticsClient should exist for the scope of your application and is the primary entry point for tracking via Measurement Protocol.
    /// </summary>
    public class MeasurementAnalyticsClient
    {
        private readonly Dictionary<int, string> customDimensions = new Dictionary<int, string>();
        private readonly Dictionary<int, long?> customMetrics = new Dictionary<int, long?>();
        private readonly Queue<MeasurementActivityEntry> queue = new Queue<MeasurementActivityEntry>();

        private MeasurementTracker tracker;

        public void Configure(MeasurementConfiguration configuration, SessionManager sessionManager, IEnvironment environment, Action<Uri> sender)
        {
            Debug.Assert(tracker == null);
            var newTracker = new MeasurementTracker(configuration, sessionManager, environment, sender);
            while (queue.Count > 0)
                newTracker.Track(queue.Dequeue());
            tracker = newTracker;
        }

        /// <summary>
        /// Track a activity in analytics.
        /// </summary>
        /// <param name="activity">Activity to track in analytics.</param>
        /// <param name="endSession">True if this tracking event should end the session.</param>
        public void Track(IMeasurementActivity activity, bool endSession = false)
        {
            if (activity is AutoTimedEventActivity)
                ((AutoTimedEventActivity)activity).End();

            var entry = new MeasurementActivityEntry(activity)
            {
                CustomDimensions = customDimensions.ToArray(),
                CustomMetrics = customMetrics.ToArray(),
                EndSession = endSession
            };

            customDimensions.Clear();
            customMetrics.Clear();

            if (tracker == null)
                queue.Enqueue(entry);
            else
                tracker.Track(entry);
        }

        /// <summary>
        /// Set the value of a custom dimension to be set with the next activity.
        /// </summary>
        /// <remarks>
        /// These need to be configured first in Google Analytics.
        /// </remarks>
        /// <param name="index">Index of the custom dimension the value is for.</param>
        /// <param name="value">Value for the custom dimension specified by the index.</param>
        public void SetCustomDimension(int index, string value)
        {
            customDimensions[index] = value;
        }

        /// <summary>
        /// Set the value of a custom dimension to be set with the next activity.
        /// </summary>
        /// <remarks>
        /// These need to be configured first in Google Analytics.
        /// This overide allows you to use an enum instead of integers for the index.
        /// </remarks>
        /// <param name="index">Index of the custom dimension the value is for.</param>
        /// <param name="value">Value for the custom dimension specified by the index.</param>
        public void SetCustomDimension(Enum index, string value)
        {
            ValidateEnum(index);
            SetCustomDimension(Convert.ToInt32(index, CultureInfo.InvariantCulture), value);
        }

        /// <summary>
        /// Set the value of a custom metric to be set with the next activity.
        /// </summary>
        /// <remarks>
        /// These need to be configured first in Google Analytics.
        /// </remarks>
        /// <param name="index">Index of the custom metric the value is for.</param>
        /// <param name="value">Value for the custom metric specified by the index.</param>
        public void SetCustomMetric(int index, long? value)
        {
            customMetrics[index] = value;
        }

        /// <summary>
        /// Set the value of a custom dimension to be set with the next activity.
        /// </summary>
        /// <remarks>
        /// These need to be configured first in Google Analytics.
        /// This overide allows you to use an enum instead of integers for the index.
        /// </remarks>
        /// <param name="index">Index of the custom dimension the value is for.</param>
        /// <param name="value">Value for the custom dimension specified by the index.</param>
        public void SetCustomMetric(Enum index, long? value)
        {
            ValidateEnum(index);
            SetCustomMetric(Convert.ToInt32(index, CultureInfo.InvariantCulture), value);
        }

        /// <summary>
        /// Validate an enum to ensure it is defined and has an underlying int type throwing
        /// an exception if it does not.
        /// </summary>
        /// <param name="index">Enum to check.</param>
        private static void ValidateEnum(Enum index)
        {
            if (Enum.GetUnderlyingType(index.GetType()) != typeof(int))
                throw new ArgumentException("Enum must be of type int", "index");

            if (!Enum.IsDefined(index.GetType(), index))
                throw new ArgumentOutOfRangeException("index", "Enum value is not defined");
        }
    }
}