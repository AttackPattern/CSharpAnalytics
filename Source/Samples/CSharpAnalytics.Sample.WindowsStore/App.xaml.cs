﻿using System.Threading.Tasks;
using CSharpAnalytics.Activities;
using CSharpAnalytics.Protocols.Measurement;
using CSharpAnalytics.Protocols.Urchin;
using CSharpAnalytics.Sample.WindowsStore.Common;
using System;
using CSharpAnalytics.WindowsStore;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CSharpAnalytics.Sample.WindowsStore
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App
    {
        /// <summary>
        /// Initializes the singleton Application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            var timeLaunch = new AutoTimedEventActivity("ApplicationLifecycle", "Launching");
            var rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active

            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                // Associate the frame with a SuspensionManager key
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        // Something went wrong restoring state.
                        // Assume there is no state and continue
                    }
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(GroupedItemsPage), "AllGroups"))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // You choose *one* of these two techniques - NOT BOTH - depending on whether your property is a site or an app in GA.

            // AutoAnalytics currently uses Urchin API originally designed for web sites
            await AutoAnalytics.StartAsync(new UrchinConfiguration("UA-319000-10", "sample.csharpanalytics.com"));

            // AutoMeasurement uses the Measurement Protocol API that Google's Native SDKs for iOS and Android use
            await AutoMeasurement.StartAsync(new MeasurementConfiguration("UA-319000-8"));
            
            // Ensure the current window is active
            Window.Current.Activate();

            AutoAnalytics.Client.Track(timeLaunch);
            AutoMeasurement.Client.Track(timeLaunch);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private static async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await Task.WhenAll(SuspensionManager.SaveAsync(), AutoAnalytics.StopAsync(), AutoMeasurement.StopAsync());
            deferral.Complete();
        }
    }
}