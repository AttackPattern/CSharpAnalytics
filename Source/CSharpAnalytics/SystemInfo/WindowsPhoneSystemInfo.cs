﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System;
using System.Linq;
using System.Xml.Linq;
#if WINDOWS_PHONE_APP
using Windows.Networking;
using Windows.Devices;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.UI.Xaml;
using Windows.ApplicationModel;
#else
using Microsoft.Phone.Info;
#endif
namespace CSharpAnalytics.SystemInfo
{
    public static class WindowsPhoneSystemInfo
    {
        private static readonly string applicationName;
        private static readonly string applicationVersion;

        static WindowsPhoneSystemInfo()
        {
#if WINDOWS_PHONE_APP
            /// @todo    not wild about the name here; not sure what the better option is
            applicationName = Package.Current.Id.Name;
            applicationVersion = String.Format("{0}.{1}", Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor);            
#else
            var manifest = XDocument.Load("WMAppManifest.xml");
            if (manifest.Root == null) return;
            
            var app = manifest.Root.Element("App");
            if (app == null) return;

            applicationName = (string)app.Attribute("Title");
            applicationVersion = (string)app.Attribute("Version");
#endif
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
#if WINDOWS_PHONE_APP
                var info = new EasClientDeviceInformation();
                String operatingSystem = info.OperatingSystem;
                String deviceManufacturer = info.SystemManufacturer;
                String deviceName = info.SystemProductName;
#else
                var osVersion = System.Environment.OSVersion.Version;
                var minor = osVersion.Minor;
                if (minor > 9) minor /= 10;
                String operatingSystem = "Windows Phone " + osVersion.Major + "." + minor;
                String deviceManufacturer = DeviceStatus.DeviceManufacturer;
                String deviceName = DeviceStatus.DeviceName;
#endif

                var parts = new[] {
                    "Windows Phone " + operatingSystem,
                    "ARM",
                    "Touch",
                    deviceManufacturer,
                    deviceName
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