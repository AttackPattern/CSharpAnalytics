﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Linq;
using System.Xml.Linq;
using Windows.Networking;
using Windows.Devices;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.UI.Xaml;
using Windows.ApplicationModel;

 namespace CSharpAnalytics.SystemInfo
{
    public static class WindowsPhoneSystemInfo
    {
        private static readonly string applicationName;
        private static readonly string applicationVersion;

        static WindowsPhoneSystemInfo()
        {
            /// @todo    not wild about the name here; not sure what the better option is
            applicationName = Package.Current.Id.Name;
            applicationVersion = String.Format("{0}.{1}", Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor);            
        }

        /// <summary>
        /// Get the Windows version number and processor architecture and cache it
        /// as a user agent string so it can be sent with HTTP requests.
        /// </summary>
        /// <returns>String containing formatted system parts of the user agent.</returns>
        public static string GetSystemUserAgent()
        {
            try
            {
                var info = new EasClientDeviceInformation();

                var parts = new[] {
                    "Windows Phone " + info.OperatingSystem,
                    "ARM",
                    "Touch",
                    info.SystemManufacturer,
                    info.SystemProductName
                };

                return "(" + String.Join("; ", parts.Where(e => !String.IsNullOrEmpty(e))) + ")";
            }
            catch
            {
                return "";
            }
        }

        public static string ApplicationName { get { return applicationName; } }
        public static string ApplicationVersion { get { return applicationVersion; } }
    }
}