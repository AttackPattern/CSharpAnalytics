﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using CSharpAnalytics.Activities;
using CSharpAnalytics.Protocols.Measurement;

namespace CSharpAnalytics.Activities
{
    /// <summary>
    /// Captures the details of a view displayed within an application.
    /// </summary>
    public class AppViewActivity : ContentViewActivity
    {
        /// <summary>
        /// Create a new AppViewActivity to capture details of this application view.
        /// </summary>
        /// <param name="screenName">Name of the screen being viewed.</param>
        public AppViewActivity(string screenName)
            : base(null, null, screenName)
        {
        }

        /// <summary>
        /// Name of the screen being viewed.
        /// </summary>
        public string ScreenName { get { return ContentDescription; } }
    }
}

namespace CSharpAnalytics
{
    public static class AppViewExtensions
    {
        /// <summary>
        /// Capture the details of an application view event that will be sent to analytics.
        /// </summary>
        /// <param name="analyticsClient"></param>
        /// <param name="screenName"></param>
        public static void TrackAppView(this MeasurementAnalyticsClient analyticsClient, string screenName)
        {
            if (analyticsClient == null) throw new ArgumentNullException("analyticsClient");
            analyticsClient.Track(new AppViewActivity(screenName));
        }
    }
}