﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
using System;
using System.Diagnostics;
using CSharpAnalytics.Activities;

namespace CSharpAnalytics.Activities
{
    /// <summary>
    /// Captures the details of an individual event to be recorded in analytics.
    /// See https://developers.google.com/analytics/devguides/collection/gajs/eventTrackerGuide Event Tracking Guide
    /// </summary>
    [DebuggerDisplay("Event {Category}, {Action}, {Label}")]
    public class EventActivity : ActivityBase
    {
        private readonly string action;
        private readonly string category;
        private readonly string label;
        private readonly bool nonInteraction;
        private readonly int? value;

        /// <summary>
        /// Name to represent what this event did. Uniquely paired with <seealso cref="Category"/>.
        /// </summary>
        /// <example>Play</example>
        public virtual string Action
        {
            get { return action; }
        }

        /// <summary>
        /// Name used to group various related actions together.
        /// </summary>
        /// <example>Videos</example>
        public string Category
        {
            get { return category; }
        }

        /// <summary>
        /// Optional on-screen label associated with this event.
        /// </summary>
        /// <example>Gone With the Wind</example>
        public string Label
        {
            get { return label; }
        }

        /// <summary>
        /// Whether this event was caused by a user interaction or not. Used in the calculation
        /// of bounce rates.
        /// </summary>
        public bool NonInteraction
        {
            get { return nonInteraction; }
        }

        /// <summary>
        /// Optional numerical value associated with this event often used for timing or ratings.
        /// </summary>
        public int? Value
        {
            get { return value; }
        }

        /// <summary>
        /// Create a new EventActivity with various parameters to be captured.
        /// </summary>
        /// <param name="action">Action name to be assigned to the Action property.</param>
        /// <param name="category">Category name to be assigned to Category property.</param>
        /// <param name="label">Optional label to be assigned to the Label property.</param>
        /// <param name="value">Optional value to be assigned to the Value property.</param>
        /// <param name="nonInteraction">Optional boolean value to be assigned to the NonInteraction property.</param>
        public EventActivity(string action, string category, string label = null, int? value = null, bool nonInteraction = false)
        {
            this.category = category;
            this.action = action;
            this.label = label;
            this.value = value;
            this.nonInteraction = nonInteraction;
        }
    }
}

namespace CSharpAnalytics
{
    public static class EventExtensions
    {
        /// <summary>
        /// Capture the details of an event that will be sent to analytics.
        /// </summary>
        /// <param name="analyticsClient">Analytics object with queue and configuration set-up.</param>
        /// <param name="action">Action name of the event to send.</param>
        /// <param name="category">Category of the event to send.</param>
        /// <param name="label">Optional label name of the event to send.</param>
        /// <param name="value">Optional numeric value of the event to send.</param>
        /// <param name="nonInteraction">Optional boolean value to be assigned to the NonInteraction property.</param>
        public static void TrackEvent(this AnalyticsClient analyticsClient, string action, string category, string label = null, int? value = null, bool nonInteraction = false)
        {
            if (analyticsClient == null) throw new ArgumentNullException("analyticsClient");
            analyticsClient.Track(new EventActivity(action, category, label, value, nonInteraction));
        }
    }
}