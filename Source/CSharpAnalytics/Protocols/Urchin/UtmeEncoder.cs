﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using CSharpAnalytics.Activities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CSharpAnalytics.Protocols.Urchin
{
    /// <summary>
    /// Encodes parameter values into the Utme parameter format used by Google Analytics for events and custom variables.
    /// </summary>
    internal static class UtmeEncoder
    {
        /// <summary>
        /// Encode an array of values with an prefix using Utme compression and escaping.
        /// </summary>
        /// <param name="prefix">Prefix of the values being encoded.</param>
        /// <param name="values">Values being encoded.</param>
        /// <returns>Encoded set of values marked with prefix.</returns>
        public static string Encode(string prefix, params string[] values)
        {
            var compressedEncodedValues = Compress(values.Select(EscapeValue).ToArray());
            var result = String.Join("*", compressedEncodedValues);

            if (String.IsNullOrWhiteSpace(result))
                return String.Empty;

            return prefix + "(" + result + ")";
        }

        /// <summary>
        /// Encode a TimedEventActivity using Utme escaping.
        /// </summary>
        /// <param name="timedEventActivity">TimedEventActivity being encoded.</param>
        /// <returns>Encoded set of values marked with prefix.</returns>
        public static string Encode(TimedEventActivity timedEventActivity)
        {
            var roundedTime = timedEventActivity.Time.TotalMilliseconds/10*10;
            var result = String.Join("*", new[] { timedEventActivity.Variable, timedEventActivity.Category, roundedTime.ToString(CultureInfo.InvariantCulture), timedEventActivity.Label });

            if (String.IsNullOrWhiteSpace(result))
                return String.Empty;

            return "14(90!" + result + ")(90!" + timedEventActivity.Time.TotalMilliseconds.ToString(CultureInfo.InvariantCulture) + ")";
        }


        /// <summary>
        /// Escaped a value by replacing single quote, close brace, asterisk and exclamation with their encoded counterparts.
        /// </summary>
        /// <param name="value">Unescaped value to escape.</param>
        /// <returns>Escaped equivalent.</returns>
        public static string EscapeValue(string value)
        {
            return String.IsNullOrWhiteSpace(value)
                ? ""
                : value.Replace("'", "'0")
                      .Replace(")", "'1")
                      .Replace("*", "'2")
                      .Replace("!", "'3");
        }

        /// <summary>
        /// Compress an array of values into a Utme compressed array by skipping empty elements and prefixing the next one with an offset.
        /// </summary>
        /// <param name="uncompressed">Original uncompressed array of values</param>
        /// <returns>Array of values that are Utme compressed.</returns>
        public static string[] Compress(string[] uncompressed)
        {
            if (uncompressed == null) throw new ArgumentNullException("uncompressed");

            var compressed = new List<string>();
            var compressedIndex = 0;

            for (var i = 0; i < uncompressed.Length; i++)
            {
                var value = uncompressed[i];
                if (!String.IsNullOrWhiteSpace(value))
                {
                    if (i == compressedIndex)
                    {
                        compressed.Add(value);
                        compressedIndex++;
                    }
                    else
                    {
                        compressed.Add(i + 1 + "!" + value);
                        compressedIndex = i + 1;
                    }
                }
            }

            return compressed.ToArray();
        }
    }
}