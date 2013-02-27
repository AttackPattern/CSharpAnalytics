﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
using System;
using System.Diagnostics;
using CSharpAnalytics.Activities;
using CSharpAnalytics.Protocols.Measurement;

namespace CSharpAnalytics.Activities
{
    /// <summary>
    /// Captures the details of an application exception to be recorded in analytics.
    /// </summary>
    [DebuggerDisplay("Exception {Description}")]
    public class ExceptionActivity : IMeasurementActivity
    {
        private readonly string description;
        private readonly bool isFatal;

        /// <summary>
        /// Description of this exception.
        /// </summary>
        public virtual string Description
        {
            get { return description; }
        }

        /// <summary>
        /// Whether this exception was fatal (caused the application to stop) or not.
        /// </summary>
        public bool IsFatal
        {
            get { return isFatal; }
        }

        /// <summary>
        /// Create a new ExceptionActivity with various parameters to be captured.
        /// </summary>
        /// <param name="description">Description of the exception.</param>
        /// <param name="isFatal">Whether the exception was fatal (caused the app to crash).</param>
        public ExceptionActivity(string description, bool isFatal)
        {
            this.description = description;
            this.isFatal = isFatal;
        }
    }
}

namespace CSharpAnalytics
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Capture the details of an event that will be sent to analytics.
        /// </summary>
        /// <param name="analyticsClient">Analytics object with queue and configuration set-up.</param>
        /// <param name="description">Description of the exception.</param>
        /// <param name="isFatal">Whether the exception was fatal (caused the app to crash).</param>
        public static void TrackException(this MeasurementAnalyticsClient analyticsClient, string description, bool isFatal = false)
        {
            if (analyticsClient == null) throw new ArgumentNullException("analyticsClient");
            analyticsClient.Track(new ExceptionActivity(description, isFatal));
        }
    }
}