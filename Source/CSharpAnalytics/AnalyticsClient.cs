﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
using System;
using CSharpAnalytics.Activities;
using CSharpAnalytics.Sessions;

namespace CSharpAnalytics
{
    /// <summary>
    /// Analytics client that should exist for the scope of your application and is the primary entry point for tracking.
    /// </summary>
    public abstract class AnalyticsClient
    {
        private readonly Action<Uri> sender;
        internal Func<IActivity, Uri> Tracker;
        protected readonly SessionManager SessionManager;

        /// <summary>
        /// Create a new AnalyticsClient with a given configuration, session, environment and URI sender.
        /// </summary>
        /// <param name="sessionManager">Session manager with Visitor and session information.</param>
        /// <param name="sender">Action to take prepared URIs for Google Analytics and send them on.</param>
        protected AnalyticsClient(SessionManager sessionManager, Action<Uri> sender)
        {
            SessionManager = sessionManager;
            this.sender = sender;
        }

        /// <summary>
        /// Track an activity in analytics by recording the details and encoding them into a URI to be sent via the sender.
        /// </summary>
        /// <param name="activity">Activity to be recorded in analytics.</param>
        public virtual void Track(IActivity activity)
        {
            if (activity is AutoTimedEventActivity)
                ((AutoTimedEventActivity)activity).End();

            SessionManager.Hit();
            var trackingUri = Tracker(activity);
            sender(trackingUri);
        }
    }
}