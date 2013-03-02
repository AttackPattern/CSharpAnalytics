﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
using System;
using CSharpAnalytics.Activities;
using System.Diagnostics;
using CSharpAnalytics.Protocols.Measurement;

namespace CSharpAnalytics.Activities
{
    /// <summary>
    /// Captures the details of a page view to be recorded in analytics.
    /// </summary>
    [DebuggerDisplay("ContentView {Title} [{Page}]")]
    public class ContentViewActivity : IMeasurementActivity
    {
        private readonly Uri documentLocation;

        public Uri DocumentLocation
        {
            get { return documentLocation; }
        }

        private readonly string documentHostName;

        public string DocumentHostName
        {
            get { return documentHostName; }
        }

        private readonly string documentPath;

        public string DocumentPath
        {
            get { return documentPath; }
        }

        private readonly string documentTitle;

        public string DocumentTitle
        {
            get { return documentTitle; }
        }

        private readonly string contentDescription;

        public string ContentDescription { get { return contentDescription; } }

        public ContentViewActivity(Uri documentLocation, string documentTitle, string contentDescription = null, string documentPath = null, string documentHostName = null)
        {
            this.documentLocation = documentLocation;
            this.documentTitle = documentTitle;
            this.contentDescription = contentDescription;
            this.documentPath = documentPath;
            this.documentHostName = documentHostName;
        }
    }
}

namespace CSharpAnalytics
{
    public static class ContentViewExtensions
    {
        /// <summary>
        /// Track a new AppView for a given view.
        /// </summary>
        /// <param name="analyticsClient">AnalyticsClient currently configured.</param>
        /// <param name="name">Name of the view.</param>
        public static void TrackAppView(this MeasurementAnalyticsClient analyticsClient, string name)
        {
            if (analyticsClient == null) throw new ArgumentNullException("analyticsClient");
            analyticsClient.Track(new AppViewActivity(name));
        }
    }
}