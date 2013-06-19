﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Globalization;

namespace CSharpAnalytics.Protocols
{
    /// <summary>
    /// Represents a date and time expressed as the number of seconds since 00:00 on 01-Jan-1970.
    /// </summary>
    internal class EpochTime
    {
        private static readonly DateTimeOffset epochMoment = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        private readonly ulong secondsSince1970;

        /// <summary>
        /// Current system time expressed in EpochTime.
        /// </summary>
        public static EpochTime Now { get { return new EpochTime(DateTimeOffset.Now); } }

        /// <summary>
        /// Create a new EpochTime with a given number of seconds since the start of 1970.
        /// </summary>
        /// <param name="secondsSince1970">Number of seconds since the start of 1970.</param>
        public EpochTime(ulong secondsSince1970)
        {
            this.secondsSince1970 = secondsSince1970;
        }

        /// <summary>
        /// Create a new EpochTime from an existing DateTimeOffset.
        /// </summary>
        /// <param name="offset"></param>
        public EpochTime(DateTimeOffset offset)
        {
            secondsSince1970 = Convert.ToUInt64((offset - epochMoment).TotalSeconds);
        }

        /// <summary>
        /// Return an EpochTime as a DateTimeOffset.
        /// </summary>
        /// <returns>A DateTimeOffset that represents the EpochTime.</returns>
        public DateTimeOffset ToDateTimeOffset()
        {
            return epochMoment.AddSeconds(secondsSince1970);
        }

        /// <summary>
        /// Return a string containing the number of seconds since 1970.
        /// </summary>
        /// <returns>A string containing the number of seconds since 1970.</returns>
        public override string ToString()
        {
            return secondsSince1970.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Return a string with the EpochTime represented as a UTC date format.
        /// </summary>
        /// <returns>UTC date format of the current EpochTime.</returns>
        public string ToUtcString()
        {
            return ToDateTimeOffset().ToString("r");
        }

        /// <summary>
        /// Format number of seconds since 1970 as formatted UTC date.
        /// </summary>
        /// <param name="secondsSince1970">Number of seconds since 01-Jan-1970.</param>
        /// <returns>Formatted UTC date.</returns>
        public static string FormatDate(string secondsSince1970)
        {
            ulong numericSecondsSince1970;
            return !ulong.TryParse(secondsSince1970, out numericSecondsSince1970)
                ? String.Empty
                : new EpochTime(numericSecondsSince1970).ToUtcString();
        }
    }
}