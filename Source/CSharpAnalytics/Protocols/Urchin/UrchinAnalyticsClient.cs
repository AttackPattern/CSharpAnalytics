﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using CSharpAnalytics.Activities;
using CSharpAnalytics.CustomVariables;
using CSharpAnalytics.Sessions;
using System;

namespace CSharpAnalytics.Protocols.Urchin
{
    /// <summary>
    /// UrchinAnalyticsClient should exist for the scope of your application and is the primary entry point for tracking via Urchin.
    /// </summary>
    public class UrchinAnalyticsClient
    {
        private readonly SessionManager sessionManager;
        private readonly Action<Uri> sender;
        private readonly string hostName;
        private readonly UrchinUriBuilder tracker;

        /// <summary>
        /// Create a new AnalyticsClient with a given configuration, session, environment and URI sender.
        /// </summary>
        /// <param name="configuration">Configuration of this Google Analytics Urchin client.</param>
        /// <param name="sessionManager">Session manager with visitor and session information.</param>
        /// <param name="environment">Provider of environmental information such as screen resolution.</param>
        /// <param name="sender">Action to take prepared URIs for Google Analytics and send them on.</param>
        public UrchinAnalyticsClient(UrchinConfiguration configuration, SessionManager sessionManager, IEnvironment environment, Action<Uri> sender)
        {
            this.sessionManager = sessionManager;
            this.sender = sender;

            tracker = new UrchinUriBuilder(configuration, sessionManager, environment);
            hostName = configuration.HostName;
        }

        /// <summary>
        /// Custom variables currently declared for this visitor.
        /// </summary>
        public ScopedCustomVariableSlots VisitorCustomVariables = new ScopedCustomVariableSlots(CustomVariableScope.Visitor);

        /// <summary>
        /// Custom variables currently declared for this session.
        /// </summary>
        public ScopedCustomVariableSlots SessionCustomVariables = new ScopedCustomVariableSlots(CustomVariableScope.Session);

        /// <summary>
        /// Track this activity in analytics.
        /// </summary>
        /// <param name="activity">Activity to track in analytics.</param>
        /// <param name="activityCustomVariables">Activity scoped custom variable slots to record for this activity.</param>
        public void Track(IUrchinActivity activity, ScopedCustomVariableSlots activityCustomVariables = null)
        {
            if (activityCustomVariables != null && activityCustomVariables.Scope != CustomVariableScope.Activity)
                throw new ArgumentException("Custom variable slots must be scoped to activity", "activityCustomVariables");

            if (activity is AutoTimedEventActivity)
                ((AutoTimedEventActivity)activity).End();

            sessionManager.Hit();
            var trackingUri = tracker.CreateUri(activity, new[] { VisitorCustomVariables, SessionCustomVariables, activityCustomVariables });
            sender(trackingUri);

            if (activity is PageViewActivity)
                sessionManager.Referrer = new Uri("http://" + hostName + ((PageViewActivity)activity).Page);
        }
    }
}