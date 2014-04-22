﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System.Xml.Linq;

namespace CSharpAnalytics.WindowsPhone
{
    public static class WindowsPhoneSystemInformation
    {
        private static readonly string applicationName;
        private static readonly string applicationVersion;

        static WindowsPhoneSystemInformation()
        {
            var manifest = XDocument.Load("WMAppManifest.xml");
            if (manifest.Root != null)
            {
                var app = manifest.Root.Element("App");
                if (app != null)
                {
                    applicationName = (string)app.Attribute("Title");
                    applicationVersion = (string)app.Attribute("Version");
                }
            }
        }

        public static string ApplicationName { get { return applicationName; } }
        public static string ApplicationVersion { get { return applicationVersion; } }
    }
}