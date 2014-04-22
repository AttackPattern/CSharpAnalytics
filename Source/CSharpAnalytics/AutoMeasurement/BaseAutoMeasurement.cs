﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using CSharpAnalytics.Environment;
using CSharpAnalytics.Network;
using CSharpAnalytics.Protocols;
using CSharpAnalytics.Protocols.Measurement;
using CSharpAnalytics.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CSharpAnalytics
{
    /// <summary>
    /// Helper class to get up and running with CSharpAnalytics in WindowsStore applications.
    /// Either use as-is by calling StartAsync, Attach and StopAsync from your App.xaml.cs or use as a
    /// starting point to wire up your own way.
    /// </summary>
    public abstract class BaseAutoMeasurement
    {
        protected Dictionary<Type, string> Filenames = new Dictionary<Type, string>
        {
            { typeof(SessionState), "CSharpAnalytics-MeasurementSession" },
            { typeof(List<Uri>),  "CSharpAnalytics-MeasurementQueue"}
        };

        private const string ApplicationLifecycleEvent = "ApplicationLifecycle";
        private const int MaximumRequestsToPersist = 60;

        private readonly ProtocolDebugger protocolDebugger = new ProtocolDebugger(MeasurementParameterDefinitions.All);
        private readonly MeasurementAnalyticsClient client = new MeasurementAnalyticsClient();

        private bool? delayedOptOut;
        private TimeSpan lastUploadInterval;
        private BackgroundUriRequester backgroundRequester;
        private SessionManager sessionManager;
        private bool isStarted;

        protected const string ClientUserAgent = "CSharpAnalytics/0.3";
        protected Func<Uri, CancellationToken, bool> Requester;

        /// <summary>
        /// Access to the MeasurementAnalyticsClient necessary to send additional events.
        /// </summary>
        public MeasurementAnalyticsClient Client { get { return client; } }

        /// <summary>
        /// Action to receive protocol debug output. 
        /// </summary>
        public Action<string> DebugWriter { get; set; }

        /// <summary>
        /// Initialize CSharpAnalytics by restoring the session state and starting the background sender and tracking
        /// the application lifecycle start event.
        /// </summary>
        /// <param name="configuration">Configuration to use, must at a minimum specify your Google Analytics ID and app name.</param>
        /// <param name="launchKind">Kind of launch this application experienced.</param>
        /// <param name="uploadInterval">How often to upload to the server. Lower times = more traffic but realtime. Defaults to 5 seconds.</param>
        /// <example>var analyticsTask = AutoMeasurement.StartAsync(new MeasurementConfiguration("UA-123123123-1", "MyApp", "1.0.1.0"));</example>
        public async void Start(MeasurementConfiguration configuration, string launchKind, TimeSpan? uploadInterval = null)
        {
            if (!isStarted)
            {
                isStarted = true;
                lastUploadInterval = uploadInterval ?? TimeSpan.FromSeconds(5);
                await StartRequesterAsync();

                var sessionState = await Load<SessionState>();
                sessionManager = new SessionManager(sessionState, configuration.SampleRate);
                if (delayedOptOut != null) SetOptOut(delayedOptOut.Value);

                Client.Configure(configuration, sessionManager, GetEnvironment(), Add);

                // Sometimes apps crash so preserve at least session number and visitor id on launch
                await Save(sessionManager.GetState());

                HookEvents();
            }

            Client.TrackEvent("Start", ApplicationLifecycleEvent, launchKind);
        }

        /// <summary>
        /// Hook into various events to automatically track suspend, resume, page navigation,
        /// social sharing etc.
        /// </summary>
        protected abstract void HookEvents();

        /// <summary>
        /// Unhook events that were wired up in HookEvents.
        /// </summary>
        /// <remarks>
        /// Not actually used in AutoMeasurement but here to show you what to do if you wanted to.
        /// </remarks>
        protected abstract void UnhookEvents();

        protected abstract IEnvironment GetEnvironment();

        /// <summary>
        /// Load the session state from storage if it exists, null if it does not.
        /// </summary>
        /// <returns>Task that completes when the SessionState is available.</returns>
        protected abstract Task<T> Load<T>();

        /// <summary>
        /// Save the session state to preserve state between application launches.
        /// </summary>
        /// <returns>Task that completes when the session state has been saved.</returns>
        protected abstract Task Save<T>(T data);

        protected abstract Task SetupRequesterAsync();
        protected abstract bool IsInternetAvailable();

        /// <summary>
        /// Opt the user in or out of analytics for this application install.
        /// </summary>
        /// <param name="optOut">True if the user is opting out, false if they are opting back in.</param>
        /// <remarks>
        /// This option persists automatically.
        /// You should call this only when the user changes their decision.
        /// </remarks>
        public async void SetOptOut(bool optOut)
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
                await Save(sessionManager.GetState());
            }
        }

        /// <summary>
        /// Internal status of this visitor.
        /// </summary>
        internal VisitorStatus VisitorStatus
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
        /// Track an app view if it does not track itself.
        /// </summary>
        /// <param name="page">Page to track in analytics.</param>
        protected void TrackScreenView(Type page)
        {
            if (typeof(ITrackOwnView).GetTypeInfo().IsAssignableFrom(page.GetTypeInfo())) return;

            var screenName = GetScreenName(page);
            Client.TrackScreenView(screenName);
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
        protected async Task StartRequesterAsync()
        {
            await SetupRequesterAsync();
            backgroundRequester = new BackgroundUriRequester(Request, IsInternetAvailable);

            var previousRequests = await Load<List<Uri>>();
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
        private bool Request(Uri uri, CancellationToken token)
        {
            if (sessionManager.VisitorStatus != VisitorStatus.Active)
                return true;

            uri = client.AdjustUriBeforeRequest(uri);
            protocolDebugger.Dump(uri, DebugWriter);

            return Requester(uri, token);
        }

        /// <summary>
        /// Suspend the requester and preserve any unsent URIs.
        /// </summary>
        /// <returns>Task that completes when the requester has been suspended.</returns>
        protected async Task StopRequesterAsync()
        {
            var safeBackgroundRequester = backgroundRequester;
            if (safeBackgroundRequester == null) return;

            var recentRequestsToPersist = new List<Uri>();
            if (safeBackgroundRequester.IsStarted)
            {
                var pendingRequests = await safeBackgroundRequester.StopAsync();
                recentRequestsToPersist = pendingRequests.Skip(pendingRequests.Count - MaximumRequestsToPersist).ToList();
            }

            await Save(recentRequestsToPersist);
            await Save(sessionManager.GetState());

            safeBackgroundRequester.Dispose();
            backgroundRequester = null;
        }

        /// <summary>
        /// Send the Uri request to the current background requester safely.
        /// </summary>
        private void Add(Uri uri)
        {
            var safeRequester = backgroundRequester;
            if (safeRequester != null)
                safeRequester.Add(uri);
        }
    }
}