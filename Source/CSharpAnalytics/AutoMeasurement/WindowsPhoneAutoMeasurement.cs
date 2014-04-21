﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System.Threading;
using CSharpAnalytics.Activities;
using CSharpAnalytics.Network;
using CSharpAnalytics.Protocols;
using CSharpAnalytics.Protocols.Measurement;
using CSharpAnalytics.Sessions;
using CSharpAnalytics.WindowsPhone;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Windows.Networking.Connectivity;
using Microsoft.Phone.Shell;

namespace CSharpAnalytics
{
    /// <summary>
    /// Helper class to get up and running with CSharpAnalytics in Winows Phone 8 applications.
    /// Either use as-is by calling StartAsync, Attach and StopAsync from your App.xaml.cs or use as a
    /// starting point to wire up your own way.
    /// </summary>
    public static class AutoMeasurement
    {
        private const string ApplicationLifecycleEvent = "ApplicationLifecycle";
        private const string RequestQueueFileName = "CSharpAnalytics-MeasurementQueue";
        private const string SessionStateFileName = "CSharpAnalytics-MeasurementSession";
        private const int MaximumRequestsToPersist = 60;

        private static readonly ProtocolDebugger protocolDebugger = new ProtocolDebugger(MeasurementParameterDefinitions.All);
        private static readonly MeasurementAnalyticsClient client = new MeasurementAnalyticsClient();
        private static readonly string[] clientUserAgent = { "CSharpAnalytics", "0.2" };

        private static PhoneApplicationFrame attachedFrame;
        private static bool? delayedOptOut;
        private static TimeSpan lastUploadInterval;
        private static BackgroundUriRequester backgroundRequester;
        private static HttpWebRequester requester;
        private static SessionManager sessionManager;
        private static string systemUserAgent;
        private static bool isStarted;

        /// <summary>
        /// Access to the MeasurementAnalyticsClient necessary to send additional events.
        /// </summary>
        public static MeasurementAnalyticsClient Client { get { return client; } }

        /// <summary>
        /// Action to receive protocol debug output. 
        /// </summary>
        public static Action<string> DebugWriter { get; set; }

        /// <summary>
        /// Initialize CSharpAnalytics by restoring the session state and starting the background sender and tracking
        /// the application lifecycle start event.
        /// </summary>
        /// <param name="configuration">Configuration to use, must at a minimum specify your Google Analytics ID and app name.</param>
        /// <param name="launchArgs">Launch arguments from your Application OnLaunched to determine how the app was launched.</param>
        /// <param name="uploadInterval">How often to upload to the server. Lower times = more traffic but realtime. Defaults to 5 seconds.</param>
        /// <example>var analyticsTask = AutoMeasurement.StartAsync(new MeasurementConfiguration("UA-123123123-1", "MyApp", "1.0.0.0"));</example>
        public static async void Start(MeasurementConfiguration configuration, LaunchingEventArgs launchArgs, TimeSpan? uploadInterval = null)
        {
            if (!isStarted)
            {
                isStarted = true;
                lastUploadInterval = uploadInterval ?? TimeSpan.FromSeconds(5);
                systemUserAgent = GetSystemUserAgent();
                await StartRequesterAsync();

                var sessionState = await LoadSessionState();
                sessionManager = new SessionManager(sessionState, configuration.SampleRate);
                if (delayedOptOut != null) SetOptOut(delayedOptOut.Value);

                Client.Configure(configuration, sessionManager, new WindowsPhoneEnvironment(), Add);
                HookEvents();
            }

            var launchReason = launchArgs == null
                ? ""
                : launchArgs.GetType().Name.Replace("LaunchingEventArgs", "");
            Client.TrackEvent("Start", ApplicationLifecycleEvent, launchReason);
        }

