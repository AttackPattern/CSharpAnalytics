﻿// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using CSharpAnalytics.Network;
using CSharpAnalytics.Protocols.Measurement;
using CSharpAnalytics.Sessions;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSharpAnalytics.SystemInfo;

namespace CSharpAnalytics
{
    /// <summary>
    /// Helper class to get up and running with CSharpAnalytics in Windows Forms applications.
    /// </summary>
    public class WinFormAutoMeasurement : BaseAutoMeasurement
    {
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int connDescription, int reservedValue);

        protected override void HookEvents()
        {
            Application.ApplicationExit += ApplicationOnApplicationExit;
        }

        private async void ApplicationOnApplicationExit(object sender, EventArgs eventArgs)
        {
            UnhookEvents();
            await StopRequesterAsync();
        }

        protected override void UnhookEvents()
        {
            Application.ApplicationExit -= ApplicationOnApplicationExit;
        }

        protected override IEnvironment GetEnvironment()
        {
            return new WinFormsEnvironment();;
        }

        protected override async Task<T> Load<T>()
        {
            return await AppDataContractSerializer.Restore<T>(Filenames[typeof(T)]);
        }

        protected override async Task Save<T>(T data)
        {
            await AppDataContractSerializer.Save(data, Filenames[typeof(T)]);
        }

        protected override void SetupRequester()
        {
            var httpClientRequester = new HttpClientRequester();
            httpClientRequester.HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(ClientUserAgent);

            var systemUserAgent = WindowsSystemInfo.GetSystemUserAgent();
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
            int connDesc;
            return InternetGetConnectedState(out connDesc, 0);
        }
    }

    public static class AutoMeasurement
    {
        private static readonly WinFormAutoMeasurement instance = new WinFormAutoMeasurement();

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

        public static void SetOptOut(bool optOut)
        {
            instance.SetOptOut(optOut);
        }

        public static void Start(MeasurementConfiguration measurementConfiguration, TimeSpan? uploadInterval = null)
        {
            instance.Start(measurementConfiguration, "", uploadInterval);
        }
    }
}