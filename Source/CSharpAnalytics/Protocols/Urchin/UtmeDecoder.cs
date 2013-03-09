﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Collections.Generic;
using System.Linq;

namespace CSharpAnalytics.Protocols.Urchin
{
    /// <summary>
    /// Decodes Utme parameter encoded values back into their original unencoded values.
    /// </summary>
    internal static class UtmeDecoder
    {
        /// <summary>
        /// Extract a specific segment part and segment from the originalUtme value.
        /// </summary>
        /// <param name="originalUtme">Original utme value to extract from.</param>
        /// <param name="part">Part number of the data to extract. May be 5 for events, 8 to 11 for custom variable labels, values and scopes.</param>
        /// <param name="segment">Segment number to extract.</param>
        /// <returns>Extracted value.</returns>
        public static string Decode(string originalUtme, int part, int segment)
        {
            if (String.IsNullOrWhiteSpace(originalUtme))
                return String.Empty;

            var foundPart = originalUtme.Split(')')
                .Select(p => new { Value = p, OpenIndex = p.IndexOf('(') })
                .Where(p => p.OpenIndex > 0)
                .Select(p => new { Value = p.Value.Substring(p.OpenIndex + 1), Id = int.Parse(p.Value.Substring(0, p.OpenIndex)) })
                .FirstOrDefault(p => p.Id == part);

            if (foundPart == null)
                return String.Empty;

            var segments = Decompress(foundPart.Value.Split('*'));
            return segment < segments.Length ? UnescapeValue(segments[segment]) : null;
        }

        /// <summary>
        /// Unescape Utme values back to their unescaped original value.
        /// </summary>
        /// <param name="value">Utme escaped value.</param>
        /// <returns>Original unescaped value.</returns>
        public static string UnescapeValue(string value)
        {
            return String.IsNullOrWhiteSpace(value)
                ? ""
                : value.Replace("'3", "!")
                    .Replace("'2", "*")
                    .Replace("'1", ")")
                    .Replace("'0", "'");
        }

        /// <summary>
        /// Decompresses an array of Utme values back into their full uncompressed array.
        /// </summary>
        /// <param name="compressed">Compressed array of Utme values.</param>
        /// <returns>Original uncompressed array of Utme values.</returns>
        public static string[] Decompress(string[] compressed)
        {
            if (compressed == null) throw new ArgumentNullException("compressed");

            var decompressed = new List<string>();
            
            for(var i = 0; i < compressed.Length; i++) {
                var value = compressed[i];
                var bangIndex = value.IndexOf("!", StringComparison.Ordinal);
                if (bangIndex > 0)
                {
                    var decompressedIndex = int.Parse(value.Substring(0, bangIndex)) - 1;
                    value = value.Substring(bangIndex + 1);
                    while (decompressedIndex-- > i)
                        decompressed.Add(null);
                }
                
                decompressed.Add(value);
            }

            return decompressed.ToArray();
        }
    }
}