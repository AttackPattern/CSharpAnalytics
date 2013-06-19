﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using CSharpAnalytics.Activities;
using CSharpAnalytics.Protocols.Measurement;

namespace CSharpAnalytics
{
    public static class MeasurementActivityExtensions
    {
        /// <summary>
        /// Capture the details of an application view event that will be sent to analytics.
        /// </summary>
        /// <param name="analyticsClient"></param>
        /// <param name="screenName"></param>
        public static void TrackAppView(this MeasurementAnalyticsClient analyticsClient, string screenName)
        {
            if (analyticsClient == null) throw new ArgumentNullException("analyticsClient");
            if (String.IsNullOrWhiteSpace(screenName)) throw new ArgumentException("Screen name must not be null or blank", "screenName");
            analyticsClient.Track(new AppViewActivity(screenName));
        }

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

        /// <summary>
        /// Capture the details of an event that will be sent to analytics.
        /// </summary>
        /// <param name="analyticsClient">MeasurementAnalyticsClient object with queue and configuration set-up.</param>
        /// <param name="action">Action name of the event to send.</param>
        /// <param name="category">Category of the event to send.</param>
        /// <param name="label">Optional label name of the event to send.</param>
        /// <param name="value">Optional numeric value of the event to send.</param>
        /// <param name="nonInteraction">Optional boolean value to be assigned to the NonInteraction property.</param>
        public static void TrackEvent(this MeasurementAnalyticsClient analyticsClient, string action, string category, string label = null, int? value = null, bool nonInteraction = false)
        {
            if (analyticsClient == null) throw new ArgumentNullException("analyticsClient");
            analyticsClient.Track(new EventActivity(action, category, label, value, nonInteraction));
        }

        /// <summary>
        /// Capture the details of an event that will be sent to analytics.
        /// </summary>
        /// <param name="analyticsClient">MeasurementAnalyticsClient object with queue and configuration set-up.</param>
        /// <param name="description">Description of the exception.</param>
        /// <param name="isFatal">Optional whether the exception was fatal (caused the app to crash), defaults to false.</param>
        public static void TrackException(this MeasurementAnalyticsClient analyticsClient, string description, bool isFatal = false)
        {
            if (analyticsClient == null) throw new ArgumentNullException("analyticsClient");
            analyticsClient.Track(new ExceptionActivity(description, isFatal));
        }

        /// <summary>
        /// Track a social activity being performed.
        /// </summary>
        /// <param name="analyticsClient">MeasurementAnalyticsClient object with queue and configuration set-up.</param>
        /// <param name="action">Social action being performed.</param>
        /// <param name="network">Name of the social network being acted upon.</param>
        /// <param name="pagePath">Optional path of the page the action occured on.</param>
        /// <param name="target">Optional target resource being acted upon.</param>
        public static void TrackSocial(this MeasurementAnalyticsClient analyticsClient, string action, string network, string target = null, string pagePath = null)
        {
            if (analyticsClient == null) throw new ArgumentNullException("analyticsClient");
            analyticsClient.Track(new SocialActivity(action, network, pagePath, target));
        }

        /// <summary>
        /// Capture the details of a timed event that will be sent to analytics.
        /// </summary>
        /// <param name="analyticsClient">MeasurementAnalyticsClient object with queue and configuration set-up.</param>
        /// <param name="category">Category of the event to send.</param>
        /// <param name="variable">Variable name of the event to send.</param>
        /// <param name="time">Time of the event to send.</param>
        /// <param name="label">Optional label name of the event to send.</param>
        public static void TrackTimedEvent(this MeasurementAnalyticsClient analyticsClient, string category, string variable, TimeSpan time, string label = null)
        {
            if (analyticsClient == null) throw new ArgumentNullException("analyticsClient");
            analyticsClient.Track(new TimedEventActivity(category, variable, time, label));
        }
    }
}