﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
using System;
using CSharpAnalytics.Activities;
using CSharpAnalytics.Protocols.Measurement;
using CSharpAnalytics.Protocols.Urchin;

namespace CSharpAnalytics.Activities
{
    /// <summary>
    /// Captures the details of a campaign to be recorded in analytics.
    /// </summary>
    public class CampaignActivity : IUrchinActivity, IMeasurementActivity
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
        /// <summary>
        /// Capture the details of a campaign that will be sent to analytics.
        /// </summary>
        /// <param name="analyticsClient">UrchinAnalyticsClient object with queue and configuration set-up.</param>
        /// <param name="source">Source of the campaign.</param>
        /// <param name="name">Optional name of the campaign.</param>
        /// <param name="medium">Optional type of campaign.</param>
        /// <param name="term">Optional keyword terms for this campaign.</param>
        /// <param name="content">Optional content such as the specific link or content item for this campaign.</param>
        public static void TrackCampaign(this UrchinAnalyticsClient analyticsClient, string source, string name = null, string medium = null, string term = null, string content = null)
        {
            if (analyticsClient == null) throw new ArgumentNullException("analyticsClient");
            analyticsClient.Track(new CampaignActivity(source) { Name = name, Medium = medium, Term = term, Content = content });
        }

        /// <summary>
        /// Capture the details of a campaign that will be sent to analytics.
        /// </summary>
        /// <param name="analyticsClient">MeasurementAnalyticsClient object with queue and configuration set-up.</param>
        /// <param name="source">Source of the campaign.</param>
        /// <param name="name">Optional name of the campaign.</param>
        /// <param name="medium">Optional type of campaign.</param>
        /// <param name="term">Optional keyword terms for this campaign.</param>
        /// <param name="content">Optional content such as the specific link or content item for this campaign.</param>
        public static void TrackCampaign(this MeasurementAnalyticsClient analyticsClient, string source, string name = null, string medium = null, string term = null, string content = null)
        {
            if (analyticsClient == null) throw new ArgumentNullException("analyticsClient");
            analyticsClient.Track(new CampaignActivity(source) { Name = name, Medium = medium, Term = term, Content = content });
        }
    }
}