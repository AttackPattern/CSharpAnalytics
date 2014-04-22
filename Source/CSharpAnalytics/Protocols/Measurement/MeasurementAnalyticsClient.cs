﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using CSharpAnalytics.Activities;
using CSharpAnalytics.Collections;
using CSharpAnalytics.Sessions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace CSharpAnalytics.Protocols.Measurement
{
    /// <summary>
    /// MeasurementAnalyticsClient should exist for the scope of your application and is the primary entry point for tracking via Measurement Protocol.
    /// </summary>
    public class MeasurementAnalyticsClient
    {
        private readonly LockingDictionary<int, string> customDimensions = new LockingDictionary<int, string>();
        private readonly LockingDictionary<int, object> customMetrics = new LockingDictionary<int, object>();
        private readonly Queue<MeasurementActivityEntry> queue = new Queue<MeasurementActivityEntry>();

        private MeasurementTracker tracker;

        /// <summary>
        /// Event to allow you to hook in to capture or modify activities.
        /// </summary>
        public event EventHandler<IMeasurementActivity> OnTrack = delegate { };

        /// <summary>
        /// Configure this MeasurementAnalyticsClient so it can start recording and sending analytics.
        /// </summary>
        /// <param name="configuration">Configuration settings for this client.</param>
        /// <param name="sessionManager">Session manager to store and retreive session state.</param>
        /// <param name="environment">Provider of environmental details such as screen resolution.</param>
        /// <param name="sender">Action delegate responsible for sending URIs to analytics.</param>
        public void Configure(MeasurementConfiguration configuration, SessionManager sessionManager, IEnvironment environment, Action<Uri> sender)
        {
            Debug.Assert(tracker == null);
            var newTracker = new MeasurementTracker(configuration, sessionManager, environment, sender);
            while (queue.Count > 0)
                newTracker.Track(queue.Dequeue());
            tracker = newTracker;
        }

        /// <summary>
        /// Track a activity in analytics.
        /// </summary>
        /// <param name="activity">Activity to track in analytics.</param>
        /// <param name="endSession">True if this tracking event should end the session.</param>
        public void Track(IMeasurementActivity activity, bool endSession = false)
        {
            if (activity is AutoTimedEventActivity)
                ((AutoTimedEventActivity)activity).End();

            OnTrack(this, activity);

            var entry = new MeasurementActivityEntry(activity)
            {
                CustomDimensions = customDimensions.ToArray(),
                CustomMetrics = customMetrics.ToArray(),
                EndSession = endSession
            };

            customDimensions.Clear();
            customMetrics.Clear();

            if (tracker == null)
                queue.Enqueue(entry);
            else
                tracker.Track(entry);
        }

        /// <summary>
        /// Set the value of a custom dimension to be set with the next activity.
        /// </summary>
        /// <remarks>
        /// These need to be configured first in Google Analytics.
        /// </remarks>
        /// <param name="index">Index of the custom dimension the value is for.</param>
        /// <param name="value">Value for the custom dimension specified by the index.</param>
        public void SetCustomDimension(int index, string value)
        {
            customDimensions[index] = value;
        }

        /// <summary>
        /// Set the value of a custom dimension to be set with the next activity.
        /// </summary>
        /// <remarks>
        /// These need to be configured first in Google Analytics.
        /// This overide allows you to use an enum instead of integers for the index.
        /// </remarks>
        /// <param name="index">Index of the custom dimension the value is for.</param>
        /// <param name="value">Value for the custom dimension specified by the index.</param>
        public void SetCustomDimension(Enum index, string value)
        {
            ValidateEnum(index);
            SetCustomDimension(Convert.ToInt32(index, CultureInfo.InvariantCulture), value);
        }

        /// <summary>
        /// Set the integer value of a custom metric to be sent with the next activity.
        /// </summary>
        /// <remarks>
        /// These need to be configured first in Google Analytics.
        /// </remarks>
        /// <param name="index">Index of the custom metric the value is for.</param>
        /// <param name="value">Integer value for the custom metric specified by the index.</param>
        public void SetCustomMetric(int index, long value)
        {
            customMetrics[index] = value;
        }

        /// <summary>
        /// Set the time value of a custom metric to be sent with the next activity.
        /// </summary>
        /// <remarks>
        /// These need to be configured first in Google Analytics.
        /// </remarks>
        /// <param name="index">Index of the custom metric the value is for.</param>
        /// <param name="value">Time value for the custom metric specified by the index.</param>
        public void SetCustomMetric(int index, TimeSpan value)
        {
            customMetrics[index] = value;
        }

        /// <summary>
        /// Set the financial value of a custom metric to be sent with the next activity.
        /// </summary>
        /// <remarks>
        /// These need to be configured first in Google Analytics.
        /// </remarks>
        /// <param name="index">Index of the custom metric the value is for.</param>
        /// <param name="value">Financial value for the custom metric specified by the index.</param>
        public void SetCustomMetric(int index, decimal value)
        {
            customMetrics[index] = value;
        }

        /// <summary>
        /// Validate an enum to ensure it is defined and has an underlying int type throwing
        /// an exception if it does not.
        /// </summary>
        /// <param name="index">Enum to check.</param>
        private static void ValidateEnum(Enum index)
        {
            if (Enum.GetUnderlyingType(index.GetType()) != typeof(int))
                throw new ArgumentException("Enum must be of type int", "index");

            if (!Enum.IsDefined(index.GetType(), index))
                throw new ArgumentOutOfRangeException("index", "Enum value is not defined");
        }

        /// <summary>
        /// Adjust the URI before it is finally sent so it can have a relative queue time parameter.
        /// </summary>
        /// <param name="uri">Uri that was going to requested via Measurement Protocol collector.</param>
        /// <returns>Adjusted Uri to be requested instead.</returns>
        public Uri AdjustUriBeforeRequest(Uri uri)
        {
            var parameters = GetQueryParameters(uri.GetComponents(UriComponents.Query, UriFormat.Unescaped));
            AddQueueTimeFromFragment(uri, parameters);
            return new UriBuilder(uri) { Query = GetQueryString(parameters), Fragment = "" }.Uri;
        }

        /// <summary>
        /// Extract the timestamp from the URI fragment and turn it into a relative time offset as
        /// the QT parameter.
        /// </summary>
        /// <param name="uri">URI to extract the timestamp fragment from.</param>
        /// <param name="parameters">URI parameters to add the relative QT parameter to.</param>
        private static void AddQueueTimeFromFragment(Uri uri, IDictionary<string, string> parameters)
        {
            if (String.IsNullOrWhiteSpace(uri.Fragment) || parameters.ContainsKey("sc")) return;
            var decodedFragment = uri.GetComponents(UriComponents.Fragment, UriFormat.Unescaped);

            DateTime utcHitTime;
            if (!DateTime.TryParse(decodedFragment, out utcHitTime)) return;

            var queueTime = DateTimeOffset.Now.Subtract(utcHitTime);
            if (queueTime.TotalMilliseconds < 0) return;

            parameters["qt"] = queueTime.TotalMilliseconds.ToString("0", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Splits a URI query string into a key/value dictionary of parameters and
        /// their arguments.
        /// </summary>
        /// <param name="queryString">Query string to split into a dictionary.</param>
        /// <returns>Dictionary of key-value pairs matching the query string parameter names and values.</returns>
        private static IDictionary<string, string> GetQueryParameters(string queryString)
        {
            return queryString
                .Split('&')
                .Select(q => q.Split('='))
                .ToDictionary(k => k[0], v => v.Length > 1 ? v[1] : "");
        }

        /// <summary>
        /// Build a URI query string from a set from an enumeration of key/value pairs.
        /// </summary>
        /// <param name="queryParameters">Enumeration of names and parameters to make into a URI query string.</param>
        /// <returns>URI query string containing the names and parameters.</returns>
        private static string GetQueryString(IEnumerable<KeyValuePair<string, string>> queryParameters)
        {
            return String.Join("&", queryParameters.Select(p => p.Key + "=" + p.Value));
        }
    }
}