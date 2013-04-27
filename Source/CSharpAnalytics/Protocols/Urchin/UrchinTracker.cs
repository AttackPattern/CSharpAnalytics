﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using CSharpAnalytics.Activities;
using CSharpAnalytics.Sessions;
using System;

namespace CSharpAnalytics.Protocols.Urchin
{
    /// <summary>
    /// UrchinTracker coordinates the tracking of IUrchinActivity with the environment, session and sender.
    /// </summary>
    internal class UrchinTracker
    {
        private readonly string hostName;
        private readonly SessionManager sessionManager;
        private readonly Action<Uri> sender;
        private readonly UrchinUriBuilder tracker;

        /// <summary>
        /// Create a new AnalyticsClient with a given configuration, session, environment and URI sender.
        /// </summary>
        /// <param name="configuration">Configuration of this Google Analytics Urchin client.</param>
        /// <param name="sessionManager">Session manager with visitor and session information.</param>
        /// <param name="environment">Provider of environmental information such as screen resolution.</param>
        /// <param name="sender">Action to take prepared URIs for Google Analytics and send them on.</param>
        public UrchinTracker(UrchinConfiguration configuration, SessionManager sessionManager, IEnvironment environment, Action<Uri> sender)
        {
            this.sessionManager = sessionManager;
            this.sender = sender;
            tracker = new UrchinUriBuilder(configuration, sessionManager, environment);
            hostName = configuration.HostName;
        }

        /// <summary>
        /// Track an activity in analytics.
        /// </summary>
        /// <param name="entry">UrchinActivityEntry to track in analytics.</param>
        public void Track(UrchinActivityEntry entry)
        {
            sessionManager.Hit();
            var trackingUri = tracker.BuildUri(entry);
            sender(trackingUri);
            if (entry.Activity is PageViewActivity)
                sessionManager.Referrer = new Uri("http://" + hostName + ((PageViewActivity)entry.Activity).Page);
        }
    }
}