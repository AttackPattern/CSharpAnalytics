﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Linq;

namespace CSharpAnalytics.Protocols.Urchin
{
    /// <summary>
    /// Lists all known parameters found in a Google Analytics Urchin style URI.
    /// </summary>
    /// <remarks>
    /// Used to facilitate the debugging of such requests.
    /// </remarks>
    static class UrchinParameterDefinitions
    {
        /// <summary>
        /// All parameters that may be found in a Google Analytics Urchin style request.
        /// </summary>
        /// <remarks>
        /// The order of these should match the ga_debug.js output order for consistency.
        /// </remarks>
        public static readonly ParameterDefinition[] All = new[]
        {
            new ParameterDefinition("utmac",    "Account ID"),
            new ParameterDefinition("utmdt",    "Page Title"),
            new ParameterDefinition("utmhn",    "Host Name"),
            new ParameterDefinition("utmp",     "Page"),
            new ParameterDefinition("utmr",     "Referring URL"),
            new ParameterDefinition("utmhid",   "Hit ID"),
            new ParameterDefinition("utmt",     "Hit Type"),

            new ParameterDefinition("utmsn",    "Social Network"),
            new ParameterDefinition("utmsa",    "Social Action"),
            new ParameterDefinition("utmsid",   "Social Action URL"),

            // Google's own ga_debug.js gets Event Type/Action reversed - this one is correct.
            new ParameterDefinition("utme",     "Event Type", s => UtmeDecoder.Decode(s, 5, 0)),
            new ParameterDefinition("utme",     "Event Action", s => UtmeDecoder.Decode(s, 5, 1)),
            new ParameterDefinition("utme",     "Event Label", s => UtmeDecoder.Decode(s, 5, 2)),
            new ParameterDefinition("utme",     "Event Value", s => UtmeDecoder.Decode(s, 5, 3)),

            new ParameterDefinition("utmcc",    "Visitor ID", s => ExtractUtma(s, 1)),
            new ParameterDefinition("utmcc",    "Session Count", s => ExtractUtma(s, 5)),
            new ParameterDefinition("utmcc",    "Session Time - First", s => EpochTime.FormatDate(ExtractUtma(s, 2))),
            new ParameterDefinition("utmcc",    "Session Time - Last"  , s => EpochTime.FormatDate(ExtractUtma(s, 3))),
            new ParameterDefinition("utmcc",    "Session Time - Current", s => EpochTime.FormatDate(ExtractUtma(s, 4))),

            new ParameterDefinition("utmcc",    "Campaign Time", s => EpochTime.FormatDate(ExtractUtmz(s, 1))),
            new ParameterDefinition("utmcc",    "Campaign Session", s => ExtractUtmz(s, 2)),
            new ParameterDefinition("utmcc",    "Campaign Count", s => ExtractUtmz(s, 3)),
            new ParameterDefinition("utmcc",    "Campaign Source", s => ExtractUtmz(s, 4, "utmcsr")),
            new ParameterDefinition("utmcc",    "Campaign Medium", s => ExtractUtmz(s, 4, "utmcmd")),
            new ParameterDefinition("utmcc",    "Campaign Name", s => ExtractUtmz(s, 4, "utmccn")),

            new ParameterDefinition("utme",     "Custom Var 1", s => ExtractCustomVar(s, 1)),
            new ParameterDefinition("utme",     "Custom Var 2", s => ExtractCustomVar(s, 2)),
            new ParameterDefinition("utme",     "Custom Var 3", s => ExtractCustomVar(s, 3)),
            new ParameterDefinition("utme",     "Custom Var 4", s => ExtractCustomVar(s, 4)),
            new ParameterDefinition("utme",     "Custom Var 5", s => ExtractCustomVar(s, 5)),

            new ParameterDefinition("utmipc",   "Product Code"),
            new ParameterDefinition("utmipn",   "Product Name"),
            new ParameterDefinition("utmipr",   "Unit Price"),
            new ParameterDefinition("utmiqt",   "Quantity"),
            new ParameterDefinition("utmiva",   "Item Variation"),

            new ParameterDefinition("utmtid",   "Order ID"),
            new ParameterDefinition("utmtci",   "Billing City"),
            new ParameterDefinition("utmtrg",   "Billing Region"),
            new ParameterDefinition("utmtco",   "Billing Country"),
            new ParameterDefinition("utmtst",   "Affilliation"),
            new ParameterDefinition("utmtto",   "Total"),
            new ParameterDefinition("utmtsp",   "Shipping Cost"),
            new ParameterDefinition("utmttx",   "Tax"),
        
            new ParameterDefinition("utmul",    "Language"),
            new ParameterDefinition("utmcs",    "Encoding"),
            new ParameterDefinition("utmfl",    "Flash Version"),
            new ParameterDefinition("utmje",    "Java Enabled", FormatBoolean),
            new ParameterDefinition("utmsr",    "Screen Resolution"),
            new ParameterDefinition("utmvp",    "Browser Size"),
            new ParameterDefinition("utmsc",    "Color Depth"),
            new ParameterDefinition("utmwv",    "Tracking Agent"),
            new ParameterDefinition("utmn",     "Cachebuster"),

            // Additional debug info not present in ga_debug.js
            new ParameterDefinition("utms",     "Session Hit Count"),
            new ParameterDefinition("aip",      "Anonymize IP", FormatBoolean),
            new ParameterDefinition("utmht",    "Real Event Time", EpochTime.FormatDate)
        };

