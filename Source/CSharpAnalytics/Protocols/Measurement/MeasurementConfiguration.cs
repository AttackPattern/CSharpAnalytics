﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Text.RegularExpressions;

namespace CSharpAnalytics.Protocols.Measurement
{
    /// <summary>
    /// Configuration settings for analytics.
    /// </summary>
    public class MeasurementConfiguration
    {
        private static readonly Regex accountIdMatch = new Regex(@"^UA-\d+-\d+$"); 

        private readonly string accountId;
        private readonly string applicationName;
        private readonly string applicationVersion;

        /// <summary>
        /// Google Analytics provided property id in the format UA-XXXX-Y.
        /// </summary>
        public string AccountId { get { return accountId; } }

        /// <summary>
        /// Name of the application.
        /// </summary>
        public string ApplicationName { get { return applicationName; } }

        /// <summary>
        /// Version of the application.
        /// </summary>
        public string ApplicationVersion { get { return applicationVersion; } }

        /// <summary>
        /// Anonymize the tracking by scrubbing the last octet of the IP address.
        /// </summary>
        public bool AnonymizeIp { get; set; }

        /// <summary>
        /// Send analytics requests over HTTPS/SSL if true, over HTTP if not.
        /// </summary>
        public bool UseSsl { get; set; }
        
        /// <summary>
        /// Create a new cofiguration for analytics.
        /// </summary>
        /// <param name="accountId">Google Analytics provided property id in the format UA-XXXX-Y.</param>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="applicationVersion">Version of the application.</param>
        public MeasurementConfiguration(string accountId, string applicationName, string applicationVersion)
        {
            if (!accountIdMatch.IsMatch(accountId))
                throw new ArgumentException("accountID must be in the format UA-XXXX-Y.");

            this.accountId = accountId;
            this.applicationName = applicationName;
            this.applicationVersion = applicationVersion;
            AnonymizeIp = true;
        }

#if WINDOWS_STORE
        /// <summary>
        /// Create a new cofiguration for analytics.
        /// </summary>
        /// <param name="accountId">Google Analytics provided property id in the format UA-XXXX-Y.</param>
        public MeasurementConfiguration(string accountId)
            : this(accountId, Windows.ApplicationModel.Package.Current.Id.Name, FormatVersion(Windows.ApplicationModel.Package.Current.Id.Version))
        {
        }

        /// <summary>
        /// Format the application version number to be sent to analytics.
        /// </summary>
        /// <param name="version">Version of the application.</param>
        /// <returns>Formatted string containing the version number.</returns>
        internal static string FormatVersion(Windows.ApplicationModel.PackageVersion version)
        {
            return String.Join(".", version.Major, version.Minor, version.Build, version.Revision);
        }
#endif
    }
}