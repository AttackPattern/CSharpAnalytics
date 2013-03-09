﻿﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
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
        private readonly string documentHostName;
        private readonly string documentPath;
        private readonly string documentTitle;
        private readonly string contentDescription;

        /// <summary>
        /// Location of the document being viewed.
        /// </summary>
        public Uri DocumentLocation
        {
            get { return documentLocation; }
        }

        /// <summary>
        /// Host name of the document being viewed.
        /// </summary>
        public string DocumentHostName
        {
            get { return documentHostName; }
        }

        /// <summary>
        /// Path of the document being viewed.
        /// </summary>
        public string DocumentPath
        {
            get { return documentPath; }
        }

        /// <summary>
        /// Title of the document being viewed.
        /// </summary>
        public string DocumentTitle
        {
            get { return documentTitle; }
        }

        /// <summary>
        /// Description of the content being viewed.
        /// </summary>
        public string ContentDescription
        {
            get { return contentDescription; }
        }

        /// <summary>
        /// Create a ContentViewActivity to capture details of the content or document being viewed.
        /// </summary>
        /// <param name="documentLocation">Location of the document.</param>
        /// <param name="documentTitle">Title of the document.</param>
        /// <param name="contentDescription">Optional description of the content.</param>
        /// <param name="documentPath">Optional path of the content - will be determined by documentLocation if not provided.</param>
        /// <param name="documentHostName">Optional host name of the content - will be determined by documentLocation if not provided..</param>
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
    /// <summary>
    /// Extension methods for adding ContentViews to compatible analytics clients.
    /// </summary>
    public static class ContentViewExtensions
    {
        /// <summary>
        /// Track a ContentView activity for a given piece of content.
        /// </summary>
        /// <param name="analyticsClient">MeasurementAnalyticsClient object with queue and configuration set-up.</param>
        /// <param name="documentLocation">URI location of the document.</param>
        /// <param name="contentDescription">Description of the content.</param>
        /// <param name="documentPath">Optional path override of the document location.</param>
        /// <param name="documentHostName">Optional host name override of the document location.</param>
        public static void TrackContentView(this MeasurementAnalyticsClient analyticsClient, Uri documentLocation, string contentDescription, 
            string documentPath = null, string documentHostName = null)
        {
            if (analyticsClient == null) throw new ArgumentNullException("analyticsClient");
            analyticsClient.Track(new ContentViewActivity(documentLocation, contentDescription, documentPath, documentHostName));
        }
    }
}