        /// <summary>
        /// Extract values from the __utma cookie part for a given slot.
        /// </summary>
        /// <param name="cookies">Utmcc cookie string to extract from.</param>
        /// <param name="slot">Slot number to be extracted.</param>
        /// <returns>Extracted value.</returns>
        private static string ExtractUtma(string cookies, int slot)
        {
            return CookiePart(cookies, "__utma", slot);
        }

        /// <summary>
        /// Extract values from the __utmz cookie part for a given slot.
        /// </summary>
        /// <param name="cookies">Utmcc cookie string to extract from.</param>
        /// <param name="slot">Slot number to be extracted.</param>
        /// <param name="key">Key name to extract from slot value.</param>
        /// <returns>Extracted value.</returns>
        private static string ExtractUtmz(string cookies, int slot, string key = null)
        {
            var cookiePart = CookiePart(cookies, "__utmz", slot);
            if (key == null || cookiePart == null) return cookiePart;
            var parameters = cookiePart.Split('|').ToDictionary(k => k.Split('=')[0], v => v.Split('=')[1]);
            string utmzValue;
            parameters.TryGetValue(key, out utmzValue);
            return utmzValue;
        }

        /// <summary>
        /// Convert a boolean 1 or 0 back to True or False.
        /// </summary>
        /// <param name="input">Input string to convert.</param>
        /// <returns>True for 1, False for 0 and - for all other values.</returns>
        private static string FormatBoolean(string input)
        {
            switch (input)
            {
                case "1": return "True";
                case "0": return "False";
                default: return "-";
            }
        }

        /// <summary>
        /// Extract a single custom variable from an originalCookie string by slot number.
        /// </summary>
        /// <param name="originalUtme">Original utme value to extract from.</param>
        /// <param name="slot">Slot number to extract.</param>
        /// <returns>Extracted formatted custom variable.</returns>
        private static string ExtractCustomVar(string originalUtme, int slot)
        {
            var result = string.Empty;

            {
                var label = UtmeDecoder.Decode(originalUtme, 8, slot - 1);
                if (!string.IsNullOrWhiteSpace(label))
                    result += "label:'" + label + "' ";
            }

            {
                var value = UtmeDecoder.Decode(originalUtme, 9, slot - 1);
                if (!string.IsNullOrWhiteSpace(value))
                    result += "value:'" + value + "' ";
            }

            {
                var scope = UtmeDecoder.Decode(originalUtme, 11, slot - 1);
                if (!string.IsNullOrWhiteSpace(scope))
                    result += "scope:'" + scope + "' ";
            }

            return result.Trim();
        }

        /// <summary>
        /// Obtain part of a cookie by key name and slot number.
        /// </summary>
        /// <param name="originalCookie">Original cookie to extract from.</param>
        /// <param name="key">Name of the key to extract from the cookie.</param>
        /// <param name="slot">Number of the slot to extract.</param>
        /// <returns>Extracted value.</returns>
        private static string CookiePart(string originalCookie, string key, int slot)
        {
            if (slot <= 0) return null;

            var pairs = Uri.UnescapeDataString(originalCookie)
                .Split('+')
                .Select(c => new { Key = SubstringBefore(c, '='), Value = SubstringAfter(c, '=') });

            return pairs
                .Where(p => p.Key == key)
                .Select(p => p.Value.Split('.'))
                .Select(values => slot < values.Length ? values[slot] : null)
                .FirstOrDefault();
        }

        /// <summary>
        /// Extract part of a string up to and not including the first instance of the before character.
        /// </summary>
        /// <param name="input">String to extract from.</param>
        /// <param name="before">Character to stop before.</param>
        /// <returns>Substring of input up to but not including before character.</returns>
        private static string SubstringBefore(string input, char before)
        {
            return input.Substring(0, input.IndexOf(before));
        }

        /// <summary>
        /// Extract part of a string from the point directly following the after character to the end.
        /// </summary>
        /// <param name="input">String to extract from.</param>
        /// <param name="after">Character to start after.</param>
        /// <returns>Substring of input directly following the after character to the end of the string.</returns>
        private static string SubstringAfter(string input, char after)
        {
            return input.Substring(input.IndexOf(after) + 1);
        }
    }
}