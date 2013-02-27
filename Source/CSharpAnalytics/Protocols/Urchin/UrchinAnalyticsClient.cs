﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
using System;
using CSharpAnalytics.Activities;
using CSharpAnalytics.CustomVariables;
using CSharpAnalytics.Sessions;

namespace CSharpAnalytics.Protocols.Urchin
{
    /// <summary>
    /// Analytics client that should exist for the scope of your application and is the primary entry point for tracking.
    /// </summary>
    public class UrchinAnalyticsClient : IAnalyticsClient
    {
        private readonly SessionManager sessionManager;
        private readonly Action<Uri> sender;
        private readonly string hostName;
        private readonly UrchinTracker tracker;

        /// <summary>
        /// Create a new AnalyticsClient with a given configuration, session, environment and URI sender.
        /// </summary>
        /// <param name="configuration">Configuration of this Google Analytics client.</param>
        /// <param name="sessionManager">Session manager with Visitor and session information.</param>
        /// <param name="environment">Provider of environmental information such as screen resolution.</param>
        /// <param name="sender">Action to take prepared URIs for Google Analytics and send them on.</param>
        public UrchinAnalyticsClient(UrchinConfiguration configuration, SessionManager sessionManager, IEnvironment environment, Action<Uri> sender)
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

        public void Track(IActivity activity)
        {
            if (activity is AutoTimedEventActivity)
                ((AutoTimedEventActivity)activity).End();

            sessionManager.Hit();
            var trackingUri = tracker.CreateUri(activity, new[] { VisitorCustomVariables, SessionCustomVariables });
            sender(trackingUri);
            
            if (activity is PageViewActivity)
                sessionManager.Referrer = new Uri("http://" + hostName + ((PageViewActivity)activity).Page);
        }
    }
}