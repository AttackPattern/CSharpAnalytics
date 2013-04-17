﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using CSharpAnalytics.Activities;
using CSharpAnalytics.Sessions;

namespace CSharpAnalytics.Protocols.Measurement
{
    /// <summary>
    /// MeasurementAnalyticsClient should exist for the scope of your application and is the primary entry point for tracking via Measurement Protocol.
    /// </summary>
    public class MeasurementAnalyticsClient
    {
        private readonly SessionManager sessionManager;
        private readonly Action<Uri> sender;
        private readonly MeasurementUriBuilder tracker;

        /// <summary>
        /// Create a new AnalyticsClient with a given configuration, session, environment and URI sender.
        /// </summary>
        /// <param name="configuration">Configuration of this Google Analytics Measurement Protocol client.</param>
        /// <param name="sessionManager">Session manager with visitor and session information.</param>
        /// <param name="environment">Provider of environmental information such as screen resolution.</param>
        /// <param name="sender">Action to take prepared URIs for Google Analytics and send them on.</param>
        public MeasurementAnalyticsClient(MeasurementConfiguration configuration, SessionManager sessionManager, IEnvironment environment, Action<Uri> sender)
        {
            this.sessionManager = sessionManager;
            this.sender = sender;

            tracker = new MeasurementUriBuilder(configuration, sessionManager, environment);
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

            sessionManager.Hit();
            if (endSession)
                sessionManager.End();
            var trackingUri = tracker.BuildUri(activity, customDimensions, customMetrics);
            sender(trackingUri);
        }

        private readonly Dictionary<int, string> customDimensions = new Dictionary<int, string>();
        private readonly Dictionary<int, long?> customMetrics = new Dictionary<int, long?>(); 

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
    }
}