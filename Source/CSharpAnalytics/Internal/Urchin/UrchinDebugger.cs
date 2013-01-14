﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
using System;
using System.Collections.Generic;
using System.Linq;
 
namespace CSharpAnalytics.Internal.Urchin
{
    /// <summary>
    /// Provide debugging of Urchin style tracking requests by decomposing such request back into their parts.
    /// </summary>
    /// <remarks>
    /// Output of this is as close to the ga_debug.js output as possible.
    /// </remarks>
    internal class UrchinDebugger
    {
        private readonly Action<string> writer;

        /// <summary>
        /// Create a new UrchinDebugger with a given action to receive debugger output.
        /// </summary>
        /// <param name="writer">Action that takes a string to receive debugger output.</param>
        public UrchinDebugger(Action<string> writer)
        {
            this.writer = writer;
        }

        /// <summary>
        /// Examine an Urchin style URI to break down into constituent parts via the writer
        /// for this debugger.
        /// </summary>
        /// <param name="uri">Urchin style URI to examine.</param>
        public void Examine(Uri uri)
        {
            writer("-Analytics-------------------------------------");

            var parameters = ExtractParameters(uri);

            writer("Track " + (parameters.ContainsKey("utmt") ? parameters["utmt"] : "page"));
            writer(uri.Query);

            foreach (var parameterDefinition in UrchinParameterDefinitions.All)
            {
                string rawValue;
                if (parameters.TryGetValue(parameterDefinition.Name, out rawValue))
                    WriteParameter(parameterDefinition, rawValue);
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
        private void WriteParameter(ParameterDefinition parameterDefinition, string rawValue)
        {
            var formattedValue = parameterDefinition.Formatter(rawValue);
            if (!String.IsNullOrWhiteSpace(formattedValue))
                writer(parameterDefinition.Label.PadRight(24) + ": " + formattedValue);
        }
    }
}