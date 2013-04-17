﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CSharpAnalytics.Protocols
{
    /// <summary>
    /// Provide debugging of tracking requests by decomposing such request back into their parts.
    /// </summary>
    /// <remarks>
    /// Output is similar to ga_debug.js.
    /// </remarks>
    internal class ProtocolDebugger
    {
        private readonly ParameterDefinition[] parameterDefinitions;
        private readonly Action<string> writer;

        /// <summary>
        /// Create a new ProtocolDebugger with a given action to receive debugger output.
        /// </summary>
        /// <param name="writer">Action that takes a string to receive debugger output.</param>
        /// <param name="parameterDefinitions">Array of ParameterDefinitions valid for this debugger.</param>
        public ProtocolDebugger(Action<string> writer, ParameterDefinition[] parameterDefinitions)
        {
            this.writer = writer;
            this.parameterDefinitions = parameterDefinitions;
        }

        /// <summary>
        /// Examine URI and break down into constituent parts via the writer
        /// for this debugger.
        /// </summary>
        /// <param name="uri">Analytics tracking URI to examine.</param>
        public void Examine(Uri uri)
        {
            writer("-Analytics-------------------------------------");

            var parameters = ExtractParameters(uri);

            writer(uri.Query);

            foreach (var parameterDefinition in parameterDefinitions)
            {
                if (!parameterDefinition.IsRegexMatch)
                {
                    string rawValue;
                    if (parameters.TryGetValue(parameterDefinition.Name, out rawValue))
                        WriteParameter(parameterDefinition, parameterDefinition.Label, rawValue);
                }
                else
                {
                    foreach (var pair in parameters)
                    {
                        var match = Regex.Match(pair.Key, parameterDefinition.Name);
                        if (match.Success)
                        {
                            var label = parameterDefinition.Label.Replace("$1", match.Groups[1].Captures[0].Value);
                            WriteParameter(parameterDefinition, label, pair.Value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Extract the query string parameters from a URI into a dictionary of keys and values.
        /// </summary>
        /// <param name="uri">URI to extract the parameters from.</param>
        /// <returns>Dictionary of keys and values representing the parameters.</returns>
        private static Dictionary<string, string> ExtractParameters(Uri uri)
        {
            return uri.GetComponents(UriComponents.Query, UriFormat.SafeUnescaped)
                .Split('&')
                .Select(kv => kv.Split('='))
                .ToDictionary(k => k[0], v => Uri.UnescapeDataString(v[1]));
        }

        /// <summary>
        /// Format and write a parameter to the current writer.
        /// </summary>
        /// <param name="parameterDefinition">Parameter to write out.</param>
        /// <param name="rawValue">Raw value of the parameter to format before writing.</param>
        private void WriteParameter(ParameterDefinition parameterDefinition, string label, string rawValue)
        {
            var formattedValue = parameterDefinition.Formatter(rawValue);
            if (!String.IsNullOrWhiteSpace(formattedValue))
                writer(label.PadRight(24) + ": " + formattedValue);
        }
    }
}