// Copyright (c) Attack Pattern LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System.Globalization;
using System.Windows;

#if !SILVERLIGHT
using Windows.Graphics.Display;
#endif

namespace CSharpAnalytics.Environment
{
    /// <summary>
    /// Implements the IEnvironment interface required by analytics to track details of the machine
    /// in a Windows Phone application.
    /// </summary>
    internal class WindowsPhoneEnvironment : IEnvironment
    {
        public string CharacterSet { get { return "UTF-8"; } }

        public string LanguageCode { get { return CultureInfo.CurrentCulture.ToString(); } }

        public string FlashVersion { get { return null; } }
        public bool? JavaEnabled { get { return null; } }

        public uint ScreenColorDepth { get { return 32; } }

        public uint ScreenHeight
        {
#if WINDOWS_PHONE_APP
            get { return (uint)Windows.UI.Xaml.Window.Current.Bounds.Height; }
#else
            get { return (uint)(ViewportHeight * (Application.Current.Host.Content.ScaleFactor / 100)); }
#endif
        }

        public uint ScreenWidth
        {
#if WINDOWS_PHONE_APP
            get { return (uint)Windows.UI.Xaml.Window.Current.Bounds.Width; }
#else
            get { return (uint)(ViewportWidth * (Application.Current.Host.Content.ScaleFactor / 100)); }
#endif
        }

        public uint ViewportHeight
        {
#if WINDOWS_PHONE_APP
            /// @todo   not sure what the WP8.1 equivalent is
            get { return ScreenHeight; }
#else
            get { return (uint)Application.Current.Host.Content.ActualHeight; }
#endif
        }

        public uint ViewportWidth
        {
#if WINDOWS_PHONE_APP
            /// @todo   not sure what the WP8.1 equivalent is
            get { return ScreenWidth; }
#else
            get { return (uint)Application.Current.Host.Content.ActualWidth; }
#endif
        }
    }
}