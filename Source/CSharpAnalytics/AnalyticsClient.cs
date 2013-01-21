﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
using System;
using CSharpAnalytics.Activities;
using CSharpAnalytics.CustomVariables;
using CSharpAnalytics.Sessions;
using CSharpAnalytics.Protocols.Urchin;

namespace CSharpAnalytics
{
    /// <summary>
    /// Analytics client that should exist for the scope of your application and is the primary entry point for tracking.
    /// </summary>
    public class AnalyticsClient
    {
        private readonly string hostName;
        private readonly UrchinTracker tracker;
        private readonly Action<Uri> sender;
        private readonly SessionManager sessionManager;

        /// <summary>
        /// Create a new AnalyticsClient with a given configuration, session, environment and URI sender.
        /// </summary>
        /// <param name="configuration">Configuration of this Google Analytics client.</param>
        /// <param name="sessionManager">Session manager with Visitor and session information.</param>
        /// <param name="environment">Provider of environmental information such as screen resolution.</param>
        /// <param name="sender">Action to take prepared URIs for Google Analytics and send them on.</param>
        public AnalyticsClient(Configuration configuration, SessionManager sessionManager, IEnvironment environment, Action<Uri> sender)
        {
            this.sessionManager = sessionManager;
            this.sender = sender;
            tracker = new UrchinTracker(configuration, sessionManager, environment);
            hostName = configuration.HostName;
        }

        /// <summary>
        /// Custom variables currently declared for this Visitor.
        /// </summary>
        public ScopedCustomVariableSlots VisitorCustomVariables = new ScopedCustomVariableSlots(CustomVariableScope.Visitor);

        /// <summary>
        /// Custom variables currently declared for this session.
        /// </summary>
        public ScopedCustomVariableSlots SessionCustomVariables = new ScopedCustomVariableSlots(CustomVariableScope.Session);

        /// <summary>
        /// Track an activity in analytics by recording the details and encoding them into a URI to be sent via the sender.
        /// </summary>
        /// <param name="activity">Activity to be recorded in analytics.</param>
        public void Track(IActivity activity)
        {
            if (activity is AutoTimedEventActivity)
                ((AutoTimedEventActivity)activity).End();

            sessionManager.Hit();
            var trackingUri = tracker.CreateUri(activity, new [] { VisitorCustomVariables, SessionCustomVariables });
            sender(trackingUri);

            if (activity is PageViewActivity)
                sessionManager.Referrer = new Uri("http://" + hostName + ((PageViewActivity)activity).Page);
        }
    }
}