        /// <summary>
        /// Opt the user in or out of analytics for this application install.
        /// </summary>
        /// <param name="optOut">True if the user is opting out, false if they are opting back in.</param>
        /// <remarks>
        /// This option persists automatically.
        /// You should call this only when the user changes their decision.
        /// </remarks>
        public static async void SetOptOut(bool optOut)
        {
            if (sessionManager == null)
            {
                delayedOptOut = optOut;
                return;
            }
            delayedOptOut = null;

            if (sessionManager.VisitorStatus == VisitorStatus.SampledOut) return;

            var newVisitorStatus = optOut ? VisitorStatus.OptedOut : VisitorStatus.Active;
            if (newVisitorStatus != sessionManager.VisitorStatus)
            {
                System.Diagnostics.Debug.WriteLine("Switching VisitorStatus from {0} to {1}", sessionManager.VisitorStatus, newVisitorStatus);
                sessionManager.VisitorStatus = newVisitorStatus;
                await SaveSessionState(sessionManager.GetState());
            }
        }

        /// <summary>
        /// Attach to the root frame, hook into the navigation event and track initial page appview.
        /// Call this just before Window.Current.Activate() in your App.OnLaunched method.
        /// </summary>
        public static void Attach(PhoneApplicationFrame frame)
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
                TrackFrameNavigate(content.GetType());
        }

        /// <summary>
        /// Internal status of this visitor.
        /// </summary>
        internal static VisitorStatus VisitorStatus
        {
            get
            {
                // Allow AnalyticsUserOption to function at design time.
                if (sessionManager == null)
                    return delayedOptOut == true ? VisitorStatus.OptedOut : VisitorStatus.Active;

                return sessionManager.VisitorStatus;
            }
        }

        /// <summary>
        /// Hook into various events to automatically track suspend, resume, page navigation,
        /// social sharing etc.
        /// </summary>
        private static void HookEvents()
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
        private static async void ApplicationOnStartup(object sender, StartupEventArgs e)
        {
            await StartRequesterAsync();
            Client.TrackEvent("Start", ApplicationLifecycleEvent);
        }

        /// <summary>
        /// Handle application suspending.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Empty event information.</param>
        private static async void ApplicationOnExit(object sender, EventArgs e)
        {
            Client.Track(new EventActivity("Exit", ApplicationLifecycleEvent), true); // Stop the session
            await SuspendRequesterAsync();
        }

        /// <summary>
        /// Unhook events that were wired up in HookEvents.
        /// </summary>
        /// <remarks>
        /// Not actually used in AutoMeasurement but here to show you what to do if you wanted to.
        /// </remarks>
        private static void UnhookEvents()
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
        private static void FrameNavigated(object sender, NavigationEventArgs e)
        {
            if (e.Content != null)
                TrackFrameNavigate(e.Content.GetType());
        }

        /// <summary>
        /// Track an app view if it does not track itself.
        /// </summary>
        /// <param name="page">Page to track in analytics.</param>
        private static void TrackFrameNavigate(Type page)
        {
            if (typeof(ITrackOwnView).GetTypeInfo().IsAssignableFrom(page.GetTypeInfo())) return;

            var screenName = GetScreenName(page);
            Client.TrackAppView(screenName);
        }

        /// <summary>
        /// Determine the screen name of a page to track.
        /// </summary>
        /// <param name="page">Page within the application to track.</param>
        /// <returns>String for the screen name in analytics.</returns>
        private static string GetScreenName(Type page)
        {
            var screenNameAttribute = page.GetTypeInfo().GetCustomAttribute(typeof(AnalyticsScreenNameAttribute)) as AnalyticsScreenNameAttribute;
            if (screenNameAttribute != null)
                return screenNameAttribute.ScreenName;

            var screenName = page.Name;
            if (screenName.EndsWith("Page"))
                screenName = screenName.Substring(0, screenName.Length - 4);
            return screenName;
        }

        /// <summary>
        /// Start the requester with any unsent URIs from the last application run.
        /// </summary>
        /// <returns>Task that completes when the requester is ready.</returns>
        private static async Task StartRequesterAsync()
        {
            requester = new HttpWebRequester(systemUserAgent);

            backgroundRequester = new BackgroundUriRequester(Request, IsInternetAvailable);
            var previousRequests = await LocalFolderContractSerializer<List<Uri>>.RestoreAsync(RequestQueueFileName);
            backgroundRequester.Start(lastUploadInterval, previousRequests);
        }

