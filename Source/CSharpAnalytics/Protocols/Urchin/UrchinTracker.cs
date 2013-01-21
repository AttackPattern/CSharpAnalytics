﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
using System.Linq;
using CSharpAnalytics.Activities;
using System;
using System.Collections.Generic;
using System.Globalization;
using CSharpAnalytics.CustomVariables;
using CSharpAnalytics.Protocols;
using CSharpAnalytics.Sessions;

namespace CSharpAnalytics.Protocols.Urchin
{
    /// <summary>
    /// Creates Urchin style URIs for tracking by Google Analytics Urchin tracking endpoint.
    /// </summary>
    internal class UrchinTracker
    {
        private const string ClientVersion = "5.3.3csa1"; // Compatible with GA 5.3.3 (CSharpAnalytics v1)
        private const string ResolutionFormat = "{0}x{1}";

        private static readonly Random random = new Random();
        private static readonly Uri trackingEndpoint = new Uri("http://www.google-analytics.com/__utm.gif");
        private static readonly Uri secureTrackingEndpoint = new Uri("https://secure.google-analytics.com/__utm.gif");

        private readonly SessionManager sessionManager;
        private readonly Configuration configuration;
        private readonly IEnvironment environment;
        private string lastUtmpParameterValue;

        /// <summary>
        /// Create new UrchinTracker to prepare URIs for Google's Urchin tracker endpoint.
        /// </summary>
        /// <param name="configuration">Configuration of analytics.</param>
        /// <param name="sessionManager">Session manager.</param>
        /// <param name="environment">Environment details.</param>
        public UrchinTracker(Configuration configuration, SessionManager sessionManager, IEnvironment environment)
        {
            this.sessionManager = sessionManager;
            this.configuration = configuration;
            this.environment = environment;
        }

        /// <summary>
        /// Create an Urchin style URI from an activity and custom variables.
        /// </summary>
        /// <param name="activity">Activity to create a URI for.</param>
        /// <param name="customVariables">Custom variables to include in the URI.</param>
        /// <returns>Uri that when requested will track this activity.</returns>
        public Uri CreateUri(IActivity activity, ScopedCustomVariableSlots[] customVariables)
        {
            var parameters = BuildParameterList(activity, customVariables);

            // Undocumented parameter to set the date/time event occured - useful for offline or batch modes
            if (configuration.SendClientTime)
                parameters.Add(KeyValuePair.Create("utmht", new EpochTime(DateTimeOffset.Now).ToString()));

            CarryForwardLastPageParameter(activity, parameters);

            var uriBuilder = new UriBuilder(configuration.UseSsl ? secureTrackingEndpoint : trackingEndpoint) { Query = CreateQueryString(parameters) };
            return uriBuilder.Uri;
        }

        /// <summary>
        /// Carry forward the utmp page parameter value to future event activities to know which page they occurred on.
        /// </summary>
        /// <param name="activity">Current activity being processed.</param>C:\src\CSharpAnalytics\Source\CSharpAnalytics\Internal\
        /// <param name="parameters">Current parameters for this request.</param>
        private void CarryForwardLastPageParameter(IActivity activity, ICollection<KeyValuePair<string, string>> parameters)
        {
            if (activity is EventActivity && lastUtmpParameterValue != null)
                parameters.Add(KeyValuePair.Create("utmp", lastUtmpParameterValue));

            if (parameters.Any(k => k.Key == "utmp"))
                lastUtmpParameterValue = parameters.First(p => p.Key == "utmp").Value;
        }

