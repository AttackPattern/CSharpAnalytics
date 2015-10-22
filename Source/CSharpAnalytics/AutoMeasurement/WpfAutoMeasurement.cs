// Copyright (c) Attack Pattern LLC.  All rights reserved.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0

using System.Windows;
using CSharpAnalytics.Environment;
using CSharpAnalytics.Network;
using CSharpAnalytics.Serializers;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CSharpAnalytics.SystemInfo;

namespace CSharpAnalytics
{
    /// <summary>
    /// Helper class to get up and running with CSharpAnalytics in Windows Forms applications.
    /// </summary>
    public class WpfAutoMeasurement : BaseAutoMeasurement
    {
        public WpfAutoMeasurement()
        {
            if (Application.Current == null)
            {
                throw new NullReferenceException(
                    "System.Windows.Application.Current is null. Make sure you are running a WPF application.");
            }
        }

        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int connDescription, int reservedValue);

        /// <summary>
        /// Hook into various events to automatically track suspend, resume, page navigation,
        /// social sharing etc.
        /// </summary>
        protected override void HookEvents()
        {
            Application.Current.Exit += ApplicationOnExit;
        }

        /// <summary>
        /// Unhook events that were wired up in HookEvents.
        /// </summary>
        protected override void UnhookEvents()
        {
            Application.Current.Exit -= ApplicationOnExit;
        }

        /// <summary>
        /// Get the environment details for this system.
        /// </summary>
        /// <returns>
        /// IEnvironment implementation for getting screen, language and other system details.
        /// </returns>
        protected override IEnvironment GetEnvironment()
        {
            return new WpfEnvironment();
        }

        /// <summary>
        /// Load the data object from storage with the given name.
        /// </summary>
        /// <typeparam name="T">Type of data to load from storage.</typeparam>
        /// <param name="name">Name of the data in storage.</param>
        /// <returns>Instance of T containing the loaded data or null if did not exist.</returns>
        protected override async Task<T> Load<T>(string name)
        {
            return await AppDataContractSerializer.Restore<T>(name);
        }

        /// <summary>
        /// Save the data object to storage with the given name overwriting if required.
        /// </summary>
        /// <typeparam name="T">Type of data object to persist.</typeparam>
        /// <param name="data">Data object to persist.</param>
        /// <param name="name">Name to give to the object in storage.</param>
        /// <returns>Task that is complete when the data has been saved to storage.</returns>
        protected override async Task Save<T>(T data, string name)
        {
            await AppDataContractSerializer.Save(data, name);
        }

        /// <summary>
        /// Setup the Uri requester complete with user agent etc.
        /// </summary>
        /// <returns>Task that completes when the requester is ready to use.</returns>
        protected override Task SetupRequesterAsync()
        {
            var httpClientRequester = new HttpClientRequester();
            httpClientRequester.HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(ClientUserAgent);

            var systemUserAgent = WindowsSystemInfo.GetSystemUserAgent();
            if (!String.IsNullOrEmpty(systemUserAgent))
                httpClientRequester.HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(systemUserAgent);

            Requester = httpClientRequester.Request;

            return Task.FromResult(true);
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

        /// <summary>
        /// Handle the application exiting.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void ApplicationOnExit(object sender, EventArgs eventArgs)
        {
            UnhookEvents();
            StopRequesterAsync();
        }
    }
}