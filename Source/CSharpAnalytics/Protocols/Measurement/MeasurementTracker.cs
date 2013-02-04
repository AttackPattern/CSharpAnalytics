﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
using System.Linq;
using CSharpAnalytics.Activities;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CSharpAnalytics.Protocols.Measurement
{
    /// <summary>
    /// Creates Measurement Protocol URIs for tracking by Google Analytics Measurement Protocol endpoint.
    /// </summary>
    internal class MeasurementTracker
    {
        private const string ProtocolVersion = "1";
        private const string ResolutionFormat = "{0}x{1}";

        private static readonly Random random = new Random();
        private static readonly Uri trackingEndpoint = new Uri("http://www.google-analytics.com/collect");
        private static readonly Uri secureTrackingEndpoint = new Uri("https://ssl.google-analytics.com/collect");

        private readonly MeasurementTrackerActivities trackerActivities = new MeasurementTrackerActivities();
        private readonly MeasurementConfiguration configuration;
        private readonly IEnvironment environment;     

        /// <summary>
        /// Create new MeasurementTracker to prepare URIs for Google's Measurement Protocol endpoint.
        /// </summary>
        /// <param name="configuration">Configuration of analytics.</param>
        /// <param name="environment">Environment details.</param>
        public MeasurementTracker(MeasurementConfiguration configuration, IEnvironment environment)
        {
            this.configuration = configuration;
            this.environment = environment;
        }

        /// <summary>
        /// Create an Measurement Protocol URI from an activity and custom variables.
        /// </summary>
        /// <param name="activity">Activity to create a URI for.</param>
        /// <returns>URI that when requested will track this activity.</returns>
        public Uri CreateUri(IActivity activity)
        {
            var parameters = BuildParameterList(activity);
            var uriBuilder = new UriBuilder(configuration.UseSsl ? secureTrackingEndpoint : trackingEndpoint) { Query = CreateQueryString(parameters) };
            return uriBuilder.Uri;
        }

        /// <summary>
        /// Build a list of the parameters required based on configuration, environment, activity, session, custom variables and state.
        /// </summary>
        /// <param name="activity">Activity to include in the parameter list.</param>
        /// <returns>Enumeration of key/value pairs containing the parameters necessary for this request.</returns>
        private IEnumerable<KeyValuePair<string, string>> BuildParameterList(IActivity activity)
        {
            return GetParameters()
                .Concat(GetParameters(environment))
                .Concat(GetParameters(configuration))
                .Concat(trackerActivities.GetActivityParameters(activity))
                .ToList();
        }

        /// <summary>
        /// Create a query for all the parameters in the key/value pairs applying necessary encoding.
        /// </summary>
        /// <param name="parameters">Parameters to combine into a query string.</param>
        /// <returns>Encoded query string of parameters.</returns>
        private static string CreateQueryString(IEnumerable<KeyValuePair<string, string>> parameters)
        {
            if (parameters == null) throw new ArgumentNullException("parameters");
            var normalized = parameters
                .GroupBy(p => p.Key)
                .Select(p => new { p.Key, Value = String.Join("", p.Select(r => r.Value)) })
                .ToArray();

            return String.Join("&", normalized.Select(p => p.Key + "=" + Uri.EscapeDataString(p.Value)));
        }

        /// <summary>
        /// Get parameters for this tracker's internal state.
        /// </summary>
        /// <returns>Enumerable of key/value pairs containing parameters for this tracker's internal state.</returns>
        private static IEnumerable<KeyValuePair<string, string>> GetParameters()
        {
            yield return KeyValuePair.Create("v", ProtocolVersion);
            yield return KeyValuePair.Create("z", random.Next().ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Get parameters for a given environment.
        /// </summary>
        /// <param name="environment">Environment to obtain parameters from.</param>
        /// <returns>Enumerable of key/value pairs containing parameters for this environment.</returns>
        internal static IEnumerable<KeyValuePair<string, string>> GetParameters(IEnvironment environment)
        {
            yield return KeyValuePair.Create("ul", environment.LanguageCode.ToLowerInvariant());
            yield return KeyValuePair.Create("de", environment.CharacterSet == null ? "-" : environment.CharacterSet.ToUpperInvariant());
            yield return KeyValuePair.Create("fl", String.IsNullOrEmpty(environment.FlashVersion) ? "-" : environment.FlashVersion);
            yield return KeyValuePair.Create("je", !environment.JavaEnabled.HasValue ? "-" : environment.JavaEnabled.Value ? "1" : "0");

            if (environment.ScreenColorDepth > 0)
                yield return KeyValuePair.Create("sd", String.Format("{0}-bit", environment.ScreenColorDepth));

            if (environment.ScreenHeight != 0 && environment.ScreenWidth != 0)
                yield return KeyValuePair.Create("sr", string.Format(ResolutionFormat, environment.ScreenWidth, environment.ScreenHeight));

            if (environment.ViewportHeight != 0 && environment.ViewportWidth != 0)
                yield return KeyValuePair.Create("vp", string.Format(ResolutionFormat, environment.ViewportWidth, environment.ViewportHeight));
        }

        /// <summary>
        /// Get parameters for a given configuration.
        /// </summary>
        /// <param name="configuration">Configuration to obtain parameters from.</param>
        /// <returns>Enumerable of key/value pairs containing parameters for this configuration.</returns>
        internal static IEnumerable<KeyValuePair<string, string>> GetParameters(MeasurementConfiguration configuration)
        {
            yield return KeyValuePair.Create("tid", configuration.AccountId);
            yield return KeyValuePair.Create("an", configuration.ApplicationName);
            yield return KeyValuePair.Create("av", configuration.ApplicationVersion);

            if (configuration.AnonymizeIp)
                yield return KeyValuePair.Create("aip", "1");
        }       
    }
}