        /// <summary>
        /// Build a list of the parameters required based on configuration, environment, activity, session, custom variables and state.
        /// </summary>
        /// <param name="activity">Activity to include in the parameter list.</param>
        /// <param name="customVariables">Custom variables to include in the parameter list.</param>
        /// <returns>List of key/value pairs containing the parameters necessary for this request.</returns>
        private List<KeyValuePair<string, string>> BuildParameterList(IActivity activity, ScopedCustomVariableSlots[] customVariables)
        {
            var finalCustomVariables = GetFinalCustomVariables(customVariables);

            return GetParameters()
                .Concat(GetParameters(environment))
                .Concat(GetParameters(configuration))
                .Concat(GetParameters(sessionManager, configuration.GetHostNameHash()))
                .Concat(GetParameters(finalCustomVariables))
                .Concat(UrchinTrackerActivities.GetActivityParameters(activity))
                .ToList();
        }

        /// <summary>
        /// Expands a set of scoped custom variable slots into a final array containing the custom variables and their scope.
        /// </summary>
        /// <param name="scopedCustomVariableSlots">Set of scoped custom variable slots to expand.</param>
        /// <returns>An array of custom variables with their scope indexed by slot.</returns>
        internal static ScopedCustomVariable[] GetFinalCustomVariables(params ScopedCustomVariableSlots[] scopedCustomVariableSlots)
        {
            var allSlots = scopedCustomVariableSlots.SelectMany(s => s.AllSlots).ToList();
            var highestSlotIndex = allSlots.Any() ? allSlots.Max(s => s.Key) : 0;
            var finalCustomVariables = new ScopedCustomVariable[highestSlotIndex + 1];

            foreach (var scopedSlots in scopedCustomVariableSlots.OrderByDescending(s => s.Scope))
                foreach (var slot in scopedSlots.AllSlots)
                    finalCustomVariables[slot.Key] = new ScopedCustomVariable(scopedSlots.Scope, slot.Value);

            return finalCustomVariables;
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
            yield return KeyValuePair.Create("utmwv", ClientVersion);
            yield return KeyValuePair.Create("utmn", random.Next().ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Get parameters for a given set of custom variables.
        /// </summary>
        /// <param name="customVariables">Custom variables to obtain parameters from.</param>
        /// <returns>Enumerable of key/value pairs containing parameters for these custom variables.</returns>
        private static IEnumerable<KeyValuePair<string, string>> GetParameters(ScopedCustomVariable[] customVariables)
        {
            yield return KeyValuePair.Create("utme", EncodeCustomVariables(customVariables));
        }

        /// <summary>
        /// Get parameters for a given environment.
        /// </summary>
        /// <param name="environment">Environment to obtain parameters from.</param>
        /// <returns>Enumerable of key/value pairs containing parameters for this environment.</returns>
        internal static IEnumerable<KeyValuePair<string, string>> GetParameters(IEnvironment environment)
        {
            yield return KeyValuePair.Create("utmul", environment.LanguageCode.ToLowerInvariant());
            yield return KeyValuePair.Create("utmcs", environment.CharacterSet == null ? "-" : environment.CharacterSet.ToUpperInvariant());
            yield return KeyValuePair.Create("utmfl", String.IsNullOrEmpty(environment.FlashVersion) ? "-" : environment.FlashVersion);
            yield return KeyValuePair.Create("utmje", !environment.JavaEnabled.HasValue ? "-" : environment.JavaEnabled.Value ? "1" : "0");

            if (environment.ScreenColorDepth > 0)
                yield return KeyValuePair.Create("utmsc", String.Format("{0}-bit", environment.ScreenColorDepth));

            if (!String.IsNullOrEmpty(environment.IpAddress))
                yield return KeyValuePair.Create("utmip", environment.IpAddress);

            if (environment.ScreenHeight != 0 && environment.ScreenWidth != 0)
                yield return KeyValuePair.Create("utmsr", string.Format(ResolutionFormat, environment.ScreenWidth, environment.ScreenHeight));

            if (environment.ViewportHeight != 0 && environment.ViewportWidth != 0)
                yield return KeyValuePair.Create("utmvp", string.Format(ResolutionFormat, environment.ViewportWidth, environment.ViewportHeight));
        }

        /// <summary>
        /// Get parameters for a given configuration.
        /// </summary>
        /// <param name="configuration">Configuration to obtain parameters from.</param>
        /// <returns>Enumerable of key/value pairs containing parameters for this configuration.</returns>
        internal static IEnumerable<KeyValuePair<string, string>> GetParameters(Configuration configuration)
        {
            yield return KeyValuePair.Create("utmac", configuration.AccountId);
            yield return KeyValuePair.Create("utmhn", configuration.HostName);

            if (configuration.AnonymizeIp)
                yield return KeyValuePair.Create("aip", "1");
        }

        /// <summary>
        /// Get parameters for a given session manager and domain hash.
        /// </summary>
        /// <param name="sessionManager">Session manager to obtain parameters from.</param>
        /// <param name="hostNameHash">Hash of this host name.</param>
        /// <returns>Enumerable of key/value pairs of session information.</returns>
        internal static IEnumerable<KeyValuePair<string, string>> GetParameters(SessionManager sessionManager, long hostNameHash)
        {
            yield return KeyValuePair.Create("utmhid", sessionManager.Session.HitId.ToString(CultureInfo.InvariantCulture));
            yield return KeyValuePair.Create("utms", sessionManager.Session.HitCount.ToString(CultureInfo.InvariantCulture));
            yield return KeyValuePair.Create("utmr", sessionManager.Referrer == null ? "-" : sessionManager.Referrer.OriginalString);
            yield return KeyValuePair.Create("utmcc", CreateCookieSubstituteParameter(sessionManager, hostNameHash));
        }

        /// <summary>
        /// Create a cookie-substitute parameter used to track session activity.
        /// </summary>
        /// <param name="sessionManager">Session manager to obtain session and visitor from</param>
        /// <param name="hostNameHash">Hash of this host name.</param>
        /// <returns>String containing a cookie-like set of session information.</returns>
        internal static string CreateCookieSubstituteParameter(SessionManager sessionManager, long hostNameHash)
        {
            return String.Format(CultureInfo.InvariantCulture, "__utma={0}.{1}.{2}.{3}.{4}.{5};",
                    hostNameHash,
                    ReduceGuidToUint(sessionManager.Visitor.Id),
                    new EpochTime(sessionManager.Visitor.FirstVisitAt),
                    new EpochTime(sessionManager.PreviousSessionStartedAt),
                    new EpochTime(sessionManager.Session.StartedAt),
                    sessionManager.Session.Number);
        }

        private static uint ReduceGuidToUint(Guid guid)
        {
            var bytes = guid.ToByteArray();
            unchecked
            {
                uint r = 0; // Shift-Add-XOR hash
                for (var i = 0; i < bytes.Length; i++)
                    r ^= (r << 5) + (r >> 2) + bytes[i];
                return r;
            }
        }

        /// <summary>
        /// Encode custom variables into a single parameter string.
        /// </summary>
        /// <param name="customVariables">Custom variables to encode.</param>
        /// <returns>Encoded custom variables.</returns>
        internal static string EncodeCustomVariables(ScopedCustomVariable[] customVariables)
        {
            return UtmeEncoder.Encode("8", customVariables.Select(c => c == null ? null : c.Variable.Name).ToArray())
                 + UtmeEncoder.Encode("9", customVariables.Select(c => c == null ? null : c.Variable.Value).ToArray())
                + UtmeEncoder.Encode("11", customVariables.Select(c => c == null ? null : GetScopeIdentity(c.Scope)).ToArray());
        }

        /// <summary>
        /// Get the numeric identity of visitor and session level scopes.
        /// </summary>
        /// <param name="scope">Scope to obtain the identity of.</param>
        /// <returns>Scope numeric identity of this scope or null if no scope or is Activity.</returns>
        private static string GetScopeIdentity(CustomVariableScope? scope)
        {
            return !scope.HasValue || scope == CustomVariableScope.Activity
                       ? null
                       : ((int)scope).ToString(CultureInfo.InvariantCulture);
        }
    }
}