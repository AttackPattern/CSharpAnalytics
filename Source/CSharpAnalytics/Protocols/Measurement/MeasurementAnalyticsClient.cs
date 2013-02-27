﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
using System;
using CSharpAnalytics.Activities;
using CSharpAnalytics.Sessions;

namespace CSharpAnalytics.Protocols.Measurement
{
    /// <summary>
    /// Google Analytics Measurement Protocol client should exist for the scope of your application and is the primary entry point for tracking.
    /// </summary>
    public class MeasurementAnalyticsClient
    {
        private readonly SessionManager sessionManager;
        private readonly Action<Uri> sender;        
        private readonly MeasurementTracker tracker;

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

            tracker = new MeasurementTracker(configuration, environment);
        }

        /// <summary>
        /// Track a activity in analytics.
        /// </summary>
        /// <param name="activity">Activity to track in analytics.</param>
        public void Track(IMeasurementActivity activity)
        {
            if (activity is AutoTimedEventActivity)
                ((AutoTimedEventActivity)activity).End();

            sessionManager.Hit();
            var trackingUri = tracker.CreateUri(activity);
            sender(trackingUri);            
        }
    }
}