﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using CSharpAnalytics.Environment;
using CSharpAnalytics.Network;
using CSharpAnalytics.Protocols.Measurement;
using CSharpAnalytics.Serializers;
using CSharpAnalytics.Sessions;
using CSharpAnalytics.SystemInfo;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CSharpAnalytics
{
    /// <summary>
    /// Helper class to get up and running with CSharpAnalytics in WindowsStore applications.
    /// Either use as-is by calling StartAsync, Attach and StopAsync from your App.xaml.cs or use as a
    /// starting point to wire up your own way.
    /// </summary>
    public class WindowsStoreAutoMeasurement : BaseAutoMeasurement
    {
        private DataTransferManager attachedDataTransferManager;
        private Frame attachedFrame;

        /// <summary>
        /// Attach to the root frame, hook into the navigation event and track initial screen view.
        /// Call this just before Window.Current.Activate() in your App.OnLaunched method.
        /// </summary>
        public void Attach(Frame frame)
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
                TrackScreenView(content.GetType());
        }

        protected override IEnvironment GetEnvironment()
        {
            return new WindowsStoreEnvironment();
        }

        /// <summary>
        /// Hook into various events to automatically track suspend, resume, page navigation,
        /// social sharing etc.
        /// </summary>
        protected override void HookEvents()
        {
            var application = Application.Current;
            application.Resuming += ApplicationOnResuming;
            application.Suspending += ApplicationOnSuspending;

            attachedDataTransferManager = DataTransferManager.GetForCurrentView();
            attachedDataTransferManager.TargetApplicationChosen += SocialShare;
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
            application.Resuming -= ApplicationOnResuming;
            application.Suspending -= ApplicationOnSuspending;
            attachedDataTransferManager.TargetApplicationChosen -= SocialShare;

            if (attachedFrame != null)
                attachedFrame.Navigated -= FrameNavigated;
            attachedFrame = null;
        }

        /// <summary>
        /// Handle application resuming from suspend without shutdown.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="o">Undocumented event parameter that is null.</param>
        private async void ApplicationOnResuming(object sender, object o)
        {
            await StartRequesterAsync();
        }

        /// <summary>
        /// Handle application suspending.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="suspendingEventArgs">Details about the suspending event.</param>
        private async void ApplicationOnSuspending(object sender, SuspendingEventArgs suspendingEventArgs)
        {
            var deferral = suspendingEventArgs.SuspendingOperation.GetDeferral();
            await StopRequesterAsync();
            deferral.Complete();
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
            TrackScreenView(e.SourcePageType);
        }

        private void SocialShare(DataTransferManager sender, TargetApplicationChosenEventArgs e)
        {
            Client.TrackEvent("Share", "Charms", e.ApplicationName);
        }

        protected override async Task SetupRequesterAsync()
        {
            var systemUserAgent = await WindowsStoreSystemInfo.GetSystemUserAgentAsync();

            var httpClientRequester = new HttpClientRequester();
            httpClientRequester.HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(ClientUserAgent);

            if (!String.IsNullOrEmpty(systemUserAgent))
                httpClientRequester.HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(systemUserAgent);

            Requester = httpClientRequester.Request;
        }

        /// <summary>
        /// Determine if the Internet is available at this point in time.
        /// </summary>
        /// <returns>True if the Internet is available, false otherwise.</returns>
        protected override bool IsInternetAvailable()
        {
            var internetProfile = NetworkInformation.GetInternetConnectionProfile();
            if (internetProfile == null) return false;

            switch (internetProfile.GetNetworkConnectivityLevel())
            {
                case NetworkConnectivityLevel.None:
                case NetworkConnectivityLevel.LocalAccess:
                    return false;
                default:
                    return true;
            }
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
        private static readonly WindowsStoreAutoMeasurement instance = new WindowsStoreAutoMeasurement();

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

        public static void Attach(Frame rootFrame)
        {
            instance.Attach(rootFrame);
        }

        public static void SetOptOut(bool optOut)
        {
            instance.SetOptOut(optOut);
        }

        public static void Start(MeasurementConfiguration measurementConfiguration, LaunchActivatedEventArgs args, TimeSpan? uploadInterval = null)
        {
            instance.Start(measurementConfiguration, args.Kind.ToString(), uploadInterval);
        }
    }
}