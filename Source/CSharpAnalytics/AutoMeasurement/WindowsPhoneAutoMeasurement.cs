﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using CSharpAnalytics.Network;
using CSharpAnalytics.Protocols;
using CSharpAnalytics.Protocols.Measurement;
using CSharpAnalytics.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Networking.Connectivity;

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
        private static readonly TypedEventHandler<DataTransferManager, TargetApplicationChosenEventArgs> socialShare = (sender, e) => Client.TrackEvent("Share", "Charms", e.ApplicationName);
        private static readonly MeasurementAnalyticsClient client = new MeasurementAnalyticsClient();
        private static readonly ProductInfoHeaderValue clientUserAgent = new ProductInfoHeaderValue("CSharpAnalytics", "0.2");

        private static DataTransferManager attachedDataTransferManager;
        private static bool? delayedOptOut;
        private static TimeSpan lastUploadInterval;
        private static BackgroundHttpRequester requester;
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
        public static async void Start(MeasurementConfiguration configuration, IActivatedEventArgs launchArgs, TimeSpan? uploadInterval = null)
        {
            if (!isStarted)
            {
                isStarted = true;
                lastUploadInterval = uploadInterval ?? TimeSpan.FromSeconds(5);
                systemUserAgent = await WindowsStoreSystemInformation.GetSystemUserAgent();
                await StartRequesterAsync();

                var sessionState = await LoadSessionState();
                sessionManager = new SessionManager(sessionState, configuration.SampleRate);
                if (delayedOptOut != null) SetOptOut(delayedOptOut.Value);

                Client.Configure(configuration, sessionManager, new WindowsStoreEnvironment(), Add);
                HookEvents();
            }

            Client.TrackEvent("Start", ApplicationLifecycleEvent, launchArgs.Kind.ToString());
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
            application.Resuming += ApplicationOnResuming;
            application.Suspending += ApplicationOnSuspending;

            attachedDataTransferManager = DataTransferManager.GetForCurrentView();
            attachedDataTransferManager.TargetApplicationChosen += socialShare;
        }

        /// <summary>
        /// Handle application resuming from suspend without shutdown.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="o">Undocumented event parameter that is null.</param>
        private static async void ApplicationOnResuming(object sender, object o)
        {
            await StartRequesterAsync();
        }

        /// <summary>
        /// Handle application suspending.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="suspendingEventArgs">Details about the suspending event.</param>
        private static async void ApplicationOnSuspending(object sender, SuspendingEventArgs suspendingEventArgs)
        {
            var deferral = suspendingEventArgs.SuspendingOperation.GetDeferral();
            await SuspendRequesterAsync();
            deferral.Complete();
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
            application.Resuming -= ApplicationOnResuming;
            application.Suspending -= ApplicationOnSuspending;
            attachedDataTransferManager.TargetApplicationChosen -= socialShare;
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
            requester = new BackgroundHttpClientRequester(PreprocessHttpRequest, IsInternetAvailable);
            var previousRequests = await LocalFolderContractSerializer<List<Uri>>.RestoreAsync(RequestQueueFileName);
            requester.Start(lastUploadInterval, previousRequests);
        }

        /// <summary>
        /// Determine if the Internet is available at this point in time.
        /// </summary>
        /// <returns>True if the Internet is available, false otherwise.</returns>
        private static bool IsInternetAvailable()
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
        /// Pre-process the HttpRequestMessage before it is sent. This includes adding the user agent for tracking
        /// and for debug builds writing out the debug information to the console log.
        /// </summary>
        /// <param name="requestMessage">HttpRequestMessage to modify or inspect before it is sent.</param>
        /// <remarks>
        /// Because user agent is not persisted unsent URIs that are saved and then sent after an upgrade
        /// will have the new user agent string not the actual one that generated them.
        /// </remarks>
        private static void PreprocessHttpRequest(HttpRequestMessage requestMessage)
        {
            if (sessionManager.VisitorStatus != VisitorStatus.Active)
            {
                requestMessage.RequestUri = null;
                return;
            }

            requestMessage.RequestUri = client.AdjustUriBeforeRequest(requestMessage.RequestUri);
//            AddUserAgent(requestMessage.Headers.UserAgent);
            DebugRequest(requestMessage);
        }


        /// <summary>
        /// Send the HttpRequestMessage with the protocol debugger for examination.
        /// </summary>
        /// <param name="requestMessage">HttpRequestMessage to examine with the protocol debugger.</param>
        private async static void DebugRequest(HttpRequestMessage requestMessage)
        {
            var payloadUri = await RejoinPayload(requestMessage);
            protocolDebugger.Dump(payloadUri, DebugWriter);
        }

        /// <summary>
        /// Rejoin the POST body payload with the Uri parameter if necessary so it can be sent to the
        /// protocol debugger.
        /// </summary>
        /// <param name="requestMessage">HttpRequestMessage to obtain complete payload for.</param>
        /// <returns>Uri with final payload to be sent.</returns>
        private async static Task<Uri> RejoinPayload(HttpRequestMessage requestMessage)
        {
            if (requestMessage.Content == null)
                return requestMessage.RequestUri;

            var bodyPayload = await requestMessage.Content.ReadAsStringAsync();
            return new UriBuilder(requestMessage.RequestUri) { Query = bodyPayload }.Uri;
        }

        /// <summary>
        /// Suspend the requester and preserve any unsent URIs.
        /// </summary>
        /// <returns>Task that completes when the requester has been suspended.</returns>
        private static async Task SuspendRequesterAsync()
        {
            var recentRequestsToPersist = new List<Uri>();
            if (requester.IsStarted)
            {
                var pendingRequests = await requester.StopAsync();
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
            var safeRequester = requester;
            if (safeRequester != null)
                safeRequester.Add(uri);
        }
    }
}