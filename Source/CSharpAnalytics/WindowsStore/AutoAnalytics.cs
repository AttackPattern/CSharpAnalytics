﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using CSharpAnalytics.Activities;
using CSharpAnalytics.Network;
using CSharpAnalytics.Protocols;
using CSharpAnalytics.Protocols.Urchin;
using CSharpAnalytics.Sessions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CSharpAnalytics.WindowsStore
{
    /// <summary>
    /// Helper class to get up and running with CSharpAnalytics in WindowsStore applications.
    /// Either use as-is by calling StartAsync, Attach and StopAsync from your App.xaml.cs or use as a
    /// starting point to wire up your own way.
    /// </summary>
    [Obsolete("Switch to using AutoMeasurement and Universal app properties")]
    public static class AutoAnalytics
    {
        private const string RequestQueueFileName = "CSharpAnalytics-RequestQueue";
        private const string SessionStateFileName = "CSharpAnalytics-SessionState";
        private const int MaximumRequestsToPersist = 50;

        private static readonly ProtocolDebugger protocolDebugger = new ProtocolDebugger(s => Debug.WriteLine(s), UrchinParameterDefinitions.All);
        private static readonly TypedEventHandler<DataTransferManager, TargetApplicationChosenEventArgs> socialShare = (sender, e) => Client.TrackSocial("ShareCharm", e.ApplicationName);
        private static readonly UrchinAnalyticsClient client = new UrchinAnalyticsClient();

        private static string[] agentParts;
        private static BackgroundHttpRequester requester;
        private static SessionManager sessionManager;
        private static Frame attachedFrame;
        private static DataTransferManager attachedDataTransferManager;
        private static TimeSpan lastUploadInterval;

        /// <summary>
        /// Access to the UrchinAnalyticsClient necessary to send additional events.
        /// </summary>
        public static UrchinAnalyticsClient Client { get { return client; } }

        /// <summary>
        /// Initialize CSharpAnalytics by restoring the session state and starting the background sender and tracking
        /// the application lifecycle start event.
        /// </summary>
        /// <param name="configuration">Configuration to use, must at a minimum specify your Google Analytics ID and app name.</param>
        /// <param name="uploadInterval">How often to upload to the server. Lower times = more traffic but realtime. Defaults to 5 seconds.</param>
        /// <example>await AutoAnalytics.StartAsync(new UrchinConfiguration("UA-123123123-1", "myapp.someco.com"));</example>
        public static async void StartAsync(UrchinConfiguration configuration, TimeSpan? uploadInterval = null)
        {
            lastUploadInterval = uploadInterval ?? TimeSpan.FromSeconds(5);
            await StartRequesterAsync();
            await RestoreSessionAsync(TimeSpan.FromMinutes(20));

            Client.Configure(configuration, sessionManager, new WindowsStoreEnvironment(), requester.Add);
            Client.TrackEvent("Start", "ApplicationLifecycle");

            HookEvents();
        }

        /// <summary>
        /// Attach to the root frame, hook into the navigation event and track initial page appview.
        /// Call this just before Window.Current.Activate() in your App.OnLaunched method.
        /// </summary>
        public static void Attach(Frame frame)
        {
            if (frame == null)
                throw new ArgumentNullException("frame");

            if (frame != attachedFrame)
            {
                if (attachedFrame != null) attachedFrame.Navigated -= FrameNavigated;
                frame.Navigated += FrameNavigated;
                attachedFrame = frame;
            }

            if (frame.Content != null)
                TrackFrameNavigate(frame.Content.GetType().Name);
        }

        /// <summary>
        /// Stop CSharpAnalytics by firing the analytics event, unhooking events and saving the session
        /// state and pending queue.
        /// Call this in your App.OnSuspending just before deferral.Complete(); 
        /// </summary>
        /// <returns>A Task that will complete once CSharpAnalytics is available.</returns>
        /// <remarks>await AutoAnalytics.StopAsync();</remarks>
        public static async Task StopAsync()
        {
            Debug.Assert(Client != null);
            if (Client == null) return;

            Client.TrackEvent("Stop", "ApplicationLifecycle");
            UnhookEvents();

            await SuspendRequesterAsync();
            await SaveSessionAsync();
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
            Client.Track(new EventActivity("Resume", "ApplicationLifecycle"));
        }

        /// <summary>
        /// Handle application suspending.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="suspendingEventArgs">Details about the suspending event.</param>
        private static async void ApplicationOnSuspending(object sender, SuspendingEventArgs suspendingEventArgs)
        {
            var deferral = suspendingEventArgs.SuspendingOperation.GetDeferral();
            Client.Track(new EventActivity("Suspend", "ApplicationLifecycle"));
            await SuspendRequesterAsync();
            await SaveSessionAsync();
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

            if (attachedFrame != null)
                attachedFrame.Navigated -= FrameNavigated;
            attachedFrame = null;

            attachedDataTransferManager.TargetApplicationChosen -= socialShare;
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
            if (e.Content is ITrackOwnView) return;
            Client.TrackPageView(e.SourcePageType.Name, "/" + e.SourcePageType.Name);
        }

        /// <summary>
        /// Track an page view stripping the Page suffix from the end of the name.
        /// </summary>
        /// <param name="name">Name of the page to track.</param>
        private static void TrackFrameNavigate(string name)
        {
            if (name.EndsWith("Page"))
                name = name.Substring(0, name.Length - 4);
            Client.TrackPageView(name, "/" + name);
        }

        /// <summary>
        /// Start the requester with any unsent URIs from the last application run.
        /// </summary>
        /// <returns>Task that completes when the requester is ready.</returns>
        private static async Task StartRequesterAsync()
        {
            requester = new BackgroundHttpClientRequester(PreprocessHttpRequest);
            var previousRequests = await LocalFolderContractSerializer<List<Uri>>.RestoreAsync(RequestQueueFileName);
            requester.Start(lastUploadInterval, previousRequests);
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
            AddUserAgent(requestMessage.Headers.UserAgent, Package.Current.Id);
            DebugRequest(requestMessage);
        }

        /// <summary>
        /// Figure out the user agent and add it to the header collection.
        /// </summary>
        /// <param name="userAgent">User agent header collection.</param>
        /// <param name="packageId">PackageId to extract application name and version from.</param>
        private static async void AddUserAgent(ICollection<ProductInfoHeaderValue> userAgent, PackageId packageId)
        {
            userAgent.Add(new ProductInfoHeaderValue(packageId.Name, FormatVersion(packageId.Version)));

            agentParts = agentParts ?? new[] {
                "Windows NT " + await SystemInformation.GetWindowsVersionAsync(),
                await GetProcessorArchitectureAsync()
            };

            userAgent.Add(new ProductInfoHeaderValue("(" + String.Join("; ", agentParts) + ")"));
        }

        /// <summary>
        /// Determine the current processor architecture string for the user agent.
        /// </summary>
        /// <remarks>
        /// The strings this returns should be compatible with web browser user agent
        /// processor strings.
        /// </remarks>
        /// <returns>String containing the processor architecture.</returns>
        private static async Task<string> GetProcessorArchitectureAsync()
        {
            switch (await SystemInformation.GetProcessorArchitectureAsync())
            {
                case ProcessorArchitecture.X64:
                    return "x64";
                case ProcessorArchitecture.Arm:
                    return "ARM";
                default:
                    return "";
            }
        }

        /// <summary>
        /// Get the formatted version number for a PackageVersion.
        /// </summary>
        /// <param name="version">PackageVersion to format.</param>
        /// <returns>Formatted version number of the PackageVersion.</returns>
        private static string FormatVersion(PackageVersion version)
        {
            return String.Join(".", version.Major, version.Minor, version.Revision, version.Build);
        }

        /// <summary>
        /// Send the HttpRequestMessage with the protocol debugger for examination.
        /// </summary>
        /// <param name="requestMessage">HttpRequestMessage to examine with the protocol debugger.</param>
        [Conditional("DEBUG")]
        private async static void DebugRequest(HttpRequestMessage requestMessage)
        {
            var payloadUri = await RejoinPayload(requestMessage);
            protocolDebugger.Examine(payloadUri);
        }

        /// <summary>
        /// Rejoin the POST body payload with the Uri parameter if necessary so it can be sent to the
        /// protocol debugger.
        /// </summary>
        /// <param name="requestMessage">HttpRequestMessage to obtain complete payload for.</param>
        /// <returns>Uri with final payload to be sent.</returns>
        private async static Task<Uri> RejoinPayload(HttpRequestMessage requestMessage)
        {
            if (requestMessage.Content == null) return requestMessage.RequestUri;

            var bodyPayload = await requestMessage.Content.ReadAsStringAsync();
            return new UriBuilder(requestMessage.RequestUri) { Query = bodyPayload }.Uri;
        }

        /// <summary>
        /// Suspend the requester and preserve any unsent URIs.
        /// </summary>
        /// <returns>Task that completes when the requester has been suspended.</returns>
        private static async Task SuspendRequesterAsync()
        {
            var pendingRequests = await requester.StopAsync();
            var recentRequestsToPersist = pendingRequests.Skip(pendingRequests.Count - MaximumRequestsToPersist).ToList();
            await LocalFolderContractSerializer<List<Uri>>.SaveAsync(recentRequestsToPersist, RequestQueueFileName);
        }

        /// <summary>
        /// Restores the session manager using saved session state or creates a brand new visitor if none exists.
        /// </summary>
        /// <param name="sessionTimeout">How long a session can be inactive for before it ends.</param>
        /// <returns>Task that completes when the SessionManager is ready.</returns>
        private static async Task RestoreSessionAsync(TimeSpan sessionTimeout)
        {
            var sessionState = await LocalFolderContractSerializer<SessionState>.RestoreAsync(SessionStateFileName);
            sessionManager = new SessionManager(sessionTimeout, sessionState);
            await SaveSessionAsync();
        }

        /// <summary>
        /// Save the session to ensure state is preseved across application launches.
        /// </summary>
        /// <returns>Task that completes when the session has been saved.</returns>
        private static async Task SaveSessionAsync()
        {
            await LocalFolderContractSerializer<SessionState>.SaveAsync(sessionManager.GetState(), SessionStateFileName);
        }
    }
}