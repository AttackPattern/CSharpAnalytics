﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
using System;
using CSharpAnalytics.Activities;

namespace CSharpAnalytics.Activities
{
    /// <summary>
    /// Captures the details of a campaign to be recorded in analytics.
    /// </summary>
    public class CampaignActivity : ActivityBase
    {
        private readonly string source;

        /// <summary>
        /// Campaign source such as a specific search engine, newsletter etc. 
        /// </summary>
        /// <example>
        /// Google, NewsletterJan2012.
        /// </example>
        public string Source
        {
            get { return source; }
        }

        /// <summary>
        /// Whether this is a new visit or not.
        /// </summary>
        public bool IsNewVisit { get; set; }

        /// <summary>
        /// Name of the AdWords campaign or a custom campaign.
        /// </summary>
        /// <remarks>
        /// Appears as "Source" under analytics reports segments.
        /// </remarks>
        public string Name { get; set; }

        /// <summary>
        /// Type of campaign.
        /// </summary>
        /// <example>
        /// Banner, Email, CPC.
        /// </example>
        /// <remarks>
        /// Appears as "Keyword" under analytics reports segments.
        /// </remarks>
        public string Medium { get; set; }

        /// <summary>
        /// Keyword terms for this campaign.
        /// </summary>
        /// <remarks>
        /// Appears as "Keyword" under analytics reports segments.
        /// </remarks>
        public string Term { get; set; }

        /// <summary>
        /// Define specific link or content item in a custom campaign.
        /// </summary>
        /// <remarks>
        /// Appears as "Ad Content" under analytics reports segments.
        /// </remarks>
        public string Content { get; set; }

        /// <summary>
        /// Create a new CampaignActivity to capture details of the campaign for tracking.
        /// </summary>
        /// <param name="source">Campaign source.</param>
        public CampaignActivity(string source)
        {
            this.source = source;
        }
    }
}

namespace CSharpAnalytics
{
    public static class CampaignExtensions
    {
        public static void TrackCampaign(this IAnalyticsClient analyticsClient, string source, string name = null, string medium = null, string term = null, string content = null)
        {
            if (analyticsClient == null) throw new ArgumentNullException("analyticsClient");
            analyticsClient.Track(new CampaignActivity(source) { Name = name, Medium = medium, Term = term, Content = content });
        }
    }
}