﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
using System;
using System.Text.RegularExpressions;

namespace CSharpAnalytics.Protocols.Urchin
{
    /// <summary>
    /// Configuration settings for Urchin style analytics.
    /// </summary>
    public class UrchinConfiguration
    {
        private static readonly Regex accountIdMatch = new Regex(@"^UA-\d+-\d+$"); 

        private readonly string accountId;
        private readonly string hostName;
        private readonly TimeSpan sessionTimeout;

        /// <summary>
        /// Google Analytics provided property id in the format UA-XXXX-Y.
        /// </summary>
        public string AccountId { get { return accountId; } }

        /// <summary>
        /// Host name of the site or name of the application.
        /// </summary>
        public string HostName { get { return hostName; } }

        /// <summary>
        /// Whether to calculate a hash of the hostName or to just use a fake value of 1.
        /// </summary>
        /// <remarks>
        /// Provided for completeness but is not required for applications.
        /// </remarks>
        public bool CalculateHostNameHash { get; set; }

        /// <summary>
        /// Anonymize the tracking by scrubbing the last octet of the IP address.
        /// </summary>
        public bool AnonymizeIp { get; set; }

        /// <summary>
        /// Send analytics requests over HTTPS/SSL if true, over HTTP if not.
        /// </summary>
        public bool UseSsl { get; set; }

        /// <summary>
        /// How much inactivity before the existing session expires and a new one is started.
        /// </summary>
        public TimeSpan SessionTimeout { get { return sessionTimeout; } }
        
        /// <summary>
        /// Create a new cofiguration for analytics.
        /// </summary>
        /// <param name="accountId">Google Analytics provided property id in the format UA-XXXX-Y.</param>
        /// <param name="hostName">Host name of the site or name of the application.</param>
        /// <param name="sessionTimeout">Optional inactivity before existing session expires. Defaults to 20 minutes.</param>
        public UrchinConfiguration(string accountId, string hostName, TimeSpan? sessionTimeout = null)
        {
            if (!accountIdMatch.IsMatch(accountId))
                throw new ArgumentException("accountID must be in the format UA-XXXX-Y.");

            this.accountId = accountId;
            this.hostName = hostName;
            this.sessionTimeout = sessionTimeout ?? TimeSpan.FromMinutes(20);
            AnonymizeIp = true;
        }

        /// <summary>
        /// Calculate the hash of the hostName to checksum referrer requests.
        /// </summary>
        /// <returns>Hash of the hostName.</returns>
        internal long GetHostNameHash()
        {
            if (!CalculateHostNameHash || String.IsNullOrEmpty(hostName)) return 1;

            long hash = 0;
            for (var i = hostName.Length-1; i >= 0; i--)
            {
                var current = (byte)hostName[i];
                hash = ((hash << 6) & 0xfffffff) + current + (current << 14);
                var leftMost7 = hash & 0xfe00000;

                if (leftMost7 != 0)
                    hash ^= leftMost7 >> 21;
            }
            return hash;
        }
    }
}