﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using CSharpAnalytics.Network;
using CSharpAnalytics.Protocols.Measurement;
using CSharpAnalytics.Serializers;
using CSharpAnalytics.Sessions;
using CSharpAnalytics.SystemInfo;
using CSharpAnalytics.WindowsPhone;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Windows.Networking.Connectivity;

namespace CSharpAnalytics
{
    /// <summary>
    /// Helper class to get up and running with CSharpAnalytics in Winows Phone 8 applications.
    /// Either use as-is by calling StartAsync, Attach and StopAsync from your App.xaml.cs or use as a
    /// starting point to wire up your own way.
    /// </summary>
    public class WindowsPhoneAutoMeasurement : BaseAutoMeasurement
    {
        private static PhoneApplicationFrame attachedFrame;

        /// <summary>
        /// Attach to the root frame, hook into the navigation event and track initial page appview.
        /// Call this just before Window.Current.Activate() in your App.OnLaunched method.
        /// </summary>
        public void Attach(PhoneApplicationFrame frame)
        {
            if (frame == null)
                throw new ArgumentNullException("frame");

            if (frame != attachedFrame)
            {
                if (attachedFrame != null)
                    attachedFrame.Navigated -= FrameNavigated;
                frame.Navigated += FrameNavigated;
                attachedFrame = frame;
            }

            var content = frame.Content;
            if (content != null)
                TrackAppView(content.GetType());
        }

        protected override IEnvironment GetEnvironment()
        {
            return new WindowsPhoneEnvironment();
        }

        /// <summary>
        /// Hook into various events to automatically track suspend, resume, page navigation,
        /// social sharing etc.
        /// </summary>
        protected override void HookEvents()
        {
            var application = Application.Current;
            application.Startup += ApplicationOnStartup;
            application.Exit += ApplicationOnExit;
        }

        /// <summary>
        /// Handle application starting up.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Startup event parameter.</param>
        private async void ApplicationOnStartup(object sender, StartupEventArgs e)
        {
            await StartRequesterAsync();
        }

        /// <summary>
        /// Handle application suspending.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Empty event information.</param>
        private async void ApplicationOnExit(object sender, EventArgs e)
        {
            UnhookEvents();
            await StopRequesterAsync();
        }

        /// <summary>
        /// Unhook events that were wired up in HookEvents.
        /// </summary>
        /// <remarks>
        /// Not actually used in AutoMeasurement but here to show you what to do if you wanted to.
        /// </remarks>
        protected override void UnhookEvents()
        {
            var application = Application.Current;
            application.Startup -= ApplicationOnStartup;
            application.Exit -= ApplicationOnExit;

            if (attachedFrame != null)
                attachedFrame.Navigated -= FrameNavigated;
            attachedFrame = null;
        }

        /// <summary>
        /// Receive navigation events to translate them into analytics page views.
        /// </summary>
        /// <remarks>
        /// Implement IAnalyticsPageView if your pages look up content so you can
        /// track better detail from the end of your LoadState method.
        /// </remarks>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">NavigationEventArgs for the event.</param>
        private void FrameNavigated(object sender, NavigationEventArgs e)
        {
            if (e.Content != null)
                TrackAppView(e.Content.GetType());
        }

        protected override async Task SetupRequesterAsync()
        {
            var webRequester = new HttpWebRequester(ClientUserAgent + " " + WindowsPhoneSystemInfo.GetSystemUserAgent());
            Requester = webRequester.Request;
        }

        /// <summary>
        /// Determine if the Internet is available at this point in time.
        /// </summary>
        /// <returns>True if the Internet is available, false otherwise.</returns>
        protected override bool IsInternetAvailable()
        {
            var internetProfile = NetworkInformation.GetInternetConnectionProfile();
            if (internetProfile == null) return false;

            // Don't send analytics if data limit is close/over or they are roaming
            var cost = internetProfile.GetConnectionCost();
            return !cost.ApproachingDataLimit && !cost.OverDataLimit && !cost.Roaming;
        }

        /// <summary>
        /// Load the session state from storage if it exists, null if it does not.
        /// </summary>
        /// <returns>Task that completes when the SessionState is available.</returns>
        protected override async Task<T> Load<T>()
        {
            return await LocalFolderContractSerializer<T>.RestoreAsync(Filenames[typeof(T)]);
        }

        /// <summary>
        /// Save the session state to preserve state between application launches.
        /// </summary>
        /// <returns>Task that completes when the session state has been saved.</returns>
        protected override async Task Save<T>(T data)
        {
            await LocalFolderContractSerializer<T>.SaveAsync(data, Filenames[typeof(T)]);
        }
    }

    public static class AutoMeasurement
    {
        private static readonly WindowsPhoneAutoMeasurement instance = new WindowsPhoneAutoMeasurement();

        public static VisitorStatus VisitorStatus
        {
            get { return instance.VisitorStatus; }
        }

        public static MeasurementAnalyticsClient Client
        {
            get { return instance.Client; }
        }

        public static Action<string> DebugWriter
        {
            set { instance.DebugWriter = value; }
        }

        public static void Attach(PhoneApplicationFrame rootFrame)
        {
            instance.Attach(rootFrame);
        }

        public static void SetOptOut(bool optOut)
        {
            instance.SetOptOut(optOut);
        }

        public static void Start(MeasurementConfiguration measurementConfiguration, LaunchingEventArgs args, TimeSpan? uploadInterval = null)
        {
            var launchKind = args == null
                ? ""
                : args.GetType().Name.Replace("LaunchingEventArgs", "");

            instance.Start(measurementConfiguration, launchKind, uploadInterval);
        }
    }
}