        /// <summary>
        /// Act as a middleman between the background sender and the actual http client sender
        /// so we can drop opt-out or sampled out requests already in the queue, adjust the uri
        /// for queue times and optionally debug them.
        /// </summary>
        /// <param name="uri">Uri to modify or inspect before it is sent.</param>
        /// <param name="token">CancellationToken to cancel any network request, e.g. if shutting down.</param>
        /// <remarks>
        /// Because user agent is not persisted unsent URIs that are saved and then sent after an upgrade
        /// will have the new user agent string not the actual one that generated them.
        /// </remarks>
        private static bool Request(Uri uri, CancellationToken token)
        {
            if (sessionManager.VisitorStatus != VisitorStatus.Active)
                return true;

            uri = client.AdjustUriBeforeRequest(uri);
            protocolDebugger.Dump(uri, DebugWriter);

            return requester.Request(uri, token);
        }

        /// <summary>
        /// Determine if the Internet is available at this point in time.
        /// </summary>
        /// <returns>True if the Internet is available, false otherwise.</returns>
        private static bool IsInternetAvailable()
        {
            var internetProfile = NetworkInformation.GetInternetConnectionProfile();
            if (internetProfile == null) return false;

            // Don't send analytics if data limit is close/over or they are roaming
            var cost = internetProfile.GetConnectionCost();
            return !cost.ApproachingDataLimit && !cost.OverDataLimit && !cost.Roaming;
        }

        /// <summary>
        /// Suspend the requester and preserve any unsent URIs.
        /// </summary>
        /// <returns>Task that completes when the requester has been suspended.</returns>
        private static async Task SuspendRequesterAsync()
        {
            var recentRequestsToPersist = new List<Uri>();
            if (backgroundRequester.IsStarted)
            {
                var pendingRequests = await backgroundRequester.StopAsync();
                recentRequestsToPersist = pendingRequests.Skip(pendingRequests.Count - MaximumRequestsToPersist).ToList();
            }
            await LocalFolderContractSerializer<List<Uri>>.SaveAsync(recentRequestsToPersist, RequestQueueFileName);
            await SaveSessionState(sessionManager.GetState());
        }

        /// <summary>
        /// Load the session state from storage if it exists, null if it does not.
        /// </summary>
        /// <returns>Task that completes when the SessionState is available.</returns>
        private static async Task<SessionState> LoadSessionState()
        {
            return await LocalFolderContractSerializer<SessionState>.RestoreAsync(SessionStateFileName);
        }

        /// <summary>
        /// Save the session state to preserve state between application launches.
        /// </summary>
        /// <returns>Task that completes when the session state has been saved.</returns>
        private static async Task SaveSessionState(SessionState sessionState)
        {
            await LocalFolderContractSerializer<SessionState>.SaveAsync(sessionState, SessionStateFileName);
        }

        /// <summary>
        /// Send the Uri request to the current background requester safely.
        /// </summary>
        private static void Add(Uri uri)
        {
            var safeRequester = backgroundRequester;
            if (safeRequester != null)
                safeRequester.Add(uri);
        }

        /// <summary>
        /// Get the Windows version number and processor architecture and cache it
        /// as a user agent string so it can be sent with HTTP requests.
        /// </summary>
        /// <returns>String containing formatted system parts of the user agent.</returns>
        private static string GetSystemUserAgent()
        {
            try
            {
                var osVersion = System.Environment.OSVersion.Version;
                var parts = new[] {
                    "Windows Phone " + osVersion.Major + "." + osVersion.Minor,
                    "ARM",
                    "Touch",
                    DeviceStatus.DeviceManufacturer,
                    DeviceStatus.DeviceName
                };

                return "(" + String.Join("; ", parts.Where(e => !String.IsNullOrEmpty(e))) + ")";
            }
            catch
            {
                return "";
            }
        }
    }
}