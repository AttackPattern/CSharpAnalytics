﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CSharpAnalytics.Protocols;
using CSharpAnalytics.Protocols.Urchin;
using CSharpAnalytics.Sessions;
using Windows.ApplicationModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CSharpAnalytics.WindowsStore
{
    /// <summary>
    /// Helper class to get up and running with CSharpAnalytics in WindowsStore applications.
    /// Either use as-is by calling StartAsync and StopAsync from your App.xaml.cs or use as a
    /// starting point to wire up your own way.
    /// </summary>
    public static class AutoAnalytics
    {
        private static readonly ProtocolDebugger urchinDebugger = new ProtocolDebugger(s => Debug.WriteLine(s), UrchinParameterDefinitions.All);

        private const string RequestQueueFileName = "CSharpAnalytics-RequestQueue";
        private const string SessionStateFileName = "CSharpAnalytics-SessionState";

        private static string currentAppName;
        private static BackgroundHttpRequester requester;
        private static SessionManager sessionManager;

        private static Frame attachedFrame;
        private static DataTransferManager attachedDataTransferManager;

        public static UrchinAnalyticsClient Client { get; private set; }

        /// <summary>
        /// Start CSharpAnalytics by restoring the session state, starting the background sender,
        /// hooking up events to track and firing the application start event and home page view to analytics.
        /// Call this just before Window.Current.Activate() in your App.OnLaunched method.
        /// </summary>
        /// <param name="configuration">Configuration to use, must at a minimum specify your Google Analytics ID and app name.</param>
        /// <param name="appName">Application name to use for the user agent tracking.</param>
        /// <param name="uploadInterval">How often to upload to the server. Lower times = more traffic but realtime. Defaults to 5 seconds.</param>
        /// <returns>A Task that will complete once CSharpAnalytics is available.</returns>
        /// <example>await AutoAnalytics.StartAsync(new Configuration("UA-123123123-1", "myapp.someco.com"));</example>
        public static async Task StartAsync(UrchinConfiguration configuration, string appName, TimeSpan? uploadInterval = null)
        {
            currentAppName = appName;

            await StartRequesterAsync(uploadInterval ?? TimeSpan.FromSeconds(5));
            await RestoreSessionAsync(configuration.SessionTimeout);

            Client = new UrchinAnalyticsClient(configuration, sessionManager, new WindowsStoreEnvironment(), requester.Add);
            Client.TrackEvent("ApplicationLifecycle", "Start");
            Client.TrackPageView("Home", "/");

            HookEvents(Application.Current);
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
            Client.TrackEvent("ApplicationLifecycle", "Stop");
            UnhookEvents(Application.Current);

            await SuspendRequesterAsync();
            await SaveSessionAsync();
        }

        /// <summary>
        /// Hook into various events to automatically track suspend, resume, page navigation,
        /// social sharing etc.
        /// </summary>
        /// <param name="application">Application object to hook into.</param>
        private static void HookEvents(Application application)
        {
            application.Resuming += applicationResume;
            application.Suspending += applicationSuspend;
            application.UnhandledException += applicationException;

            attachedFrame = Window.Current.Content as Frame;
            if (attachedFrame != null)
                attachedFrame.Navigated += FrameNavigated;

            attachedDataTransferManager = DataTransferManager.GetForCurrentView();
            attachedDataTransferManager.TargetApplicationChosen += socialShare;
        }

        /// <summary>
        /// Unhook events that were wired up in HookEvents.
        /// </summary>
        /// <param name="application">Application object to unhook from.</param>
        private static void UnhookEvents(Application application)
        {
            application.Resuming -= applicationResume;
            application.Suspending -= applicationSuspend;
            application.UnhandledException -= applicationException;

            if (attachedFrame != null)
                attachedFrame.Navigated -= FrameNavigated;

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
            if (e.Content is ITrackPageView) return;
            Client.TrackPageView(e.SourcePageType.Name, "/" + e.SourcePageType.Name);
        }

        private static readonly EventHandler<object> applicationResume = (sender, e) => Client.TrackEvent("ApplicationLifecycle", "Resume");
        private static readonly SuspendingEventHandler applicationSuspend = (sender, e) => Client.TrackEvent("ApplicationLifecycle", "Suspend");
        private static readonly UnhandledExceptionEventHandler applicationException = (sender, e) => Client.TrackEvent("UnhandledException", e.Exception.GetType().Name, e.Exception.Source);
        private static readonly TypedEventHandler<DataTransferManager, TargetApplicationChosenEventArgs> socialShare = (sender, e) => Client.TrackSocial("ShareCharm", e.ApplicationName);

        /// <summary>
        /// Start the requester with any unsent URIs from the last application run.
        /// </summary>
        /// <param name="uploadInterval">How often to send URIs to analytics.</param>
        /// <returns>Task that completes when the requester is ready.</returns>
        private static async Task StartRequesterAsync(TimeSpan uploadInterval)
        {
            requester = new BackgroundHttpRequester(PreprocessHttpRequest);
            var previousRequests = await LocalFolderContractSerializer<List<Uri>>.RestoreAsync(RequestQueueFileName);
            requester.Start(uploadInterval, previousRequests);
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
            requestMessage.Headers.UserAgent.Add(new ProductInfoHeaderValue(currentAppName, GetVersion(Package.Current.Id.Version)));
            DebugRequest(requestMessage);
        }

        [Conditional("DEBUG")]
        private static void DebugRequest(HttpRequestMessage requestMessage)
        {
            urchinDebugger.Examine(requestMessage.RequestUri);
        }

        private static string GetVersion(PackageVersion version)
        {
            return String.Join(".", version.Major, version.Minor, version.Revision, version.Build);
        }

        /// <summary>
        /// Suspend the requester and preserve any unsent URIs.
        /// </summary>
        /// <returns>Task that completes when the requester has been suspended.</returns>
        private static async Task SuspendRequesterAsync()
        {
            var pendingRequests = await requester.StopAsync();
            await LocalFolderContractSerializer<List<Uri>>.SaveAsync(pendingRequests, RequestQueueFileName);
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

    /// <summary>
    /// Implement this interface on any Pages in your application where you want
    /// to override the page titles or paths generated for that page by emitting them yourself at
    /// the end of the page's LoadState method.
    /// </summary>
    /// <remarks>
    /// This is especially useful for a page that obtains its content from a data source to
    /// track it as seperate virtual pages.
    /// </remarks>
    public interface ITrackPageView
    {
    }
}