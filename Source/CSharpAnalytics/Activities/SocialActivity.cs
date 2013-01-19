﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
using System;
using CSharpAnalytics.Activities;
using System.Diagnostics;

namespace CSharpAnalytics.Activities
{
    /// <summary>
    /// Captures the details of an social action that has been performed.
    /// </summary>
    [DebuggerDisplay("Social {Action} on {Network}")]
    public class SocialActivity : ActivityBase
    {
        private readonly string action;
        private readonly string network;
        private readonly string pagePath;
        private readonly string target;

        /// <summary>
        /// Type of social action performed. e.g. Like, Share, Tweet
        /// </summary>
        public string Action
        {
            get { return action; }
        }

        /// <summary>
        /// Social network the action was performed on. e.g. Facebook, Twitter, LinkedIn
        /// </summary>
        public string Network
        {
            get { return network; }
        }

        /// <summary>
        /// Optional path of the page from which the action occured.
        /// </summary>
        public string PagePath
        {
            get { return pagePath; }
        }

        /// <summary>
        /// Optional resource being acted upon. e.g. Page being shared.
        /// </summary>
        public string Target
        {
            get { return target; }
        }

        /// <summary>
        /// Create a new SocialActivity with various parameters to be captured.
        /// </summary>
        /// <param name="action">Social action being performed.</param>
        /// <param name="network">Name of the social network being acted upon.</param>
        /// <param name="pagePath">Optional path of the page the action occured on.</param>
        /// <param name="target">Optional target resource being acted upon.</param>
        public SocialActivity(string action, string network, string pagePath = null, string target = null)
        {
            this.network = network;
            this.action = action;
            this.target = target;
            this.pagePath = pagePath;
        }
    }
}

namespace CSharpAnalytics
{
    public static class SocialExtensions
    {
        /// <summary>
        /// Track a social activity being performed.
        /// </summary>
        /// <param name="analyticsClient">AnalyticsClient currently configured.</param>
        /// <param name="action">Social action being performed.</param>
        /// <param name="network">Name of the social network being acted upon.</param>
        /// <param name="pagePath">Optional path of the page the action occured on.</param>
        /// <param name="target">Optional target resource being acted upon.</param>
        public static void TrackSocial(this AnalyticsClient analyticsClient, string action, string network, string target = null, string pagePath = null)
        {
            if (analyticsClient == null) throw new ArgumentNullException("analyticsClient");
            analyticsClient.Track(new SocialActivity(action, network, pagePath, target));
        }
    }
}