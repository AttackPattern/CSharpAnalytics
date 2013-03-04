﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
using System;
using CSharpAnalytics.Activities;
using System.Diagnostics;
using CSharpAnalytics.Protocols.Urchin;

namespace CSharpAnalytics.Activities
{
    /// <summary>
    /// Captures the details of a page view to be recorded in analytics.
    /// </summary>
    [DebuggerDisplay("PageView {Title} [{Page}]")]
    public class PageViewActivity : IUrchinActivity
    {
        private readonly string page;
        private readonly string title;

        /// <summary>
        /// Relative path of the page being viewed.
        /// </summary>
        public string Page
        {
            get { return page; }
        }

        /// <summary>
        /// Title of the page being viewed.
        /// </summary>
        public string Title
        {
            get { return title; }
        }

        /// <summary>
        /// Create a new PageViewActivity for a given page and title.
        /// </summary>
        /// <param name="title">Title of the page.</param>
        /// <param name="page">Relative path of the page.</param>
        public PageViewActivity(string title, string page)
        {
            this.title = title;
            this.page = page;
        }
    }
}

namespace CSharpAnalytics
{
    public static class PageViewExtensions
    {
        /// <summary>
        /// Track a new PageView for a given page and title.
        /// </summary>
        /// <param name="analyticsClient">UrchinAnalyticsClient object with queue and configuration set-up.</param>
        /// <param name="title">Title of the page.</param>
        /// <param name="page">Relative path of the page.</param>
        public static void TrackPageView(this UrchinAnalyticsClient analyticsClient, string title, string page)
        {
            if (analyticsClient == null) throw new ArgumentNullException("analyticsClient");
            analyticsClient.Track(new PageViewActivity(title, page));
        }
    }
}